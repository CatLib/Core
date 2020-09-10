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
    /// Indicates the framework debug level.
    /// </summary>
    public enum DebugLevel
    {
        /// <summary>
        /// Production environment.
        /// </summary>
        Production,

        /// <summary>
        /// Between the production environment and the developer environment.
        /// </summary>
        Staging,

        /// <summary>
        /// Development environment.
        /// </summary>
        Development,
    }
}
