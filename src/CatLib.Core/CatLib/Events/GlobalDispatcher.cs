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
    /// 全局事件系统
    /// </summary>
    internal sealed class GlobalDispatcher : Dispatcher, IGlobalDispatcher
    {
        /// <summary>
        /// 依赖解决器
        /// </summary>
        private readonly Func<ParameterInfo[], object[], object[]> dependResolved;

        /// <summary>
        /// 构建一个新的全局事件系统实例
        /// </summary>
        /// <param name="dependResolved">依赖解决器</param>
        public GlobalDispatcher(Func<ParameterInfo[], object[], object[]> dependResolved)
        {
            Guard.Requires<ArgumentNullException>(dependResolved != null);
            this.dependResolved = dependResolved;
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent On(string eventName, Action action, object group = null)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            return On(eventName, (e, userParams) =>
            {
                action.Invoke();
                return null;
            }, group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent On<T0>(string eventName, Action<T0> action, object group = null)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            var paramInfos = action.Method.GetParameters();
            return On(eventName, (e, userParams) =>
            {
                var args = dependResolved(paramInfos, userParams);
                action.Invoke((T0)args[0]);
                return null;
            }, group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent On<T0, T1>(string eventName, Action<T0, T1> action, object group = null)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            var paramInfos = action.Method.GetParameters();
            return On(eventName, (e, userParams) =>
            {
                var args = dependResolved(paramInfos, userParams);
                action.Invoke((T0)args[0], (T1)args[1]);
                return null;
            }, group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent On<T0, T1, T2>(string eventName, Action<T0, T1, T2> action, object group = null)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            var paramInfos = action.Method.GetParameters();
            return On(eventName, (e, userParams) =>
            {
                var args = dependResolved(paramInfos, userParams);
                action.Invoke((T0)args[0], (T1)args[1], (T2)args[2]);
                return null;
            }, group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent On<T0, T1, T2, T3>(string eventName, Action<T0, T1, T2, T3> action, object group = null)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            var paramInfos = action.Method.GetParameters();
            return On(eventName, (e, userParams) =>
            {
                var args = dependResolved(paramInfos, userParams);
                action.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3]);
                return null;
            }, group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent Listen<TResult>(string eventName, Func<TResult> func, object group = null)
        {
            Guard.Requires<ArgumentNullException>(func != null);
            return On(eventName, (e, userParams) => func.Invoke(), group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent Listen<T0, TResult>(string eventName, Func<T0, TResult> func, object group = null)
        {
            Guard.Requires<ArgumentNullException>(func != null);
            var paramInfos = func.Method.GetParameters();
            return On(eventName, (e, userParams) =>
            {
                var args = dependResolved(paramInfos, userParams);
                return func.Invoke((T0)args[0]);
            }, group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent Listen<T0, T1, TResult>(string eventName, Func<T0, T1, TResult> func, object group = null)
        {
            Guard.Requires<ArgumentNullException>(func != null);
            var paramInfos = func.Method.GetParameters();
            return On(eventName, (e, userParams) =>
            {
                var args = dependResolved(paramInfos, userParams);
                return func.Invoke((T0)args[0], (T1)args[1]);
            }, group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent Listen<T0, T1, T2, TResult>(string eventName, Func<T0, T1, T2, TResult> func, object group = null)
        {
            Guard.Requires<ArgumentNullException>(func != null);
            var paramInfos = func.Method.GetParameters();
            return On(eventName, (e, userParams) =>
            {
                var args = dependResolved(paramInfos, userParams);
                return func.Invoke((T0)args[0], (T1)args[1], (T2)args[2]);
            }, group);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent Listen<T0, T1, T2, T3, TResult>(string eventName, Func<T0, T1, T2, T3, TResult> func, object group = null)
        {
            Guard.Requires<ArgumentNullException>(func != null);
            var paramInfos = func.Method.GetParameters();
            return On(eventName, (e, userParams) =>
            {
                var args = dependResolved(paramInfos, userParams);
                return func.Invoke((T0)args[0], (T1)args[1], (T2)args[2], (T3)args[3]);
            }, group);
        }
    }
}
