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

using CatLib.Exception;
using CatLib.Support;
using System;
using System.Collections.Generic;
using CatLibContainer = CatLib.Container.Container;

namespace CatLib.Container
{
    /// <inheritdoc cref="IBindData"/>
    public sealed class BindData : Bindable<IBindData>, IBindData
    {
        /// <summary>
        /// The local resolving callbacks.
        /// </summary>
        private List<Action<IBindData, object>> resolving;

        /// <summary>
        /// The local after resolving callbacks.
        /// </summary>
        private List<Action<IBindData, object>> afterResolving;

        /// <summary>
        /// The local release callbacks.
        /// </summary>
        private List<Action<IBindData, object>> release;

        /// <summary>
        /// Initializes a new instance of the <see cref="BindData"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="isStatic">Whether the service is singleton(static).</param>
        public BindData(CatLibContainer container, string service, Func<IContainer, object[], object> concrete, bool isStatic)
            : base(container, service)
        {
            Concrete = concrete;
            IsStatic = isStatic;
        }

        /// <inheritdoc />
        public Func<IContainer, object[], object> Concrete { get; }

        /// <inheritdoc />
        public bool IsStatic { get; }

        /// <inheritdoc />
        public IBindData Alias(string alias)
        {
            lock (Locker)
            {
                AssertDestroyed();
                Guard.ParameterNotNull(alias, nameof(alias));
                Container.Alias(alias, Service);
                return this;
            }
        }

        /// <inheritdoc />
        public IBindData Tag(string tag)
        {
            lock (Locker)
            {
                AssertDestroyed();
                Guard.ParameterNotNull(tag, nameof(tag));
                Container.Tag(tag, Service);
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

        internal object TriggerResolving(object instance)
        {
            return CatLibContainer.Trigger(this, instance, resolving);
        }

        internal object TriggerAfterResolving(object instance)
        {
            return CatLibContainer.Trigger(this, instance, afterResolving);
        }

        internal object TriggerRelease(object instance)
        {
            return CatLibContainer.Trigger(this, instance, release);
        }

        /// <inheritdoc />
        protected override void ReleaseBind()
        {
            ((CatLibContainer)Container).Unbind(this);
        }

        private void AddClosure(Action<IBindData, object> closure, ref List<Action<IBindData, object>> list)
        {
            Guard.Requires<ArgumentNullException>(closure != null);

            lock (Locker)
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
