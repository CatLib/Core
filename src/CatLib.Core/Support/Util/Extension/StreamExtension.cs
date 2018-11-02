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
using System.Text;

namespace CatLib
{
    /// <summary>
    /// Stream扩展函数
    /// </summary>
    public static class StreamExtension
    {
        /// <summary>
        /// 将当前流追加到目标流中
        /// </summary>
        /// <param name="source">源数据流</param>
        /// <param name="destination">目标数据流</param>
        /// <returns>总共传输了多少数据</returns>
        public static long AppendTo(this Stream source, Stream destination)
        {
            return source.AppendTo(destination, ThreadStatic.Buffer);
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
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, read);
                result += read;
            }

            return result;
        }

        /// <summary>
        /// 将流转为字符串
        /// </summary>
        /// <param name="source">源数据流</param>
        /// <param name="encoding">编码</param>
        /// <param name="closed">是否自动关闭流</param>
        /// <returns>字符串</returns>
        public static string ToText(this Stream source, Encoding encoding = null, bool closed = true)
        {
            try
            {
                encoding = encoding ?? Util.Encoding;
                if (source is MemoryStream)
                {
                    var memoryStream = (MemoryStream) source;
                    byte[] buffer;
                    try
                    {
                        buffer = memoryStream.GetBuffer();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        buffer = memoryStream.ToArray();
                    }

                    return encoding.GetString(buffer, 0, (int) memoryStream.Length);
                }

                var length = 0;
                try
                {
                    length = (int)source.Length;
                }
                catch (NotSupportedException)
                {
                    // ignore
                }

                MemoryStream targetStream;
                if (length > 0 && length <= ThreadStatic.Buffer.Length)
                {
                    targetStream = new MemoryStream(ThreadStatic.Buffer, 0, ThreadStatic.Buffer.Length, true, true);
                }
                else
                {
                    targetStream = new MemoryStream(length);
                }

                using (targetStream)
                {
                    var read = source.AppendTo(targetStream);
                    return encoding.GetString(targetStream.GetBuffer(), 0, (int)read);
                }
            }
            finally
            {
                if (closed)
                {
                    source.Dispose();
                }
            }
        }
    }
}
