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
using System.Collections.Generic;
using System.IO;

namespace CatLib
{
    /// <summary>
    /// 内存块数据流
    /// </summary>
    public class MemoryBlockStream : Stream , ICloneable
    {
        /// <summary>
        /// 最大内存使用量
        /// </summary>
        public long MaxMemoryUsable { get; private set; }

        /// <summary>
        /// 单个内存块的大小
        /// </summary>
        public int BlockSize { get; private set; }

        /// <summary>
        /// 存储的数据
        /// </summary>
        private LinkedList<byte[]> storage;

        /// <summary>
        /// 当前游标所处的内存区块
        /// </summary>
        private LinkedListNode<byte[]> current;

        /// <summary>
        /// 当前游标所处的位置
        /// </summary>
        private long position;

        /// <summary>
        /// 数据的长度
        /// </summary>
        private long length;

        /// <summary>
        /// 是否已经被释放了
        /// </summary>
        private bool disabled;

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
                AssertDisabled();
                Guard.Requires<ArgumentOutOfRangeException>(value >= 0);
                position = value;
            }
        }

        /// <summary>
        /// 总内存使用量
        /// </summary>
        public long Capacity
        {
            get
            {
                AssertDisabled();
                return storage.Count * MaxMemoryUsable;
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
                return length;
            }
        }

        /// <summary>
        /// 是否是可以写入数据的
        /// </summary>
        public override bool CanWrite
        {
            get { return !disabled; }
        }

        /// <summary>
        /// 是否可以进行游标偏移
        /// </summary>
        public override bool CanSeek
        {
            get { return !disabled; }
        }

        /// <summary>
        /// 是否可以读取数据
        /// </summary>
        public override bool CanRead
        {
            get { return !disabled; }
        }

        /// <summary>
        /// 内存块数据流
        /// </summary>
        /// <param name="blockSize">单个内存块的分块</param>
        public MemoryBlockStream(int blockSize = 4096)
            : this(long.MaxValue, blockSize)
        {

        }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return null;
        }

        /// <summary>
        /// 内存块数据流
        /// </summary>
        /// <param name="maxMemoryUsable">最大内存使用量</param>
        /// <param name="blockSize">单个内存块的分块</param>
        public MemoryBlockStream(long maxMemoryUsable, int blockSize = 4096)
        {
            BlockSize = blockSize;
            MaxMemoryUsable = Math.Max(maxMemoryUsable, blockSize);
            storage = new LinkedList<byte[]>();
            length = 0;
            position = 0;
            disabled = false;
            current = null;
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
        /// 偏移游标到指定位置
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="origin">偏移方向</param>
        /// <returns>新的位置</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
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
                    tempPosition = unchecked(length + offset);
                    break;
                }
                default:
                    throw new ArgumentException("Unknow SeekOrigin");
            }

            if (tempPosition < 0)
            {
                throw new IOException("seek position less than 0");
            }

            EnsureStorage(tempPosition);
            position = tempPosition;
            RefreshCurrentNode(position);
            return position;
        }

        /// <summary>
        /// 设定长度
        /// </summary>
        /// <param name="value">新的长度值</param>
        public override void SetLength(long value)
        {
            Guard.Requires<ArgumentOutOfRangeException>(value >= 0);
            AssertDisabled();
            EnsureStorage(value);
            length = value;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="buffer">需要写入的字节流</param>
        /// <param name="offset">字节流的起始位置</param>
        /// <param name="count">需要写入的长度</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            
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
            return 0;
        }

        /// <summary>
        /// 计算规定值最近的二的次幂的容量
        /// </summary>
        /// <param name="min">规定值</param>
        /// <returns>容量</returns>
        private static int GetPrime(int min)
        {
            min = Math.Max(0, min);

            var result = 8192;
            for (var i = 2; i < int.MaxValue; i = i << 1)
            {
                if (i >= min)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 刷新当前的游标节点
        /// </summary>
        /// <param name="position">偏移量</param>
        private void RefreshCurrentNode(long position)
        {
            // 将current 刷新至position可读节点
        }

        /// <summary>
        /// 保障需要占用的内存存储空间
        /// </summary>
        /// <param name="value">需要占用的存储空间</param>
        private void EnsureStorage(long value)
        {
            AssertMemoryUseable(value);
        }

        /// <summary>
        /// 断言是否已经被释放
        /// </summary>
        private void AssertDisabled()
        {
            if (disabled)
            {
                throw new ObjectDisposedException(null, "[" + GetType() + "] Stream is closed.");
            }
        }

        /// <summary>
        /// 断言内存使用
        /// </summary>
        /// <param name="value">内存占用</param>
        private void AssertMemoryUseable(long value)
        {
            if (value > MaxMemoryUsable)
            {
                throw new OutOfMemoryException("Memory exceeds usage limit " + (MaxMemoryUsable / 1048576) + " MB");
            }
        }
    }
}

