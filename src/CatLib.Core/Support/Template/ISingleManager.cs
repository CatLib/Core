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
    /// 管理器（拓展解决方案为单例）
    /// </summary>
    public interface ISingleManager<TInterface> : ISingleManaged<TInterface>
    {
        /// <summary>
        /// 获取指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>扩展实现</returns>
        TInterface Get(string name = null);

        /// <summary>
        /// 获取指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>扩展实现</returns>
        TInterface this[string name] { get; }

        /// <summary>
        /// 默认的扩展实现
        /// </summary>
        TInterface Default { get; }
    }
}
