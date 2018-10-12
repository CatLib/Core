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
using System.Threading;

namespace CatLib
{
    /// <summary>
    /// 管道通讯流
    /// <para>一读一写线程安全</para>
    /// </summary>
    public class PipelineStream : Stream
    {
        /// <summary>
        /// 可以被读取的长度
        /// </summary>
        private volatile int count;

        /// <summary>
        /// 容量
        /// </summary>
        private readonly int capacity;

        /// <summary>
        /// 休眠时间
        /// </summary>
        private readonly int sleep;

        /// <summary>
        /// 环形缓冲区
        /// </summary>
        private readonly RingBuffer ringBuffer;

        /// <summary>
        /// 当完成读取后触发
        /// </summary>
        public event Action<Stream> OnRead;

        /// <summary>
        /// 是否已经被释放了
        /// </summary>
        private volatile bool disabled;

        /// <summary>
        /// 是否已经关闭流了
        /// </summary>
        private volatile bool closed;

        /// <summary>
        /// 是否可以被读取
        /// </summary>
        public override bool CanRead
        {
            get { return count > 0 && !disabled; }
        }

        /// <summary>
        /// 是否可以被写入
        /// </summary>
        public override bool CanWrite
        {
            get { return count < capacity && !closed; }
        }

        /// <summary>
        /// 当前流的位置
        /// </summary>
        private long position;

        /// <summary>
        /// 流位置
        /// </summary>
        public override long Position
        {
            get { return position; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 流的长度
        /// </summary>
        private long length;

        /// <summary>
        /// 流的长度
        /// </summary>
        public override long Length
        {
            get { return length; }
        }

        /// <summary>
        /// 是否能够设定偏移量
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// 是否已经关闭了流
        /// </summary>
        public bool Closed
        {
            get { return closed; }
        }

        /// <summary>
        /// 管道通讯流
        /// </summary>
        /// <param name="capacity">缓冲区容量</param>
        /// <param name="sleep">线程休眠时间</param>
        public PipelineStream(int capacity = 4096, int sleep = 1)
        {
            this.capacity = capacity.ToPrime();
            this.sleep = Math.Max(0, sleep);
            ringBuffer = new RingBuffer(this.capacity, false);
        }

        /// <summary>
        /// GC回收时
        /// </summary>
        ~PipelineStream()
        {
            Dispose(!disabled);
        }

        /// <summary>
        /// 设定流位置（不支持）
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="origin">偏移方向</param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 设定流的长度
        /// </summary>
        /// <param name="value">长度</param>
        public override void SetLength(long value)
        {
            length = Math.Max(0, value);
        }

        /// <summary>
        /// 刷新缓冲区
        /// </summary>
        public override void Flush()
        {
            // ignore
        }

        /// <summary>
        /// 将流中的数据读取到指定缓冲区
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区起始偏移量</param>
        /// <param name="count">读取的长度</param>
        /// <returns>实际读取的长度</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                while (this.count <= 0)
                {
                    AssertDisabled();
                    if (closed)
                    {
                        return 0;
                    }
                    Thread.Sleep(sleep);
                }

                AssertDisabled();

                lock (ringBuffer.SyncRoot)
                {
                    AssertDisabled();
                    if (this.count <= 0)
                    {
                        if (closed)
                        {
                            return 0;
                        }
                        continue;
                    }

                    try
                    {
                        var read = ringBuffer.Read(buffer, offset, count);
                        this.count -= read;
                        position += read;

                        if (OnRead != null)
                        {
                            OnRead(this);
                        }

                        return read;
                    }
                    finally
                    {
                        Guard.Requires<AssertException>(this.count >= 0);
                    }
                }
            }
        }

        /// <summary>
        /// 将指定缓冲区数据写入流中
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区起始偏移量</param>
        /// <param name="count">写入的长度</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                while ((capacity - this.count) < count)
                {
                    AssertDisabled();
                    AssertClosed();

                    Thread.Sleep(sleep);
                }

                AssertDisabled();
                AssertClosed();

                lock (ringBuffer.SyncRoot)
                {
                    AssertDisabled();
                    AssertClosed();

                    if ((capacity - this.count) < count)
                    {
                        continue;
                    }

                    Guard.Requires<AssertException>(ringBuffer.Write(buffer, offset, count) == count);
                    this.count += count;
                    return;
                }
            }
        }

        /// <summary>
        /// 关闭流
        /// </summary>
        public override void Close()
        {
            if (closed)
            {
                return;
            }

            lock (ringBuffer.SyncRoot)
            {
                closed = true;
            }
        }

        /// <summary>
        /// 断言关闭
        /// </summary>
        protected void AssertClosed()
        {
            if (closed)
            {
                throw new ObjectDisposedException("PipelineStream", "Stream is Closed Cannot write");
            }
        }

        /// <summary>
        /// 断言释放
        /// </summary>
        protected void AssertDisabled()
        {
            if (disabled)
            {
                throw new ObjectDisposedException("PipelineStream", "Stream is dispose");
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否进行释放</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing || disabled)
            {
                return;
            }

            lock (ringBuffer.SyncRoot)
            {
                if (disabled)
                {
                    return;
                }

                try
                {
                    disabled = true;
                    closed = true;
                    ringBuffer.Dispose();
                }
                finally
                {
                    base.Dispose(true);
                }
            }
        }
    }
}
