/*
 * This file is part of the CatLib package.
 *
 * (c) Yu Bin <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

using System;

namespace CatLib
{
    /// <summary>
    /// Int 扩展函数
    /// </summary>
    public static class IntExtension
    {
        /// <summary>
        /// 计算规定值最近的二的次幂
        /// </summary>
        /// <param name="min">规定的值</param>
        /// <returns>容量</returns>
        public static int ToPrime(this int min)
        {
            min = Math.Max(0, min);

            var result = 0;
            for (var i = 2; i < int.MaxValue; i = i << 1)
            {
                if (i >= min)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }
    }
}
