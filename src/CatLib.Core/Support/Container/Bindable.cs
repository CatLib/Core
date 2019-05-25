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

namespace CatLib
{
    /// <summary>
    /// The bindable data indicates relational data related to the specified service.
    /// </summary>
    public abstract class Bindable : IBindable
    {
        /// <summary>
        /// Synchronize locking object.
        /// </summary>
        private readonly object locker = new object();

        /// <inheritdoc cref="Container"/>
        private readonly Container container;

        /// <summary>
        /// The mapping of the service context.
        /// </summary>
        private Dictionary<string, string> contextual;

        /// <summary>
        /// The closure mapping of the service context.
        /// </summary>
        private Dictionary<string, Func<object>> contextualClosure;

        /// <summary>
        /// Whether the bindable data is destroyed.
        /// </summary>
        private bool isDestroy;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bindable"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <param name="service">The service name.</param>
        protected Bindable(Container container, string service)
        {
            this.container = container;
            Service = service;
            isDestroy = false;
        }

        /// <inheritdoc />
        public string Service { get; }

        /// <summary>
        /// Gets the container to which the service belongs.
        /// </summary>
        public IContainer Container => container;

        /// <summary>
        /// Gets synchronize locking object.
        /// </summary>
        protected object Locker => locker;

        /// <inheritdoc />
        public void Unbind()
        {
            lock (locker)
            {
                isDestroy = true;
                ReleaseBind();
            }
        }

        /// <summary>
        /// Add the context with service.
        /// </summary>
        /// <param name="needs">Demand specified service.</param>
        /// <param name="given">Given speified service or alias.</param>
        internal void AddContextual(string needs, string given)
        {
            lock (locker)
            {
                AssertDestroyed();
                if (contextual == null)
                {
                    contextual = new Dictionary<string, string>();
                }

                if (contextual.ContainsKey(needs)
                    || (contextualClosure != null && contextualClosure.ContainsKey(needs)))
                {
                    throw new LogicException($"Needs [{needs}] is already exist.");
                }

                contextual.Add(needs, given);
            }
        }

        /// <inheritdoc cref="AddContextual(string, string)"/>
        /// <param name="given">The closure return the given service instance.</param>
        internal void AddContextual(string needs, Func<object> given)
        {
            lock (locker)
            {
                AssertDestroyed();
                if (contextualClosure == null)
                {
                    contextualClosure = new Dictionary<string, Func<object>>();
                }

                if (contextualClosure.ContainsKey(needs)
                    || (contextual != null && contextual.ContainsKey(needs)))
                {
                    throw new LogicException($"Needs [{needs}] is already exist.");
                }

                contextualClosure.Add(needs, given);
            }
        }

        /// <summary>
        /// Get the demand context of the service.
        /// </summary>
        /// <param name="needs">The demand service.</param>
        /// <returns>The given service or alias.</returns>
        internal string GetContextual(string needs)
        {
            if (contextual == null)
            {
                return null;
            }

            return contextual.TryGetValue(needs, out string contextualNeeds) ? contextualNeeds : null;
        }

        /// <inheritdoc cref="GetContextual"/>
        /// <returns>The closure return the given service instance.</returns>
        internal Func<object> GetContextualClosure(string needs)
        {
            if (contextualClosure == null)
            {
                return null;
            }

            return contextualClosure.TryGetValue(needs, out Func<object> closure) ? closure : null;
        }

        /// <inheritdoc cref="Unbind"/>
        protected abstract void ReleaseBind();

        /// <summary>
        /// Verify that the current instance i has been released.
        /// </summary>
        protected void AssertDestroyed()
        {
            if (isDestroy)
            {
                throw new LogicException("The current instance is destroyed.");
            }
        }
    }

#pragma warning disable SA1402

    /// <inheritdoc />
    public abstract class Bindable<TReturn> : Bindable, IBindable<TReturn>
        where TReturn : class, IBindable<TReturn>
    {
        /// <summary>
        /// Indicates the given relationship in the context.
        /// </summary>
        private GivenData<TReturn> given;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bindable{TReturn}"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <param name="service">The service name.</param>
        protected Bindable(Container container, string service)
            : base(container, service)
        {
        }

        /// <inheritdoc />
        public IGivenData<TReturn> Needs(string service)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            lock (Locker)
            {
                AssertDestroyed();
                if (given == null)
                {
                    given = new GivenData<TReturn>((Container)Container, this);
                }

                given.Needs(service);
            }

            return given;
        }

        /// <inheritdoc />
        public IGivenData<TReturn> Needs<TService>()
        {
            return Needs(Container.Type2Service(typeof(TService)));
        }
    }
}
