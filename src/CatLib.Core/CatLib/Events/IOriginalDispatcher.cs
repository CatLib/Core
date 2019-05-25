﻿/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this sender code.
 *
 * Document: https://catlib.io/
 */

// todo: removed with new event system.
namespace CatLib
{
    /// <summary>
    /// 原始调度器.
    /// </summary>
    internal interface IOriginalDispatcher
    {
        /// <summary>
        /// Gets 原始调度器.
        /// </summary>
        IDispatcher Dispatcher { get; }
    }
}
