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
    /// 存储数据流
    /// </summary>
    public class StorageStream : Stream
    {
        /// <summary>
        /// 当前游标所处的位置
        /// </summary>
        private long position;

        /// <summary>
        /// 是否已经被释放了
        /// </summary>
        private bool disabled;

        /// <summary>
        /// 是否是可写的
        /// </summary>
        private readonly bool writable;

        /// <summary>
        /// 存储数据
        /// </summary>
        private readonly IStorage storage;

        /// <summary>
        /// 偏移量
        /// </summary>
        public override long Position
        {
            get
            {
                AssertDisabled();
                return position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// 数据的长度
        /// </summary>
        public override long Length
        {
            get
            {
                AssertDisabled();
                return storage.Length;
            }
        }

        /// <summary>
        /// 是否是可以写入数据的
        /// </summary>
        public override bool CanWrite
        {
            get { return !Disposed && writable; }
        }

        /// <summary>
        /// 是否可以进行游标偏移
        /// </summary>
        public override bool CanSeek
        {
            get { return !Disposed; }
        }

        /// <summary>
        /// 是否可以读取数据
        /// </summary>
        public override bool CanRead
        {
            get { return !Disposed; }
        }

        /// <summary>
        /// 是否已经被释放
        /// </summary>
        protected bool Disposed
        {
            get { return disabled || storage.Disabled; }
        }

        /// <summary>
        /// 存储数据流
        /// </summary>
        /// <param name="storage">单个内存块的分块</param>
        /// <param name="writable">是否是可写的</param>
        /// <param name="timeout">锁超时时间</param>
        public StorageStream(IStorage storage, bool writable = true, int timeout = 1000)
        {
            Guard.Requires<ArgumentNullException>(storage != null);

            if (storage.Disabled)
            {
                throw new ObjectDisposedException("storage", "Storage is disposed");
            }

            this.storage = storage;
            this.writable = writable;
            position = 0;
            disabled = false;

            if (writable)
            {
                if (!storage.Locker.TryEnterWriteLock(timeout))
                {
                    throw GetOccupyException();
                }
            }
            else
            {
                if (!storage.Locker.TryEnterReadLock(timeout))
                {
                    throw GetOccupyException();
                }
            }
        }

        /// <summary>
        /// GC回收时
        /// </summary>
        ~StorageStream()
        {
            Dispose(!disabled);
        }

        /// <summary>
        /// 偏移游标到指定位置
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="origin">偏移方向</param>
        /// <returns>新的位置</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            AssertDisabled();

            long tempPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        tempPosition = offset;
                        break;
                    }
                case SeekOrigin.Current:
                    {
                        tempPosition = unchecked(position + offset);

                        break;
                    }
                case SeekOrigin.End:
                    {
                        tempPosition = unchecked(Length + offset);
                        break;
                    }
                default:
                    throw new ArgumentException("Unknow SeekOrigin");
            }

            if (tempPosition < 0)
            {
                throw new IOException("seek position less than 0");
            }

            if (tempPosition > Length)
            {
                throw new IOException("seek position is large then length(" + Length + ")");
            }

            position = tempPosition;
            return position;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="buffer">需要写入的字节流</param>
        /// <param name="offset">字节流的起始位置</param>
        /// <param name="count">需要写入的长度</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            AssertWritable();
            AssertDisabled();
            storage.Write(buffer, offset, count, position);
            position += count;
        }

        /// <summary>
        /// 读取数据到指定缓冲区
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区的起始位置</param>
        /// <param name="count">需要读取的长度</param>
        /// <returns>实际读取的长度</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            AssertDisabled();
            var read = storage.Read(buffer, offset, count, position);
            position += read;
            return read;
        }

        /// <summary>
        /// 设定长度
        /// </summary>
        /// <param name="value">新的长度值</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 清除当前流的缓冲区
        /// </summary>
        public override void Flush()
        {
            // 只有存在数据落地或者转移的情况下此函数才有效
            // 由于是内存缓存，所以这里我们忽略这个函数
        }

        /// <summary>
        /// 获取线程占用异常
        /// </summary>
        /// <returns>异常</returns>
        protected IOException GetOccupyException()
        {
            return new IOException("The resource is already occupied by other threads");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否进行释放</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing || disabled)
                {
                    return;
                }

                disabled = true;

                if (writable)
                {
                    storage.Locker.ExitWriteLock();
                }
                else
                {
                    storage.Locker.ExitReadLock();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// 断言是否已经被释放
        /// </summary>
        private void AssertDisabled()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(null, "[" + GetType() + "] Stream is closed.");
            }
        }

        /// <summary>
        /// 断言是否能够写入
        /// </summary>
        private void AssertWritable()
        {
            if (!writable)
            {
                throw new NotSupportedException("Not supported writable");
            }
        }
    }
}