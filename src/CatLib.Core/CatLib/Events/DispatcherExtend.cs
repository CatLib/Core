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
using System.Reflection;

namespace CatLib
{
    /// <summary>
    /// 事件调度器扩展方法
    /// </summary>
    public static class CatLibDispatcherExtend
    {
        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="target">事件调用目标</param>
        /// <param name="method">事件处理方法</param>
        /// <returns>事件对象</returns>
        public static IEvent On(this IDispatcher dispatcher, string eventName, object target, string method = null)
        {
            Guard.NotEmptyOrNull(eventName, "eventName");
            Guard.Requires<ArgumentException>(method != string.Empty);
            Guard.Requires<ArgumentNullException>(target != null);

            var methodInfo = target.GetType().GetMethod(method ?? Str.Method(eventName), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Guard.Requires<ArgumentNullException>(methodInfo != null);
            return dispatcher.On(eventName, target, methodInfo, target);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="target">事件调度目标</param>
        /// <param name="methodInfo">事件调度方法</param>
        /// <param name="group">事件分组</param>
        /// <returns></returns>
        public static IEvent On(this IDispatcher dispatcher, string eventName, object target, MethodInfo methodInfo, object group = null)
        {
            Guard.NotEmptyOrNull(eventName, "eventName");
            Guard.Requires<ArgumentNullException>(methodInfo != null);

            if (!methodInfo.IsStatic)
            {
                Guard.Requires<ArgumentNullException>(target != null);
            }

            return dispatcher.On(eventName, (_, userParams) => App.Call(target, methodInfo, userParams), group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent On(this IDispatcher dispatcher, string eventName, Action method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                    ? globalDispatcher.On(eventName, method, group)
                    : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent On<T1>(this IDispatcher dispatcher, string eventName, Action<T1> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.On(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent On<T1, T2>(this IDispatcher dispatcher, string eventName, Action<T1, T2> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.On(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent On<T1, T2, T3>(this IDispatcher dispatcher, string eventName, Action<T1, T2, T3> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.On(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent On<T1, T2, T3, T4>(this IDispatcher dispatcher, string eventName, Action<T1, T2, T3, T4> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.On(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent Listen<TResult>(this IDispatcher dispatcher, string eventName, Func<TResult> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.Listen(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent Listen<T1, TResult>(this IDispatcher dispatcher, string eventName, Func<T1, TResult> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.Listen(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent Listen<T1, T2, TResult>(this IDispatcher dispatcher, string eventName, Func<T1, T2, TResult> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.Listen(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent Listen<T1, T2, T3, TResult>(this IDispatcher dispatcher, string eventName, Func<T1, T2, T3, TResult> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.Listen(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="dispatcher">事件调度器</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="method">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public static IEvent Listen<T1, T2, T3, T4, TResult>(this IDispatcher dispatcher, string eventName, Func<T1, T2, T3, T4, TResult> method, object group = null)
        {
            Guard.Requires<ArgumentNullException>(method != null);
#if CATLIB_PERFORMANCE
            var globalDispatcher = ToGlobalDispatcher(dispatcher);
            return globalDispatcher != null
                ? globalDispatcher.Listen(eventName, method, group)
                : dispatcher.On(eventName, method.Target, method.Method, group);
#else
            return dispatcher.On(eventName, method.Target, method.Method, group);
#endif
        }

        /// <summary>
        /// 将调度器转为全局调度器
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        internal static IGlobalDispatcher ToGlobalDispatcher(this IDispatcher dispatcher)
        {
            var originalDispatcher = dispatcher as IOriginalDispatcher;
            var globalDispatcher = originalDispatcher == null
                ? null
                : originalDispatcher.Dispatcher as IGlobalDispatcher;
            if (globalDispatcher != null)
            {
                return globalDispatcher;
            }

            return dispatcher as IGlobalDispatcher;
        }
    }
}
