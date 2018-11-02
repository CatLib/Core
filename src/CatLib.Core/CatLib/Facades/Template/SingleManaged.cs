/*
 * Code Generation File 2018/10/16
 */

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

namespace CatLib.Facades.Template
{
    /// <summary>
    /// 管理器模版
    /// </summary>
    /// <typeparam name="TInterface">主服务接口</typeparam>
    /// <typeparam name="TExtend">扩展类型接口</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class SingleManaged<TInterface, TExtend> : Managed<TInterface, TExtend>
        where TInterface : ISingleManaged<TExtend>
    {
        /// <summary>
        /// 当释放时
        /// </summary>
        public static event Action<TExtend> OnRelease
        {
            add { Instance.OnRelease += value; }
            remove { Instance.OnRelease -= value; }
        }

        /// <summary>
        /// 释放指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        public static void Release(string name = null)
        {
            Instance.Release(name);
        }

        /// <summary>
        /// 是否包含指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>是否包含扩展实现</returns>
        public static bool Contains(string name = null)
        {
            return Instance.Contains(name);
        }
    }
}
