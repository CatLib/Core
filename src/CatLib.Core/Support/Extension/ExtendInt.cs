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

namespace CatLib.Support
{
    /// <summary>
    /// Int extension function.
    /// </summary>
    public static class ExtendInt
    {
        /// <summary>
        /// Calculate the power of the nearest two.
        /// </summary>
        /// <param name="min">The starting number.</param>
        /// <returns>The  power of the nearest two.</returns>
        public static int ToPrime(this int min)
        {
            min = Math.Max(0, min);

            var result = 0;
            for (var i = 2; i < int.MaxValue; i <<= 1)
            {
                if (i < min)
                {
                    continue;
                }

                result = i;
                break;
            }

            return result;
        }
    }
}
