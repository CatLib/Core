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
    /// 事件对象
    /// </summary>
    internal class Event : IEvent
    {
        /// <summary>
        /// 原始事件名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 事件根源对象
        /// </summary>
        public object Group { get; private set; }

        /// <summary>
        /// 依赖解决器
        /// </summary>
        private readonly Func<string, object[], object> execution;

        /// <summary>
        /// 创建一个事件对象
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="group">事件分组</param>
        /// <param name="execution">事件执行器</param>
        public Event(string eventName, object group, Func<string, object[], object> execution)
        {
            Name = eventName;
            Group = group;
            this.execution = execution;
        }

        /// <summary>
        /// 调用事件处理函数
        /// </summary>
        /// <param name="eventName">调用事件的完整名字</param>
        /// <param name="payloads">事件载荷</param>
        /// <returns>事件处理结果</returns>
        public object Call(string eventName, params object[] payloads)
        {
            return execution(eventName, payloads);
        }
    }
}
