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

namespace CatLib
{
    /// <summary>
    /// It indicates that the <see cref="IServiceProvider.Init"/> method will be called.
    /// </summary>
    public class BeforeInitEventArgs : ApplicationEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeInitEventArgs"/> class.
        /// </summary>
        /// <param name="bootstraps">An array of the bootstrap list.</param>
        /// <param name="application">The application instance.</param>
        public BeforeInitEventArgs(IApplication application)
            : base(application)
        {
        }
    }
}
