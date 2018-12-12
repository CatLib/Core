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
    /// 执行优先级
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class PriorityAttribute : Attribute
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priorities { get; }

        /// <summary>
        /// 优先级(0最高)
        /// </summary>
        /// <param name="priority">优先级(0为最优先)</param>
        public PriorityAttribute(int priority = int.MaxValue)
        {
            Priorities = Math.Max(priority, 0);
        }
    }
}