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

using System;

namespace CatLib
{
    /// <summary>
    /// 全局事件系统
    /// </summary>
    internal interface IGlobalDispatcher
    {
        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent On(string eventName, Action action, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent On<T0>(string eventName, Action<T0> action, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent On<T0, T1>(string eventName, Action<T0, T1> action, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent On<T0, T1, T2>(string eventName, Action<T0, T1, T2> action, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent On<T0, T1, T2, T3>(string eventName, Action<T0, T1, T2, T3> action, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent Listen<TResult>(string eventName, Func<TResult> func, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent Listen<T0, TResult>(string eventName, Func<T0, TResult> func, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent Listen<T0, T1, TResult>(string eventName, Func<T0, T1, TResult> func, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent Listen<T0, T1, T2, TResult>(string eventName, Func<T0, T1, T2, TResult> func, object group = null);

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="func">事件处理方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        IEvent Listen<T0, T1, T2, T3, TResult>(string eventName, Func<T0, T1, T2, T3, TResult> func,
            object group = null);
    }
}
