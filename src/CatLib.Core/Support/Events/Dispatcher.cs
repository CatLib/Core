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
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CatLib
{
    /// <summary>
    /// 事件调度器.
    /// </summary>
    public class Dispatcher : IDispatcher
    {
        /// <summary>
        /// 分组映射.
        /// </summary>
        private readonly Dictionary<object, List<IEvent>> groupMapping;

        /// <summary>
        /// 普通事件列表.
        /// </summary>
        private readonly Dictionary<string, List<IEvent>> listeners;

        /// <summary>
        /// 通配符事件列表.
        /// </summary>
        private readonly Dictionary<string, KeyValuePair<Regex, List<IEvent>>> wildcardListeners;

        /// <summary>
        /// 同步锁.
        /// </summary>
        private readonly object syncRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dispatcher"/> class.
        /// </summary>
        public Dispatcher()
        {
            syncRoot = new object();
            groupMapping = new Dictionary<object, List<IEvent>>();
            listeners = new Dictionary<string, List<IEvent>>();
            wildcardListeners = new Dictionary<string, KeyValuePair<Regex, List<IEvent>>>();
        }

        /// <summary>
        /// Gets 跳出标记.
        /// </summary>
        protected virtual object BreakFlag => false;

        /// <summary>
        /// 判断给定事件是否存在事件监听器.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        /// <param name="strict">
        /// 严格模式.
        /// <para>启用严格模式则不使用正则来进行匹配事件监听器.</para>
        /// </param>
        /// <returns>是否存在事件监听器.</returns>
        public bool HasListeners(string eventName, bool strict = false)
        {
            eventName = FormatEventName(eventName);
            lock (syncRoot)
            {
                if (listeners.ContainsKey(eventName)
                    || wildcardListeners.ContainsKey(eventName))
                {
                    return true;
                }

                if (strict)
                {
                    return false;
                }

                foreach (var element in wildcardListeners)
                {
                    if (element.Value.Key.IsMatch(eventName))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 触发一个事件,并获取事件监听器的返回结果.
        /// </summary>
        /// <param name="eventName">事件名称.</param>
        /// <param name="payloads">载荷.</param>
        /// <returns>事件结果.</returns>
        public object[] Trigger(string eventName, params object[] payloads)
        {
            return Dispatch(false, eventName, payloads) as object[];
        }

        /// <summary>
        /// 触发一个事件,并获取事件监听器的返回结果.
        /// </summary>
        /// <param name="eventName">事件名称.</param>
        /// <param name="payloads">载荷.</param>
        /// <returns>事件结果.</returns>
        public object TriggerHalt(string eventName, params object[] payloads)
        {
            return Dispatch(true, eventName, payloads);
        }

        /// <summary>
        /// 注册一个事件监听器.
        /// </summary>
        /// <param name="eventName">事件名称.</param>
        /// <param name="execution">执行方法.</param>
        /// <param name="group">事件分组，如果为.<code>Null</code>则不进行分组.</param>
        /// <returns>事件对象.</returns>
        public IEvent On(string eventName, Func<string, object[], object> execution, object group = null)
        {
            Guard.NotEmptyOrNull(eventName, nameof(eventName));
            Guard.Requires<ArgumentNullException>(execution != null);

            lock (syncRoot)
            {
                eventName = FormatEventName(eventName);

                var result = IsWildcard(eventName)
                    ? SetupWildcardListen(eventName, execution, group)
                    : SetupListen(eventName, execution, group);

                if (group == null)
                {
                    return result;
                }

                if (!groupMapping.TryGetValue(group, out List<IEvent> listener))
                {
                    groupMapping[group] = listener = new List<IEvent>();
                }

                listener.Add(result);

                return result;
            }
        }

        /// <summary>
        /// 解除注册的事件监听器.
        /// </summary>
        /// <param name="target">
        /// 事件解除目标.
        /// <para>如果传入的是字符串(.<code>string</code>)将会解除对应事件名的所有事件.</para>
        /// <para>如果传入的是事件对象(.<code>IEvent</code>)那么解除对应事件.</para>
        /// <para>如果传入的是分组(.<code>object</code>)会解除该分组下的所有事件.</para>
        /// </param>
        public void Off(object target)
        {
            Guard.Requires<ArgumentNullException>(target != null);

            lock (syncRoot)
            {
                if (target is IEvent baseEvent)
                {
                    Forget(baseEvent);
                    return;
                }

                if (target is string)
                {
                    var eventName = FormatEventName(target.ToString());
                    if (IsWildcard(eventName))
                    {
                        DismissWildcardEventName(eventName);
                    }
                    else
                    {
                        DismissEventName(eventName);
                    }
                }

                DismissTargetObject(target);
            }
        }

        /// <summary>
        /// 生成事件.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        /// <param name="execution">事件执行方法.</param>
        /// <param name="group">事件分组.</param>
        /// <param name="isWildcard">是否是通配符事件.</param>
        /// <returns>todo:1.</returns>
        protected virtual IEvent MakeEvent(string eventName, Func<string, object[], object> execution, object group, bool isWildcard = false)
        {
            return new Event(eventName, group, MakeListener(execution, isWildcard));
        }

        /// <summary>
        /// 创建事件监听器.
        /// </summary>
        /// <param name="execution">事件执行器.</param>
        /// <param name="isWildcard">是否是通配符方法.</param>
        /// <returns>事件监听器.</returns>
        protected virtual Func<string, object[], object> MakeListener(Func<string, object[], object> execution, bool isWildcard = false)
        {
            return (eventName, payloads) => execution(eventName, isWildcard
                ? Arr.Merge(new object[] { eventName }, payloads)
                : payloads);
        }

        /// <summary>
        /// 格式化事件名.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        /// <returns>格式化后的事件名.</returns>
        protected virtual string FormatEventName(string eventName)
        {
            return eventName;
        }

        /// <summary>
        /// 是否是通配符事件.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        /// <returns>表示是否是通配符事件.</returns>
        private static bool IsWildcard(string eventName)
        {
            return eventName.IndexOf('*') != -1;
        }

        /// <summary>
        /// 调度事件.
        /// </summary>
        /// <param name="halt">遇到第一个事件存在处理结果后终止.</param>
        /// <param name="eventName">事件名.</param>
        /// <param name="payload">载荷.</param>
        /// <returns>处理结果.</returns>
        private object Dispatch(bool halt, string eventName, params object[] payload)
        {
            Guard.Requires<ArgumentNullException>(eventName != null);
            eventName = FormatEventName(eventName);

            lock (syncRoot)
            {
                var outputs = new List<object>(listeners.Count);

                foreach (var listener in GetListeners(eventName))
                {
                    var response = listener.Call(eventName, payload);

                    // 如果启用了事件暂停，且得到的有效的响应那么我们终止事件调用
                    if (halt && response != null)
                    {
                        return response;
                    }

                    // 如果响应内容和终止标记相同那么我们终止事件调用
                    if (response != null && response.Equals(BreakFlag))
                    {
                        break;
                    }

                    outputs.Add(response);
                }

                return halt ? null : outputs.ToArray();
            }
        }

        /// <summary>
        /// 获取指定事件的事件列表.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        /// <returns>事件列表.</returns>
        private IEnumerable<IEvent> GetListeners(string eventName)
        {
            var outputs = new List<IEvent>();

            if (listeners.TryGetValue(eventName, out List<IEvent> result))
            {
                outputs.AddRange(result);
            }

            foreach (var element in wildcardListeners)
            {
                if (element.Value.Key.IsMatch(eventName))
                {
                    outputs.AddRange(element.Value.Value);
                }
            }

            return outputs;
        }

        /// <summary>
        /// 根据普通事件解除相关事件.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        private void DismissEventName(string eventName)
        {
            if (!listeners.TryGetValue(eventName, out List<IEvent> events))
            {
                return;
            }

            foreach (var element in events.ToArray())
            {
                Forget(element);
            }
        }

        /// <summary>
        /// 根据通配符事件解除相关事件.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        private void DismissWildcardEventName(string eventName)
        {
            if (!wildcardListeners.TryGetValue(eventName, out KeyValuePair<Regex, List<IEvent>> events))
            {
                return;
            }

            foreach (var element in events.Value.ToArray())
            {
                Forget(element);
            }
        }

        /// <summary>
        /// 根据Object解除事件.
        /// </summary>
        /// <param name="target">事件解除目标.</param>
        private void DismissTargetObject(object target)
        {
            if (!groupMapping.TryGetValue(target, out List<IEvent> events))
            {
                return;
            }

            foreach (var element in events.ToArray())
            {
                Forget(element);
            }
        }

        /// <summary>
        /// 从事件调度器中移除指定的事件监听器.
        /// </summary>
        /// <param name="target">事件监听器.</param>
        private void Forget(IEvent target)
        {
            lock (syncRoot)
            {
                if (target.Group != null && groupMapping.TryGetValue(target.Group, out List<IEvent> events))
                {
                    events.Remove(target);
                    if (events.Count <= 0)
                    {
                        groupMapping.Remove(target.Group);
                    }
                }

                if (IsWildcard(target.Name))
                {
                    ForgetWildcardListen(target);
                }
                else
                {
                    ForgetListen(target);
                }
            }
        }

        /// <summary>
        /// 销毁普通事件.
        /// </summary>
        /// <param name="target">事件对象.</param>
        private void ForgetListen(IEvent target)
        {
            if (!listeners.TryGetValue(target.Name, out List<IEvent> events))
            {
                return;
            }

            events.Remove(target);
            if (events.Count <= 0)
            {
                listeners.Remove(target.Name);
            }
        }

        /// <summary>
        /// 销毁通配符事件.
        /// </summary>
        /// <param name="target">事件对象.</param>
        private void ForgetWildcardListen(IEvent target)
        {
            if (!wildcardListeners.TryGetValue(target.Name, out KeyValuePair<Regex, List<IEvent>> wildcardEvents))
            {
                return;
            }

            wildcardEvents.Value.Remove(target);
            if (wildcardEvents.Value.Count <= 0)
            {
                wildcardListeners.Remove(target.Name);
            }
        }

        /// <summary>
        /// 设定普通事件.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        /// <param name="execution">事件调用方法.</param>
        /// <param name="group">事件分组.</param>
        /// <returns>监听事件.</returns>
        private IEvent SetupListen(string eventName, Func<string, object[], object> execution, object group)
        {
            if (!listeners.TryGetValue(eventName, out List<IEvent> listener))
            {
                listeners[eventName] = listener = new List<IEvent>();
            }

            var output = MakeEvent(eventName, execution, group);
            listener.Add(output);
            return output;
        }

        /// <summary>
        /// 设定通配符事件.
        /// </summary>
        /// <param name="eventName">事件名.</param>
        /// <param name="execution">事件调用方法.</param>
        /// <param name="group">事件分组.</param>
        /// <returns>监听事件.</returns>
        private IEvent SetupWildcardListen(string eventName, Func<string, object[], object> execution, object group)
        {
            if (!wildcardListeners.TryGetValue(eventName, out KeyValuePair<Regex, List<IEvent>> listener))
            {
                wildcardListeners[eventName] = listener =
                    new KeyValuePair<Regex, List<IEvent>>(new Regex(Str.AsteriskWildcard(eventName)), new List<IEvent>());
            }

            var output = MakeEvent(eventName, execution, group, true);
            listener.Value.Add(output);
            return output;
        }
    }
}
