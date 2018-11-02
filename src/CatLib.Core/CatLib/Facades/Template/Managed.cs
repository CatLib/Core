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
    public abstract class Managed<TInterface, TExtend> : Facade<TInterface>
        where TInterface : IManaged<TExtend>
    {
        /// <summary>
        /// 当扩展被实现时
        /// </summary>
        public static event Action<TExtend> OnResolving
        {
            add { Instance.OnResolving += value; }
            remove { Instance.OnResolving -= value; }
        }

        /// <summary>
        /// 自定义一个扩展构建器
        /// </summary>
        /// <param name="builder">扩展构建器</param>
        /// <param name="name">扩展名</param>
        public static void Extend(Func<TExtend> builder, string name = null)
        {
            Instance.Extend(builder, name);
        }

        /// <summary>
        /// 释放指定扩展的构建器
        /// </summary>
        /// <param name="name">扩展名</param>
        public static void RemoveExtend(string name = null)
        {
            Instance.RemoveExtend(name);
        }

        /// <summary>
        /// 是否包含指定扩展构建器
        /// </summary>
        /// <param name="name">扩展名</param>
        public static bool ContainsExtend(string name = null)
        {
            return Instance.ContainsExtend(name);
        }
    }
}
