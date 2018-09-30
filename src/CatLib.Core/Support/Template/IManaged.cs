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

namespace CatLib
{
    /// <summary>
    /// 管理器模版 - 扩展内容不对外可见
    /// </summary>
    /// <typeparam name="TInterface">拓展接口</typeparam>
    public interface IManaged<TInterface>
    {
        /// <summary>
        /// 当扩展被实现时
        /// </summary>
        event Action<TInterface> OnResolving;

        /// <summary>
        /// 自定义一个扩展构建器
        /// </summary>
        /// <param name="builder">扩展构建器</param>
        /// <param name="name">扩展名</param>
        void Extend(Func<TInterface> builder, string name = null);

        /// <summary>
        /// 释放指定扩展的构建器
        /// </summary>
        /// <param name="name">扩展名</param>
        [Obsolete("Please use RemoveExtend();")]
        void ReleaseExtend(string name = null);

        /// <summary>
        /// 释放指定扩展的构建器
        /// </summary>
        /// <param name="name">扩展名</param>
        void RemoveExtend(string name = null);

        /// <summary>
        /// 是否包含指定扩展构建器
        /// </summary>
        /// <param name="name">扩展名</param>
        bool ContainsExtend(string name = null);
    }
}
