/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: https://catlib.io/
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using CatLib.Container;
using CatLib.EventDispatcher;
using CatLib.Exception;
using CatLib.Util;

namespace CatLib
{
    /// <summary>
    /// The CatLib <see cref="Application"/> instance.
    /// </summary>
    public class Application : Container.Container, IApplication
    {
        private static string version;
        private readonly IList<IServiceProvider> loadedProviders;
        private readonly IList<Action<IApplication>> bootingCallbacks;
        private readonly IList<Action<IApplication>> bootedCallbacks;
        private readonly IList<Action<IApplication>> terminatingCallbacks;
        private readonly int mainThreadId;
        private bool bootstrapped;
        private bool registering;
        private long incrementId;
        private DebugLevel debugLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="global">True if sets the instance to <see cref="App"/> facade.</param>
        public Application()
        {
            bootingCallbacks = new List<Action<IApplication>>();
            bootedCallbacks = new List<Action<IApplication>>();
            terminatingCallbacks = new List<Action<IApplication>>();
            loadedProviders = new List<IServiceProvider>();

            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            RegisterBaseBindings();
            RegisterBaseProviders();

            // We use closures to save the current context state
            // Do not change to: OnFindType(Type.GetType) This
            // causes the active assembly to be not the expected scope.
            OnFindType(finder => { return Type.GetType(finder); });

            DebugLevel = DebugLevel.Production;
            Process = StartProcess.Construct;
        }

        /// <summary>
        /// Gets the CatLib <see cref="Application"/> version.
        /// </summary>
        public static string Version => version ?? (version = FileVersionInfo
                       .GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);

        /// <summary>
        /// Gets indicates the application startup process.
        /// </summary>
        public StartProcess Process { get; private set; }

        /// <summary>
        /// Gets a value indicating whether if the application has booted.
        /// </summary>
        public bool IsBooted { get; private set; }

        /// <inheritdoc />
        public bool IsMainThread => mainThreadId == Thread.CurrentThread.ManagedThreadId;

        /// <inheritdoc />
        public DebugLevel DebugLevel
        {
            get => debugLevel;
            set
            {
                debugLevel = value;
                this.Instance<DebugLevel>(debugLevel);
            }
        }

        /// <inheritdoc cref="Application(bool)"/>
        /// <returns>The CatLib <see cref="Application"/> instance.</returns>
        public static Application New(bool global = true)
        {
            var application = new Application();
            if (global)
            {
                App.That = application;
            }

            return application;
        }

        /// <inheritdoc />
        public void Booting(Action<IApplication> callback)
        {
            bootingCallbacks.Add(callback);
        }

        /// <inheritdoc />
        public void Booted(Action<IApplication> callback)
        {
            bootedCallbacks.Add(callback);

            if (IsBooted)
            {
                RaiseAppCallbacks(new[] { callback });
            }
        }

        /// <inheritdoc />
        public virtual void Terminate()
        {
            if (Process == StartProcess.Terminate || Process == StartProcess.Terminated)
            {
                return;
            }

            Process = StartProcess.Terminate;

            Flush();
            if (App.That == this)
            {
                App.That = null;
            }

            RaiseAppCallbacks(terminatingCallbacks);
            Process = StartProcess.Terminated;
        }

        /// <inheritdoc />
        public void Terminating(Action<IApplication> callback)
        {
            terminatingCallbacks.Add(callback);
        }

        /// <summary>
        /// Bootstrap the given array of bootstrap classes.
        /// </summary>
        /// <param name="bootstraps">The given bootstrap classes.</param>
        public virtual void BootstrapWith(params IBootstrap[] bootstraps)
        {
            Guard.Requires<ArgumentNullException>(bootstraps != null);

            if (bootstrapped || Process != StartProcess.Construct)
            {
                throw new LogicException($"Cannot repeatedly trigger the {nameof(BootstrapWith)}()");
            }

            Process = StartProcess.Bootstrap;
            var existed = new HashSet<IBootstrap>();

            foreach (var bootstrap in bootstraps)
            {
                if (bootstrap == null)
                {
                    continue;
                }

                if (existed.Contains(bootstrap))
                {
                    throw new LogicException($"The bootstrap already exists : {bootstrap}");
                }

                existed.Add(bootstrap);
                bootstrap.Bootstrap();
            }

            bootstrapped = true;
        }

        /// <summary>
        /// Boot all of the registered service provider.
        /// </summary>
        public virtual void Boot()
        {
            if (!bootstrapped)
            {
                throw new LogicException($"You must call {nameof(BootstrapWith)}() first.");
            }

            if (IsBooted || Process != StartProcess.Bootstrap)
            {
                throw new LogicException($"Cannot repeatedly trigger the {nameof(Boot)}()");
            }

            Process = StartProcess.Boot;

            RaiseAppCallbacks(bootingCallbacks);

            foreach (var provider in loadedProviders)
            {
                BootProvider(provider);
            }

            IsBooted = true;
            Process = StartProcess.Running;

            RaiseAppCallbacks(bootedCallbacks);
        }

        /// <inheritdoc />
        public virtual void Register(IServiceProvider provider, bool force = false)
        {
            Guard.Requires<ArgumentNullException>(provider != null, $"Parameter \"{nameof(provider)}\" can not be null.");

            if (IsRegistered(provider))
            {
                if (!force)
                {
                    throw new LogicException($"Provider [{provider.GetType()}] is already register.");
                }

                loadedProviders.Remove(provider);
            }

            if (Process == StartProcess.Boot)
            {
                throw new LogicException($"Unable to add service provider during {nameof(StartProcess.Boot)}");
            }

            if (Process > StartProcess.Running)
            {
                throw new LogicException($"Unable to {nameof(Terminate)} in-process registration service provider");
            }

            if (provider is ServiceProvider baseProvider)
            {
                baseProvider.SetApplication(this);
            }

            try
            {
                registering = true;
                provider.Register();
            }
            finally
            {
                registering = false;
            }

            loadedProviders.Add(provider);

            if (IsBooted)
            {
                BootProvider(provider);
            }
        }

        /// <inheritdoc />
        public bool IsRegistered(IServiceProvider provider)
        {
            Guard.Requires<ArgumentNullException>(provider != null);
            return loadedProviders.Contains(provider);
        }

        /// <inheritdoc />
        public long GetRuntimeId()
        {
            return Interlocked.Increment(ref incrementId);
        }

        /// <summary>
        /// Boot the specified service provider.
        /// </summary>
        /// <param name="provider">The specified service provider.</param>
        protected virtual void BootProvider(IServiceProvider provider)
        {
            provider.Boot();
        }

        /// <inheritdoc />
        protected override void GuardConstruct(string method)
        {
            if (registering)
            {
                throw new LogicException(
                    $"It is not allowed to make services or dependency injection in the {nameof(Register)} process, method:{method}");
            }

            base.GuardConstruct(method);
        }

        private void RegisterBaseBindings()
        {
            this.Singleton<IApplication>(() => this).Alias<Application>().Alias<IContainer>();
        }

        private void RegisterBaseProviders()
        {
            Register(new EventDispatcherProvider());
        }

        private void RaiseAppCallbacks(IEnumerable<Action<IApplication>> callbacks)
        {
            foreach (var callback in callbacks)
            {
                callback(this);
            }
        }
    }
}
