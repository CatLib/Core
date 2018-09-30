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
    /// 管理器模版（拓展解决方案为单例）
    /// </summary>
    public abstract class SingleManager<TInterface> : SingleManaged<TInterface>, ISingleManager<TInterface>
    {
        /// <summary>
        /// 获取指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>扩展实现</returns>
        public TInterface Get(string name = null)
        {
            return MakeExtend(name);
        }

        /// <summary>
        /// 默认的扩展实现
        /// </summary>
        public TInterface Default
        {
            get
            {
                return this[null];
            }
        }

        /// <summary>
        /// 获取指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>扩展实现</returns>
        public TInterface this[string name]
        {
            get
            {
                return Get(name);
            }
        }
    }
}
