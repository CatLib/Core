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
    /// 方法绑定数据
    /// </summary>
    internal sealed class MethodBindData : Bindable<IMethodBindData> , IMethodBindData
    {
        /// <summary>
        /// 构建一个绑定数据
        /// </summary>
        /// <param name="container">依赖注入容器</param>
        /// <param name="service">服务名</param>
        public MethodBindData(Container container, string service)
            :base(container, service)
        {
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        protected override void ReleaseBind()
        {
            
        }
    }
}
