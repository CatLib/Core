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
    /// 管理器模版（拓展解决方案为单例）- 扩展内容不对外可见
    /// </summary>
    public interface ISingleManaged<TInterface> : IManaged<TInterface>
    {
        /// <summary>
        /// 当扩展被释放时
        /// </summary>
        event Action<TInterface> OnRelease;

        /// <summary>
        /// 释放指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        void Release(string name = null);

        /// <summary>
        /// 是否包含指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>是否包含扩展实现</returns>
        bool Contains(string name = null);
    }
}
