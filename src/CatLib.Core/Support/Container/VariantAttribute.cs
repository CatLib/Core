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
    /// A constructor that represents the class allows a primitive type(Include string) to be passed in to be converted to the current class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class VariantAttribute : Attribute
    {
    }
}
