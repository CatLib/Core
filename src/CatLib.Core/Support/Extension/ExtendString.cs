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

using System.IO;
using System.Text;

namespace CatLib.Support
{
    /// <summary>
    /// The string extension function.
    /// </summary>
    public static class ExtendString
    {
        /// <summary>
        /// Convert the specified string to a stream.
        /// </summary>
        /// <param name="str">The specified string.</param>
        /// <param name="encoding">The string encoding.</param>
        /// <returns>The stream instance.</returns>
        public static Stream ToStream(this string str, Encoding encoding = null)
        {
            return new MemoryStream((encoding ?? Encoding.Default).GetBytes(str));
        }
    }
}
