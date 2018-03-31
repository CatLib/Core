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

namespace CatLib
{
    /// <summary>
    /// 事件对象
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// 原始事件名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 事件分组
        /// </summary>
        object Group { get; }

        /// <summary>
        /// 调用事件
        /// </summary>
        /// <param name="eventName">完整的事件名</param>
        /// <param name="payloads">载荷</param>
        /// <returns>事件结果</returns>
        object Call(string eventName, params object[] payloads);
    }
}
