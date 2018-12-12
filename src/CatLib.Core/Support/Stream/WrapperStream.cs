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

namespace CatLib
{
    /// <summary>
    /// 包装流
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class WrapperStream : Stream
    {
        /// <summary>
        /// 基础流
        /// </summary>
        public Stream BaseStream { get; }

        /// <summary>
        /// 流是否可读
        /// </summary>
        public override bool CanRead => BaseStream.CanRead;

        /// <summary>
        /// 流是否可以偏移
        /// </summary>
        public override bool CanSeek => BaseStream.CanSeek;

        /// <summary>
        /// 流是否可写
        /// </summary>
        public override bool CanWrite => BaseStream.CanWrite;

        /// <summary>
        /// 流的偏移位置
        /// </summary>
        public override long Position
        {
            get => BaseStream.Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <summary>
        /// 流的长度
        /// </summary>
        public override long Length => BaseStream.Length;

        /// <summary>
        /// 构建一个包装流
        /// </summary>
        public WrapperStream()
        {
            BaseStream = this;
        }

        /// <summary>
        /// 构建一个包装流
        /// </summary>
        /// <param name="stream">基础流</param>
        public WrapperStream(Stream stream)
        {
            Guard.Requires<ArgumentNullException>(stream != null);
            BaseStream = stream;
        }

        /// <summary>
        /// 偏移流到指定位置
        /// </summary>
        /// <param name="offset">指定位置</param>
        /// <param name="origin">偏移方向</param>
        /// <returns>新的位置</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        /// <summary>
        /// 刷新流的缓冲区
        /// </summary>
        public override void Flush()
        {
            BaseStream.Flush();
        }

        /// <summary>
        /// 在流中写入指定缓冲区的数据
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">指定缓冲区偏移量</param>
        /// <param name="count">写入的长度</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// 设定流的长度
        /// </summary>
        /// <param name="value">长度</param>
        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        /// <summary>
        /// 读取流的数据到指定缓冲区
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">指定缓冲区偏移量</param>
        /// <param name="count">读取的长度</param>
        /// <returns>实际读取的长度</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }
    }
}
