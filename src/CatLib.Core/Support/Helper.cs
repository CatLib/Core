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
    /// 通用支持.
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// 构建一个随机生成器.
        /// </summary>
        /// <param name="seed">种子.</param>
        /// <returns>随机生成器.</returns>
        internal static System.Random MakeRandom(int? seed = null)
        {
            return new System.Random(seed.GetValueOrDefault(MakeSeed()));
        }

        /// <summary>
        /// 生成种子.
        /// </summary>
        /// <returns>种子.</returns>
        internal static int MakeSeed()
        {
            return Environment.TickCount ^ Guid.NewGuid().GetHashCode();
        }

        /// <summary>
        /// 标准化位置.
        /// </summary>
        /// <param name="sourceLength">源长度.</param>
        /// <param name="start">起始位置.</param>
        /// <param name="length">作用长度.</param>
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
