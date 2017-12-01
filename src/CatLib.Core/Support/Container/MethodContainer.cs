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

namespace CatLib.Core.Support.Container
{
    /// <summary>
    /// 方法容器
    /// </summary>
    internal sealed class MethodContainer
    {
        /// <summary>
        /// 依赖注入容器
        /// </summary>
        private IContainer container;

        /// <summary>
        /// 构建一个新的方法容器
        /// </summary>
        /// <param name="container"></param>
        internal MethodContainer(IContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// 绑定一个方法
        /// </summary>
        /// <param name="name">通过这个名字可以调用方法</param>
        /// <param name="method">被调用的方法</param>
        /// <returns></returns>
        public int BindMethod(string name, MethodInfo method)
        {
            return 0;
        }
    }
}
