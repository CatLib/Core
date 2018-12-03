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

using System.Collections;

namespace CatLib
{
    /// <summary>
    /// 基础服务提供者
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ServiceProvider : IServiceProvider, ICoroutineInit
    {
        /// <summary>
        /// 服务提供者初始化
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// 协同初始化
        /// </summary>
        /// <returns>迭代器</returns>
        public virtual IEnumerator CoroutineInit()
        {
            yield break;
        }

        /// <summary>
        /// 当注册服务提供者
        /// </summary>
        public virtual void Register()
        {
        }
    }
}
