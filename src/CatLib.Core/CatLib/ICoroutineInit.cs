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

using System.Collections;

namespace CatLib
{
    /// <summary>
    /// 协同初始化
    /// </summary>
    public interface ICoroutineInit
    {
        /// <summary>
        /// 服务提供者初始化
        /// </summary>
        IEnumerator CoroutineInit();
    }
}
