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

using System;
using System.IO;
using System.Text;

namespace CatLib
{
    /// <summary>
    /// The stream extension function.
    /// </summary>
    public static class StreamExtension
    {
        /// <summary>
        /// Append the source stream to the destination stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destination">The destination stream.</param>
        /// <returns>Byte length of transmitted data.</returns>
        public static long AppendTo(this Stream source, Stream destination)
        {
            return source.AppendTo(destination, ThreadStatic.Buffer);
        }

        /// <summary>
        /// Append the source stream to the destination stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destination">The destination stream.</param>
        /// <param name="buffer">The buffer to use.</param>
        /// <returns>Byte length of transmitted data.</returns>
        public static long AppendTo(this Stream source, Stream destination, byte[] buffer)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            Guard.Requires<ArgumentNullException>(destination != null);

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
        /// Read the stream's data and convert it to a string.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="encoding">The encoding for data.</param>
        /// <param name="closed">Whether auto closed stream.</param>
        /// <returns>The string.</returns>
        public static string ToText(this Stream source, Encoding encoding = null, bool closed = true)
        {
            Guard.Requires<ArgumentNullException>(source != null);
            try
            {
                if (!source.CanRead)
                {
                    throw new LogicException($"Can not read stream, {nameof(source.CanRead)} == false");
                }

                encoding = encoding ?? Str.Encoding;
                var memoryStream = source as MemoryStream;
                if (memoryStream != null)
                {
                    byte[] buffer;
                    try
                    {
                        buffer = memoryStream.GetBuffer();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        buffer = memoryStream.ToArray();
                    }

                    return encoding.GetString(buffer, 0, (int)memoryStream.Length);
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
