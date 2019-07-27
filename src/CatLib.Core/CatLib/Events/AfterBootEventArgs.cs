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
    /// Indicates that the bootstrap has been booted.
    /// </summary>
    public class AfterBootEventArgs : ApplicationEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AfterBootEventArgs"/> class.
        /// </summary>
        /// <param name="application">The application instance.</param>
        public AfterBootEventArgs(IApplication application)
            : base(application)
        {
        }
    }
}
