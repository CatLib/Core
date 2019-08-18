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

namespace CatLib
{
    /// <summary>
    /// Represents an application event.
    /// </summary>
    public class ApplicationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEventArgs"/> class.
        /// </summary>
        /// <param name="application">The application instance.</param>
        public ApplicationEventArgs(IApplication application)
        {
            Application = application;
        }

        /// <summary>
        /// Gets the application instance.
        /// </summary>
        public IApplication Application { get; private set; }
    }
}
