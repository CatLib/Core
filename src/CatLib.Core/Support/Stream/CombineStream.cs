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
    /// 组合流，允许将多个不同的流组合成一个流
    /// </summary>
    public class CombineStream : Stream
    {
        /// <summary>
        /// 全局游标位置
        /// </summary>
        private long globalPosition;

        /// <summary>
        /// 当前所处的流
        /// </summary>
        private int index;

        /// <summary>
        /// 组合流
        /// </summary>
        private readonly Stream[] streams;

        /// <summary>
        /// 组合流的长度
        /// </summary>
        private long length;

        /// <summary>
        /// 组合流的长度
        /// </summary>
        public override long Length
        {
            get
            {
                if (length >= 0)
                {
                    return length;
                }

                length = 0;
                foreach (var stream in streams)
                {
                    length += stream.Length;
                }
                return length;
            }
        }

        /// <summary>
        /// 是否能够偏移
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                foreach (var stream in streams)
                {
                    if (!stream.CanSeek)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 获取当前偏移量
        /// </summary>
        public override long Position
        {
            get => globalPosition;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <summary>
        /// 是否是可读的
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// 是否是可写的
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// 组合流关闭时是否自动关闭流
        /// </summary>
        private readonly bool autoClosed;

        /// <summary>
        /// 构建一个组合流实例，允许将两个不同的流组合成一个流
        /// </summary>
        /// <param name="left">流</param>
        /// <param name="right">流</param>
        /// <param name="closed">当组合流释放时是否自动关闭其中的流</param>
        public CombineStream(Stream left, Stream right, bool closed = false)
            : this(new[] { left, right }, closed)
        {
        }

        /// <summary>
        /// 构建一个组合流实例，允许将多个流组合成一个流
        /// </summary>
        /// <param name="source">流</param>
        /// <param name="closed">当组合流释放时是否自动关闭其中的流</param>
        public CombineStream(Stream[] source, bool closed = false)
        {
            index = 0;
            streams = source;
            length = -1;
            autoClosed = closed;
        }

        /// <summary>
        /// 设定位置偏移
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="origin">偏移方向</param>
        /// <returns>当前偏移量</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
            {
                throw new NotSupportedException($"{nameof(CombineStream)} not supported {nameof(Seek)}.");
            }

            long newGloablPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newGloablPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newGloablPosition = globalPosition + offset;
                    break;
                case SeekOrigin.End:
                    newGloablPosition = Length + offset;
                    break;
                default:
                    throw new NotSupportedException($"Not support {nameof(SeekOrigin)}: {origin}");
            }

            if (newGloablPosition < 0 || newGloablPosition > Length)
            {
                throw new ArgumentOutOfRangeException($"{nameof(offset)} must large than zero or small then {nameof(Length)}");
            }

            long localPosition = 0;
            var newIndex = index = CalculatedIndex(newGloablPosition, ref localPosition);

            streams[newIndex].Seek(localPosition, SeekOrigin.Begin);
            while (++newIndex < streams.Length)
            {
                streams[newIndex].Seek(0, SeekOrigin.Begin);
            }

            return globalPosition = newGloablPosition;
        }

        /// <summary>
        /// 计算偏移下标
        /// </summary>
        /// <param name="globalPosition">全局位置</param>
        /// <param name="localPosition">本地偏移量</param>
        protected int CalculatedIndex(long globalPosition, ref long localPosition)
        {
            long length = 0;
            for (var i = 0; i < streams.Length; i++)
            {
                length += streams[i].Length;
                if (globalPosition > length)
                {
                    continue;
                }

                localPosition = streams[i].Length - (length - globalPosition);
                return i;
            }

            throw new AssertException($"Failed to determine {nameof(localPosition)}");
        }

        /// <summary>
        /// 读取组合流的数据到缓冲区
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        /// <param name="offset">缓冲区偏移量</param>
        /// <param name="count">希望读取的长度</param>
        /// <returns>实际读取的长度</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(buffer.Length - offset >= count);

            var result = 0;
            do
            {
                var read = streams[index].Read(buffer, offset, count);
                if (read <= 0 && index < streams.Length - 1)
                {
                    index++;
                    continue;
                }

                if (read <= 0)
                {
                    break;
                }

                count -= read;
                offset += read;
                globalPosition += read;
                result += read;
            } while (count > 0);

            return result;
        }

        /// <summary>
        /// 写入数据到组合流
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"{nameof(CombineStream)} not supported {nameof(Write)}.");
        }

        /// <summary>
        /// 设定流的长度
        /// </summary>
        /// <param name="value">长度</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException($"{nameof(CombineStream)} not supported {nameof(SetLength)}.");
        }

        /// <summary>
        /// Flush Stream
        /// </summary>
        public override void Flush()
        {
            throw new NotSupportedException($"{nameof(CombineStream)} not supported {nameof(Flush)}.");
        }

        /// <summary>
        /// 当组合流释放时
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!autoClosed)
                {
                    return;
                }

                foreach (var stream in streams)
                {
                    stream?.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}
