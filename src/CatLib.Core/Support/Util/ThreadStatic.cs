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
    /// 线程静态暂存对象
    /// </summary>
    public static class ThreadStatic
    {
        /// <summary>
        /// 默认的缓冲区
        /// </summary>
        [ThreadStatic]
        private static readonly byte[] buffer = new byte[4096];

        /// <summary>
        /// 默认缓冲区
        /// </summary>
        public static byte[] Buffer
        {
            get { return buffer; }
        }
    }
}
