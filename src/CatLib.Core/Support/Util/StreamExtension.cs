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
using System.IO;

namespace CatLib
{
    /// <summary>
    /// Stream扩展函数
    /// </summary>
    public static class StreamExtension
    {
        /// <summary>
        /// 默认的缓冲区
        /// </summary>
        [ThreadStatic]
        private static readonly byte[] buffer = new byte[4096];

        /// <summary>
        /// 将当前流追加到目标流中
        /// </summary>
        /// <param name="source">源数据流</param>
        /// <param name="destination">目标数据流</param>
        /// <returns>总共传输了多少数据</returns>
        public static long AppendTo(this Stream source, Stream destination)
        {
            return source.AppendTo(destination, buffer);
        }

        /// <summary>
        /// 将当前流追加到目标流中
        /// </summary>
        /// <param name="source">源数据流</param>
        /// <param name="destination">目标数据流</param>
        /// <param name="buffer">所使用的缓冲区</param>
        /// <returns>总共传输了多少数据</returns>
        public static long AppendTo(this Stream source, Stream destination, byte[] buffer)
        {
            Guard.Requires<NullReferenceException>(source != null);
            Guard.Requires<NullReferenceException>(destination != null);

            long result = 0;
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, read);
                result += read;
            }

            return result;
        }
    }
}
