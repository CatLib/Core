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

namespace CatLib
{
    /// <summary>
    /// 方法容器
    /// </summary>
    internal sealed class MethodContainer
    {
        /// <summary>
        /// 依赖注入容器
        /// </summary>
        private Container container;

        /// <summary>
        /// 构建一个新的方法容器
        /// </summary>
        /// <param name="container"></param>
        internal MethodContainer(Container container)
        {
            this.container = container;
        }

        /// <summary>
        /// 绑定一个方法
        /// </summary>
        /// <param name="method">通过这个名字可以调用方法</param>
        /// <param name="target">方法调用目标</param>
        /// <param name="call">在方法调用目标中被调用的方法</param>
        /// <returns></returns>
        public IMethodBindData BindMethod(string method, object target, string call)
        {
            return null;
        }        
    }
}
