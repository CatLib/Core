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

namespace CatLib.Util
{
    /// <summary>
    /// Represents an internal generic helper class.
    /// </summary>
    internal static class InternalHelper
    {
        internal static Random MakeRandom(int? seed = null)
        {
            return new Random(seed.GetValueOrDefault(MakeSeed()));
        }

        internal static int MakeSeed()
        {
            return Environment.TickCount ^ Guid.NewGuid().GetHashCode();
        }

        internal static void NormalizationPosition(int sourceLength, ref int start, ref int? length)
        {
            start = (start >= 0) ? Math.Min(start, sourceLength) : Math.Max(sourceLength + start, 0);

            if (length == null)
            {
                length = Math.Max(sourceLength - start, 0);
                return;
            }

            length = (length >= 0)
                    ? Math.Min(length.Value, sourceLength - start)
                    : Math.Max(sourceLength + length.Value - start, 0);
        }
    }
}
