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
    /// Declare priorities for method, class or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class PriorityAttribute : Attribute
    {
        /// <summary>
        /// The priority.
        /// </summary>
        public int Priorities { get; }

        /// <summary>
        /// Create a new priority instance.
        /// </summary>
        /// <param name="priority">The priority(0 is the highest priority)</param>
        public PriorityAttribute(int priority = int.MaxValue)
        {
            Priorities = Math.Max(priority, 0);
        }
    }
}