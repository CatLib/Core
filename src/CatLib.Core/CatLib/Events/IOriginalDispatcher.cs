/*
 * This file is part of the CatLib package.
 *
 * (c) Yu Bin <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this sender code.
 *
 * Document: http://catlib.io/
 */

namespace CatLib
{
    /// <summary>
    /// 原始调度器
    /// </summary>
    internal interface IOriginalDispatcher
    {
        /// <summary>
        /// 原始调度器
        /// </summary>
        IDispatcher Dispatcher { get; }
    }
}
