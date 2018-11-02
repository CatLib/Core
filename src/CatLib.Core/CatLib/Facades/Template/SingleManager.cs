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

namespace CatLib.Facades.Template
{
    /// <summary>
    /// 管理器模版
    /// </summary>
    /// <typeparam name="TInterface">主服务接口</typeparam>
    /// <typeparam name="TExtend">扩展类型接口</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class SingleManager<TInterface, TExtend> : SingleManaged<TInterface, TExtend>
        where TInterface : ISingleManager<TExtend>
    {
        /// <summary>
        /// 获取扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>扩展实现</returns>
        public static TExtend Get(string name = null)
        {
            return Instance.Get(name);
        }

        /// <summary>
        /// 默认的扩展实现
        /// </summary>
        public static TExtend Default
        {
            get { return Instance.Default; }
        }
    }
}
