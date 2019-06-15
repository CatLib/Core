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
    /// Extension method for <see cref="Type"/>.
    /// </summary>
    public static class ExtendType
    {
        /// <summary>
        /// Convert type to service name.
        /// </summary>
        /// <param name="type">The given type.</param>
        /// <returns>The service name.</returns>
        public static string ToService(this Type type)
        {
            return App.Type2Service(type);
        }
    }
}
