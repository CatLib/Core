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
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CatLib
{
    /// <summary>
    /// 事件调度器
    /// </summary>
    public class NewDispatcher
    {
        /// <summary>
        /// 调用方法目标 映射到 事件句柄
        /// </summary>
        private readonly Dictionary<object, List<Event>> targetMapping;

        /// <summary>
        /// 普通事件列表
        /// </summary>
        private readonly Dictionary<string, List<Event>> listeners;

        /// <summary>
        /// 通配符事件列表
        /// </summary>
        private readonly Dictionary<string, KeyValuePair<Regex, List<Event>>> wildcardListeners;

        /// <summary>
        /// 依赖注入容器
        /// </summary>
        private readonly IContainer container;

        /// <summary>
        /// 同步锁
        /// </summary>
        private readonly object syncRoot;

        /// <summary>
        /// 构建一个事件调度器
        /// </summary>
        /// <param name="container">依赖注入容器</param>
        public NewDispatcher(IContainer container)
        {
            Guard.Requires<ArgumentNullException>(container != null);

            this.container = container;
            syncRoot = new object();
            targetMapping = new Dictionary<object, List<Event>>();
            listeners = new Dictionary<string, List<Event>>();
            wildcardListeners = new Dictionary<string, KeyValuePair<Regex, List<Event>>>();
        }

        /// <summary>
        /// 触发一个事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="payloads">载荷</param>
        /// <returns></returns>
        public object[] Trigger(string eventName, params object[] payloads)
        {
            return null;
        }

        /// <summary>
        /// 注册一个事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="target">事件调用目标</param>
        /// <param name="method">事件调用方法</param>
        /// <returns>事件对象</returns>
        public IEvent On(string eventName, object target, MethodInfo method)
        {
            Guard.NotEmptyOrNull(eventName, "eventName");
            Guard.Requires<ArgumentNullException>(target != null);
            Guard.Requires<ArgumentNullException>(method != null);

            lock (syncRoot)
            {
                eventName = FormatEventName(eventName);

                var result = eventName.IndexOf('*') != -1
                    ? SetupWildcardListen(eventName, target, method)
                    : SetupListen(eventName, target, method);

                List<Event> listener;
                if (!targetMapping.TryGetValue(target, out listener))
                {
                    targetMapping[target] = listener = new List<Event>();
                }
                listener.Add(result);

                return result;
            }
        }

        /// <summary>
        /// 解除事件注册
        /// </summary>
        /// <param name="target">
        /// 事件解除目标
        /// <para>如果传入的是字符串(<code>string</code>)将会解除对应事件名的事件</para>
        /// <para>如果传入的是事件对象(<code>IEvent</code>)那么解除对应事件</para>
        /// <para>如果传入的是其他实例(<code>object</code>)会解除该实例下的所有事件</para>
        /// </param>
        public void Off(object target)
        {

        }

        /// <summary>
        /// 解除事件
        /// </summary>
        /// <param name="target">目标事件</param>
        internal void Off(Event target)
        {
            lock (syncRoot)
            {
                List<Event> events;
                if (targetMapping.TryGetValue(target.Target, out events))
                {
                    events.Remove(target);
                    if (events.Count <= 0)
                    {
                        targetMapping.Remove(target.Target);
                    }
                }

                if (target.EventName.IndexOf('*') != -1)
                {
                    DestoryWildcardListen(target);
                }
                else
                {
                    DestoryListen(target);
                }
            }
        }

        /// <summary>
        /// 销毁普通事件
        /// </summary>
        /// <param name="target">事件对象</param>
        private void DestoryListen(Event target)
        {
            List<Event> events;
            if (!listeners.TryGetValue(target.EventName, out events))
            {
                return;
            }

            events.Remove(target);
            if (events.Count <= 0)
            {
                listeners.Remove(target.EventName);
            }
        }

        /// <summary>
        /// 销毁通配符事件
        /// </summary>
        /// <param name="target">事件对象</param>
        private void DestoryWildcardListen(Event target)
        {
            KeyValuePair<Regex, List<Event>> wildcardEvents;
            if (!wildcardListeners.TryGetValue(target.EventName, out wildcardEvents))
            {
                return;
            }

            wildcardEvents.Value.Remove(target);
            if (wildcardEvents.Value.Count <= 0)
            {
                wildcardListeners.Remove(target.EventName);
            }
        }

        /// <summary>
        /// 设定普通事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="target">事件调用目标</param>
        /// <param name="method">事件调用方法</param>
        /// <returns>监听事件</returns>
        private Event SetupListen(string eventName, object target, MethodInfo method)
        {
            List<Event> listener;
            if (!listeners.TryGetValue(eventName, out listener))
            {
                listeners[eventName] = listener = new List<Event>();
            }

            Event output;
            listener.Add(output = new Event(this, eventName, target, MakeListener(target, method)));
            return output;
        }

        /// <summary>
        /// 设定通配符事件
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="target">事件调用目标</param>
        /// <param name="method">事件调用方法</param>
        /// <returns>监听事件</returns>
        private Event SetupWildcardListen(string eventName, object target, MethodInfo method)
        {
            eventName = Str.AsteriskWildcard(eventName);

            KeyValuePair<Regex, List<Event>> listener;
            if (!wildcardListeners.TryGetValue(eventName, out listener))
            {
                wildcardListeners[eventName] = listener =
                    new KeyValuePair<Regex, List<Event>>(new Regex(eventName), new List<Event>());
            }

            Event output;
            listener.Value.Add(output = new Event(this, eventName, target, MakeListener(target, method, true)));
            return output;
        }

        /// <summary>
        /// 创建事件监听器
        /// </summary>
        /// <param name="target">调用目标</param>
        /// <param name="method">调用方法</param>
        /// <param name="isWildcard">是否是通配符方法</param>
        /// <returns>事件监听器</returns>
        protected virtual Action<string, object[]> MakeListener(object target, MethodInfo method, bool isWildcard = false)
        {
            return (eventName, payloads) =>
            {
                container.Call(target, method, isWildcard
                    ? Arr.Merge(new object[] { method }, payloads)
                    : payloads);
            };
        }

        /// <summary>
        /// 格式化事件名
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <returns>格式化后的事件名</returns>
        protected virtual string FormatEventName(string eventName)
        {
            return eventName;
        }
    }
}
