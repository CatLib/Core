﻿/*
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
    [AttributeUsage(AttributeTargets.Interface |
                    AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
    }
}
