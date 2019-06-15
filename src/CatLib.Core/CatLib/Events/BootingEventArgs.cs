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
    /// Indicates a boot class that is booting.
    /// </summary>
    public class BootingEventArgs : ApplicationEventArgs, IStoppableEvent
    {
        private readonly IBootstrap bootstrap;

        /// <summary>
        /// Initializes a new instance of the <see cref="BootingEventArgs"/> class.
        /// </summary>
        /// <param name="bootstrap">The boot class that is booting.</param>
        /// <param name="application">The application instance.</param>
        public BootingEventArgs(IBootstrap bootstrap, IApplication application)
            : base(application)
        {
            IsSkip = false;
            this.bootstrap = bootstrap;
        }

        /// <summary>
        /// Gets a value indicating whether the boot class is skip booting.
        /// </summary>
        public bool IsSkip { get; private set; }

        /// <summary>
        /// Gets the a boot class that is booting.
        /// </summary>
        /// <returns>Return the boot class.</returns>
        public IBootstrap GetBootstrap()
        {
            return bootstrap;
        }

        /// <summary>
        /// Disable the boot class.
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
