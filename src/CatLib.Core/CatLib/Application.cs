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
using IEnumerator = System.Collections.IEnumerator;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace CatLib
{
    /// <summary>
    /// The CatLib <see cref="Application"/> instance.
    /// </summary>
    public class Application : Container, IApplication, IOriginalDispatcher
    {
        /// <summary>
        /// The version of the framework application.
        /// </summary>
        private static string version;

        /// <summary>
        /// Gets the CatLib <see cref="Application"/> version.
        /// </summary>
        public static string Version => version ?? (version = FileVersionInfo
                       .GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion);

        /// <summary>
        /// The framework start process type.
        /// </summary>
        public enum StartProcess
        {
            /// <summary>
            /// When you create a new <see cref="Application"/>,
            /// you are in the <see cref="Construct"/> phase.
            /// </summary>
            Construct = 0,

            /// <summary>
            /// Before the <see cref="Application.Bootstrap"/> call.
            /// </summary>
            Bootstrap = 1,

            /// <summary>
            /// When during <see cref="Application.Bootstrap"/> execution,
            /// you are in the <see cref="Bootstrapping"/> phase.
            /// </summary>
            Bootstrapping = 2,

            /// <summary>
            /// After the <see cref="Application.Bootstrap"/> called.
            /// </summary>
            Bootstraped = 3,

            /// <summary>
            /// Before the <see cref="Application.Init"/> call.
            /// </summary>
            Init = 4,

            /// <summary>
            /// When during <see cref="Application.Init"/> execution,
            /// you are in the <see cref="Initing"/> phase.
            /// </summary>
            Initing = 5,

            /// <summary>
            /// After the <see cref="Application.Init"/> called.
            /// </summary>
            Inited = 6,

            /// <summary>
            /// When the framework running.
            /// </summary>
            Running = 7,

            /// <summary>
            /// Before the <see cref="Application.Terminate"/> call.
            /// </summary>
            Terminate = 8,

            /// <summary>
            /// When during <see cref="Application.Terminate"/> execution,
            /// you are in the <see cref="Terminating"/> phase.
            /// </summary>
            Terminating = 9,

            /// <summary>
            /// After the <see cref="Application.Terminate"/> called. 
            /// All resources are destroyed.
            /// </summary>
            Terminated = 10,
        }

        /// <summary>
        /// All of the registered service providers.
        /// </summary>
        private readonly SortedList<int, List<IServiceProvider>> serviceProviders
            = new SortedList<int, List<IServiceProvider>>();

        /// <summary>
        /// The types of the loaded service providers.
        /// </summary>
        private readonly HashSet<Type> loadedProviders = new HashSet<Type>();

        /// <summary>
        /// True if the application has been bootstrapped.
        /// </summary>
        private bool bootstrapped;

        /// <summary>
        /// True if the application has been initialized.
        /// </summary>
        private bool inited;

        /// <summary>
        /// True if the <see cref="Register"/> is being executed.
        /// </summary>
        private bool registering;

        /// <summary>
        /// Indicates the application startup process.
        /// </summary>
        public StartProcess Process { get; private set; }

        /// <summary>
        /// The unique runtime id.
        /// </summary>
        private long incrementId;

        /// <summary>
        /// The main thread id.
        /// </summary>
        private readonly int mainThreadId;

        /// <inheritdoc />
        public bool IsMainThread => mainThreadId == Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// The global event dispatcher.
        /// </summary>
        private IDispatcher dispatcher;

        /// <inheritdoc cref="dispatcher"/>
        public IDispatcher Dispatcher => dispatcher ?? (dispatcher = Resolve<IDispatcher>());

        /// <summary>
        /// The debug level.
        /// </summary>
        private DebugLevels debugLevel;

        /// <summary>
        /// Create a new CatLib <see cref="Application"/> instance. 
        /// </summary>
        /// <param name="global">True if sets the instance to <see cref="App"/> facade.</param>
        public Application(bool global = true)
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            RegisterCoreAlias();
            RegisterCoreService();

            // We use closures to save the current context state
            // Do not change to: OnFindType(Type.GetType) This 
            // causes the active assembly to be not the expected scope.
            OnFindType(finder => { return Type.GetType(finder); });

            DebugLevel = DebugLevels.Production;
            Process = StartProcess.Construct;

            if (global)
            {
                App.Handler = this;
            }
        }

        /// <inheritdoc cref="Application(bool)"/>
        /// <returns>The CatLib <see cref="Application"/> instance.</returns>
        public static Application New(bool global = true)
        {
            return new Application(global);
        }

        /// <inheritdoc />
        public virtual void Terminate()
        {
            Process = StartProcess.Terminate;
            Trigger(ApplicationEvents.OnTerminate, this);
            Process = StartProcess.Terminating;
            Flush();
            if (App.HasHandler && App.Handler == this)
            {
                App.Handler = null;
            }
            Process = StartProcess.Terminated;
            Trigger(ApplicationEvents.OnTerminated);
        }

        /// <summary>
        /// Bootstrap the given array of bootstrap classes.
        /// </summary>
        /// <param name="bootstraps">The given bootstrap classes.</param>
        public virtual void Bootstrap(params IBootstrap[] bootstraps)
        {
            Guard.Requires<ArgumentNullException>(bootstraps != null);

            if (bootstrapped || Process != StartProcess.Construct)
            {
                throw new CodeStandardException($"Cannot repeatedly trigger the {nameof(Bootstrap)}()");
            }

            Process = StartProcess.Bootstrap;
            Trigger(ApplicationEvents.OnBootstrap, this);
            Process = StartProcess.Bootstrapping;

            var sorting = new SortedList<int, List<IBootstrap>>();
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
                AddSortedList(sorting, bootstrap, nameof(IBootstrap.Bootstrap));
            }

            foreach (var sorted in sorting)
            {
                foreach (var bootstrap in sorted.Value)
                {
                    var allow = TriggerHalt(ApplicationEvents.Bootstrapping, bootstrap) == null;
                    if (bootstrap != null && allow)
                    {
                        bootstrap.Bootstrap();
                    }
                }
            }

            Process = StartProcess.Bootstraped;
            bootstrapped = true;
            Trigger(ApplicationEvents.OnBootstraped, this);
        }

        /// <summary>
        /// Init all of the registered service provider.
        /// </summary>
        public virtual void Init()
        {
            StartCoroutine(CoroutineInit());
        }

        /// <inheritdoc cref="Init"/>
        /// <returns>Indicate the initialization progress.</returns>
        protected IEnumerator CoroutineInit()
        {
            if (!bootstrapped)
            {
                throw new CodeStandardException($"You must call {nameof(Bootstrap)}() first.");
            }

            if (inited || Process != StartProcess.Bootstraped)
            {
                throw new CodeStandardException($"Cannot repeatedly trigger the {nameof(Init)}()");
            }

            Process = StartProcess.Init;
            Trigger(ApplicationEvents.OnInit, this);
            Process = StartProcess.Initing;

            foreach (var sorted in serviceProviders)
            {
                foreach (var provider in sorted.Value)
                {
                    yield return InitProvider(provider);
                }
            }

            inited = true;
            Process = StartProcess.Inited;
            Trigger(ApplicationEvents.OnInited, this);

            Process = StartProcess.Running;
            Trigger(ApplicationEvents.OnStartCompleted, this);
        }

        /// <inheritdoc />
        public virtual void Register(IServiceProvider provider, bool force = false)
        {
            StartCoroutine(CoroutineRegister(provider, force));
        }

        /// <inheritdoc cref="IApplication.Register"/>
        /// <returns>Indicates the initialization progress if the 
        /// application has initialized, otherwise it makes no sense.</returns>
        protected IEnumerator CoroutineRegister(IServiceProvider provider, bool force = false)
        {
            Guard.Requires<ArgumentNullException>(provider != null);

            if (IsRegistered(provider))
            {
                if (!force)
                {
                    throw new LogicException($"Provider [{provider.GetType()}] is already register.");
                }
                loadedProviders.Remove(GetProviderBaseType(provider));
            }

            if (Process == StartProcess.Initing)
            {
                throw new CodeStandardException($"Unable to add service provider during {nameof(StartProcess.Initing)}");
            }

            if (Process > StartProcess.Running)
            {
                throw new CodeStandardException($"Unable to {nameof(Terminate)} in-process registration service provider");
            }

            var allow = TriggerHalt(ApplicationEvents.OnRegisterProvider, provider) == null;
            if (!allow)
            {
                yield break;
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

            AddSortedList(serviceProviders, provider, nameof(IServiceProvider.Init));
            loadedProviders.Add(GetProviderBaseType(provider));

            if (inited)
            {
                yield return InitProvider(provider);
            }
        }

        /// <summary>
        /// Add the specified element to the sorted list.
        /// </summary>
        /// <param name="list">The sorted list.</param>
        /// <param name="insert">The specified element.</param>
        /// <param name="priorityMethod">Specify a method name, the priority will 
        /// be obtained from this method.</param>
        protected void AddSortedList<T>(IDictionary<int, List<T>> list, T insert, string priorityMethod)
        {
            var priority = GetPriority(insert.GetType(), priorityMethod);

            if (!list.TryGetValue(priority, out List<T> providers))
            {
                list.Add(priority, providers = new List<T>());
            }

            providers.Add(insert);
        }

        /// <summary>
        /// Initialize the specified service provider.
        /// </summary>
        /// <param name="provider">The specified service provider.</param>
        /// <returns>Indicate the initialization progress.</returns>
        protected virtual IEnumerator InitProvider(IServiceProvider provider)
        {
            Trigger(ApplicationEvents.OnProviderInit, provider);

            provider.Init();
            if (provider is ICoroutineInit coroutine)
            {
                yield return coroutine.CoroutineInit();
            }

            Trigger(ApplicationEvents.OnProviderInited, provider);
        }

        /// <inheritdoc />
        public bool IsRegistered(IServiceProvider provider)
        {
            Guard.Requires<ArgumentNullException>(provider != null);
            return loadedProviders.Contains(GetProviderBaseType(provider));
        }

        /// <inheritdoc />
        public long GetRuntimeId()
        {
            return Interlocked.Increment(ref incrementId);
        }

        /// <inheritdoc />
        public int GetPriority(Type type, string method = null)
        {
            Guard.Requires<ArgumentNullException>(type != null);
            var priority = typeof(PriorityAttribute);
            var currentPriority = int.MaxValue;

            MethodInfo methodInfo;
            if (method != null &&
                (methodInfo = type.GetMethod(method)) != null &&
                methodInfo.IsDefined(priority, false))
            {
                currentPriority = ((PriorityAttribute)methodInfo.GetCustomAttributes(priority, false)[0]).Priorities;
            }
            else if (type.IsDefined(priority, false))
            {
                currentPriority = ((PriorityAttribute)type.GetCustomAttributes(priority, false)[0]).Priorities;
            }

            return currentPriority;
        }

        /// <inheritdoc />
        public DebugLevels DebugLevel
        {
            get => debugLevel;
            set
            {
                debugLevel = value;
                Instance(Type2Service(typeof(DebugLevels)), debugLevel);
            }
        }

        /// <inheritdoc />
        public object[] Trigger(string eventName, params object[] payloads)
        {
            return Dispatcher.Trigger(eventName, payloads);
        }

        /// <inheritdoc />
        public object TriggerHalt(string eventName, params object[] payloads)
        {
            return Dispatcher.TriggerHalt(eventName, payloads);
        }

        /// <inheritdoc />
        public bool HasListeners(string eventName, bool strict = false)
        {
            return Dispatcher.HasListeners(eventName, strict);
        }

        /// <inheritdoc />
        public IEvent On(string eventName, Func<string, object[], object> execution, object group = null)
        {
            return Dispatcher.On(eventName, execution, group);
        }

        /// <inheritdoc />
        public void Off(object target)
        {
            Dispatcher.Off(target);
        }

        /// <inheritdoc />
        protected override void GuardConstruct(string method)
        {
            if (registering)
            {
                throw new CodeStandardException(
                    $"It is not allowed to make services or dependency injection in the {nameof(Register)} process, method:{method}");
            }

            // todo: rebuild event system.
            /*
            if (Process < StartProcess.Bootstraped)
            {
                throw new CodeStandardException(
                    $"It is not allowed to make services or dependency injection before {nameof(Bootstrap)} process, method:{method}");
            }*/

            base.GuardConstruct(method);
        }

        /// <summary>
        /// Call the iterator with the default coroutine.
        /// </summary>
        /// <param name="iterator">The iterator.</param>
        protected void StartCoroutine(IEnumerator iterator)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(iterator);
            do
            {
                iterator = stack.Pop();
                while (iterator.MoveNext())
                {
                    if (!(iterator.Current is IEnumerator nextCoroutine))
                    {
                        continue;
                    }

                    stack.Push(iterator);
                    iterator = nextCoroutine;
                }
            } while (stack.Count > 0);
        }

        /// <summary>
        /// Resolve the given type from the container.(Will not perform <see cref="GuardConstruct"/> check).
        /// </summary>
        /// <typeparam name="TService">The service type(name).</typeparam>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The resolved service instance.</returns>
        protected TService Resolve<TService>(params object[] userParams)
        {
            return (TService)Resolve(Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Register the core service aliases.
        /// </summary>
        private void RegisterCoreAlias()
        {
            this.Singleton<IApplication>(() => this)
                .Alias<Application>()
                .Alias<IContainer>();
        }

        /// <summary>
        /// Register the core services.
        /// </summary>
        private void RegisterCoreService()
        {
            var bindable = new BindData(this, null, null, false);
            this.Singleton<IDispatcher>(() => new GlobalDispatcher(
                (paramInfos, userParams) => GetDependencies(bindable, paramInfos, userParams)));
        }

        /// <summary>
        /// Get the base type from service provider.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <returns>Base type for service provider.</returns>
        private static Type GetProviderBaseType(IServiceProvider provider)
        {
            var providerType = provider as IServiceProviderType;
            return providerType == null ? provider.GetType() : providerType.BaseType;
        }
    }
}