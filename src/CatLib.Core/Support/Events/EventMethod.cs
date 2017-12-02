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

using System.Reflection;

namespace CatLib
{
    /// <summary>
    /// 事件方法
    /// </summary>
    internal sealed class EventMethod
    {
        /// <summary>
        /// 依赖注入容器
        /// </summary>
        private readonly IContainer container;

        /// <summary>
        /// 方法
        /// </summary>
        private readonly MethodInfo method;

        /// <summary>
        /// 实例目标
        /// </summary>
        private readonly object target;

        /// <summary>
        /// 创建一个事件句柄
        /// </summary>
        /// <param name="container">依赖注入容器</param>
        /// <param name="target">调用源</param>
        /// <param name="method">调用方法名</param>
        public EventMethod(IContainer container, object target, string method)
        {
            this.container = container;
            this.target = target;
            this.method = target.GetType().GetMethod(method, 
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="args">方法参数</param>
        public object Call(params object[] args)
        {
            return container.Call(target, method, args);
        }
    }
}
