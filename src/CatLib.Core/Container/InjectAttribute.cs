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

namespace CatLib.Container
{
    /// <summary>
    /// The injection attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the property is required.
        /// </summary>
        public bool Required { get; set; } = true;
    }
}
