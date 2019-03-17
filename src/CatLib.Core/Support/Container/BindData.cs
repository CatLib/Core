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
    /// <inheritdoc cref="IBindData"/>
    public sealed class BindData : Bindable<IBindData>, IBindData
    {
        /// <inheritdoc />
        public Func<IContainer, object[], object> Concrete { get; }

        /// <inheritdoc />
        public bool IsStatic { get; }

        /// <summary>
        /// The resolving callbacks.
        /// </summary>
        private List<Action<IBindData, object>> resolving;

        /// <summary>
        /// The after resolving callbacks.
        /// </summary>
        private List<Action<IBindData, object>> afterResolving;

        /// <summary>
        /// The release callbacks.
        /// </summary>
        private List<Action<IBindData, object>> release;

        /// <summary>
        /// Create a new <see cref="BindData"/> instance.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="isStatic">Whether the service is singleton(static).</param>
        public BindData(Container container, string service, Func<IContainer, object[], object> concrete, bool isStatic)
            : base(container, service)
        {
            Concrete = concrete;
            IsStatic = isStatic;
        }

        /// <inheritdoc />
        public IBindData Alias(string alias)
        {
            lock (SyncRoot)
            {
                AssertDestroyed();
                Guard.NotEmptyOrNull(alias, nameof(alias));
                InternalContainer.Alias(alias, Service);
                return this;
            }
        }

        /// <inheritdoc />
        public IBindData Tag(string tag)
        {
            lock (SyncRoot)
            {
                AssertDestroyed();
                Guard.NotEmptyOrNull(tag, nameof(tag));
                InternalContainer.Tag(tag, Service);
                return this;
            }
        }

        /// <inheritdoc />
        public IBindData OnResolving(Action<IBindData, object> closure)
        {
            AddClosure(closure, ref resolving);
            return this;
        }

        /// <inheritdoc />
        public IBindData OnAfterResolving(Action<IBindData, object> closure)
        {
            AddClosure(closure, ref afterResolving);
            return this;
        }

        /// <inheritdoc />
        public IBindData OnRelease(Action<IBindData, object> closure)
        {
            if (!IsStatic)
            {
                throw new LogicException(
                    $"Service [{Service}] is not Singleton(Static) Bind , Can not call {nameof(OnRelease)}().");
            }

            AddClosure(closure, ref release);
            return this;
        }

        /// <inheritdoc />
        protected override void ReleaseBind()
        {
            InternalContainer.Unbind(this);
        }

        /// <summary>
        /// Trigger all of the resolving callbacks.
        /// </summary>
        /// <param name="instance">The service instance.</param>
        /// <returns>The service instance.</returns>
        internal object TriggerResolving(object instance)
        {
            return InternalContainer.Trigger(this, instance, resolving);
        }

        /// <summary>
        /// Trigger all of the after resolving callbacks.
        /// </summary>
        /// <param name="instance">The service instance.</param>
        /// <returns>The service instance.</returns>
        internal object TriggerAfterResolving(object instance)
        {
            return InternalContainer.Trigger(this, instance, afterResolving);
        }

        /// <summary>
        /// Trigger all of the release callbacks.
        /// </summary>
        /// <param name="instance">The service instance.</param>
        /// <returns>The service instance.</returns>
        internal object TriggerRelease(object instance)
        {
            return InternalContainer.Trigger(this, instance, release);
        }

        /// <summary>
        /// Register a new callback in specified list.
        /// </summary>
        /// <param name="closure">The callback.</param>
        /// <param name="list">The specified list.</param>
        private void AddClosure(Action<IBindData, object> closure, ref List<Action<IBindData, object>> list)
        {
            Guard.NotNull(closure, nameof(closure));

            lock (SyncRoot)
            {
                AssertDestroyed();

                if (list == null)
                {
                    list = new List<Action<IBindData, object>>();
                }

                list.Add(closure);
            }
        }
    }
}