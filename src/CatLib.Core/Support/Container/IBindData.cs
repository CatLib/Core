/*
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

namespace CatLib
{
    /// <summary>
    /// 服务绑定数据
    /// </summary>
    public interface IBindData : IBindable<IBindData>
    {
        /// <summary>
        /// 服务实现
        /// </summary>
        Func<IContainer, object[], object> Concrete { get; }

        /// <summary>
        /// 是否是静态服务
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// 为服务设定一个别名
        /// </summary>
        /// <param name="alias">别名</param>
        /// <returns>服务绑定数据</returns>
        IBindData Alias(string alias);

        /// <summary>
        /// 为服务打上一个标签
        /// </summary>
        /// <param name="tag">标签名</param>
        /// <returns>服务绑定数据</returns>
        IBindData Tag(string tag);

        /// <summary>
        /// 解决服务时触发的回调
        /// </summary>
        /// <param name="closure">解决事件</param>
        /// <returns>服务绑定数据</returns>
        IBindData OnResolving(Action<IBindData, object> closure);

        /// <summary>
        /// 解决服务时事件之后的回调
        /// </summary>
        /// <param name="closure">解决事件</param>
        /// <returns>服务绑定数据</returns>
        IBindData OnAfterResolving(Action<IBindData, object> closure);

        /// <summary>
        /// 当服务被释放时
        /// </summary>
        /// <param name="closure">处理事件</param>
        /// <returns>服务绑定数据</returns>
        IBindData OnRelease(Action<IBindData, object> closure);
    }
}
