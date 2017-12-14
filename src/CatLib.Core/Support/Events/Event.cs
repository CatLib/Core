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
        /// 调用事件
        /// </summary>
        public Action<string, object[]> Call { get; private set; }

        /// <summary>
        /// 事件名
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// 事件目标
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// 调度器
        /// </summary>
        private readonly NewDispatcher dispatcher;

        /// <summary>
        /// 是否被释放
        /// </summary>
        private bool isOff;

        /// <summary>
        /// 创建一个事件对象
        /// </summary>
        /// <param name="dispatcher">调度器</param>
        /// <param name="eventName">事件名</param>
        /// <param name="target">调用方法目标</param>
        /// <param name="call">调用方法</param>
        public Event(NewDispatcher dispatcher, string eventName, object target, Action<string, object[]> call)
        {
            this.dispatcher = dispatcher;
            EventName = eventName;
            Target = target;
            Call = call;
            isOff = false;
        }

        /// <summary>
        /// 释放事件
        /// </summary>
        public void Off()
        {
            if (isOff)
            {
                return;
            }
            isOff = true;
            dispatcher.Off(this);
        }
    }
}
