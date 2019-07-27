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

namespace CatLib.Events
{
    /// <summary>
    /// It indicates that the bootstrap will be bootstrapped.
    /// </summary>
    public class BeforeBootEventArgs : ApplicationEventArgs
    {
        private IBootstrap[] bootstraps;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeBootEventArgs"/> class.
        /// </summary>
        /// <param name="bootstraps">An array of the bootstrap list.</param>
        /// <param name="application">The application instance.</param>
        public BeforeBootEventArgs(IBootstrap[] bootstraps, IApplication application)
            : base(application)
        {
            this.bootstraps = bootstraps;
        }

        /// <summary>
        /// Gets an array of bootstrap will be bootstrapped.
        /// </summary>
        /// <returns>Returns an array of bootstraps.</returns>
        public IBootstrap[] GetBootstraps()
        {
            return bootstraps;
        }

        /// <summary>
        /// Sets the bootstrap will replace the old boot list.
        /// </summary>
        /// <param name="bootstraps">New bootstrap list.</param>
        public void SetBootstraps(IBootstrap[] bootstraps)
        {
            this.bootstraps = bootstraps;
        }
    }
}
