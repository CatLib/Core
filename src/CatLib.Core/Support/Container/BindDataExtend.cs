/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

using System;

namespace CatLib
{
    ///<summary>
    /// 绑定数据拓展
    /// </summary>
    public static class BindDataExtend
    {
        /// <summary>
        /// 解决服务时触发的回调
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="action">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnResolving(this IBindData bindData, Action<object> action)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            return bindData.OnResolving((_, instance) =>
            {
                action(instance);
            });
        }

        /// <summary>
        /// 解决服务时触发的回调
        /// </summary>
        /// <typeparam name="T">指定的类型</typeparam>
        /// <param name="bindData">绑定数据</param>
        /// <param name="closure">闭包</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnResolving<T>(this IBindData bindData, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnResolving((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <summary>
        /// 解决服务时触发的回调
        /// </summary>
        /// <typeparam name="T">指定的类型</typeparam>
        /// <param name="bindData">绑定数据</param>
        /// <param name="closure">闭包</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnResolving<T>(this IBindData bindData, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnResolving((bind, instance) =>
            {
                if (instance is T)
                {
                    closure(bind, (T)instance);
                }
            });
        }

        /// <summary>
        /// 解决服务时触发的回调
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="action">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnResolving(this IBindData bindData, Action action)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            return bindData.OnResolving((_, instance) =>
            {
                action();
            });
        }

        /// <summary>
        /// 解决服务事件之后的回调
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="action">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnAfterResolving(this IBindData bindData, Action<object> action)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            return bindData.OnAfterResolving((_, instance) =>
            {
                action(instance);
            });
        }

        /// <summary>
        /// 解决服务事件之后的回调
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="closure">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnAfterResolving<T>(this IBindData bindData, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnAfterResolving((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <summary>
        /// 解决服务事件之后的回调
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="closure">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnAfterResolving<T>(this IBindData bindData, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnAfterResolving((bind, instance) =>
            {
                if (instance is T)
                {
                    closure(bind, (T)instance);
                }
            });
        }

        /// <summary>
        /// 解决服务事件之后的回调
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="action">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnAfterResolving(this IBindData bindData, Action action)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            return bindData.OnAfterResolving((_, instance) =>
            {
                action();
            });
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="action">处理事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnRelease(this IBindData bindData, Action<object> action)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            return bindData.OnRelease((_, instance) =>
            {
                action(instance);
            });
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="closure">处理事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnRelease<T>(this IBindData bindData, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnRelease((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="closure">处理事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnRelease<T>(this IBindData bindData, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnRelease((bind, instance) =>
            {
                if (instance is T)
                {
                    closure(bind, (T)instance);
                }
            });
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="action">处理事件</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData OnRelease(this IBindData bindData, Action action)
        {
            Guard.Requires<ArgumentNullException>(action != null);
            return bindData.OnRelease((_, __) =>
            {
                action();
            });
        }
    }
}
