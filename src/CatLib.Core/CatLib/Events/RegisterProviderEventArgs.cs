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

using CatLib.EventDispatcher;

namespace CatLib
{
    /// <summary>
    /// Indicates a service provider that will register.
    /// </summary>
    public class RegisterProviderEventArgs : ApplicationEventArgs, IStoppableEvent
    {
        private readonly IServiceProvider provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterProviderEventArgs"/> class.
        /// </summary>
        /// <param name="provider">The service provider class that will register.</param>
        /// <param name="application">The application instance.</param>
        public RegisterProviderEventArgs(IServiceProvider provider, IApplication application)
            : base(application)
        {
            IsSkip = false;
            this.provider = provider;
        }

        /// <summary>
        /// Gets a value indicating whether the service provider is skip register.
        /// </summary>
        public bool IsSkip { get; private set; }

        /// <summary>
        /// Gets the a service provider class that will register.
        /// </summary>
        /// <returns>Return the service provider class.</returns>
        public IServiceProvider GetServiceProvider()
        {
            return provider;
        }

        /// <summary>
        /// Skip the register service provider.
        /// </summary>
        public void Skip()
        {
            IsSkip = true;
        }

        /// <inheritdoc />
        public bool IsPropagationStopped()
        {
            return IsSkip;
        }
    }
}
