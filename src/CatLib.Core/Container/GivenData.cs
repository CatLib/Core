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

using CatLib.Support;
using System;

namespace CatLib.Container
{
    /// <inheritdoc />
    internal sealed class GivenData<TReturn> : IGivenData<TReturn>
        where TReturn : class, IBindable<TReturn>
    {
        /// <inheritdoc cref="BindData"/>
        private readonly Bindable<TReturn> bindable;

        /// <summary>
        /// The container to which the service belongs.
        /// </summary>
        private readonly Container container;

        /// <summary>
        /// The demand service.
        /// </summary>
        private string needs;

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenData{TReturn}"/> class.
        /// </summary>
        /// <param name="container">The container instance.</param>
        /// <param name="bindable">The bindable data.</param>
        internal GivenData(Container container, Bindable<TReturn> bindable)
        {
            this.container = container;
            this.bindable = bindable;
        }

        /// <inheritdoc />
        public TReturn Given(string service)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            bindable.AddContextual(needs, service);
            return bindable as TReturn;
        }

        /// <inheritdoc />
        public TReturn Given<TService>()
        {
            return Given(container.Type2Service(typeof(TService)));
        }

        /// <inheritdoc />
        public TReturn Given(Func<object> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            bindable.AddContextual(needs, closure);
            return bindable as TReturn;
        }

        /// <inheritdoc cref="Bindable{TReturn}.Needs"/>
        internal IGivenData<TReturn> Needs(string needs)
        {
            this.needs = needs;
            return this;
        }
    }
}
