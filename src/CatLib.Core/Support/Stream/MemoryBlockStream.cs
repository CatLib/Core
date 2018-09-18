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
        private LinkedList<ArraySegment<byte>> storage;

        /// <summary>
        /// 当前游标所处的内存区块
        /// </summary>
        private LinkedListNode<ArraySegment<byte>> current;

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
        /// 是否是可写的
        /// </summary>
        private bool writable;

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
            get { return !disabled && writable; }
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
        /// 构建一个内存块数据流实例
        /// </summary>
        protected MemoryBlockStream()
        {
            
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
        /// 构建一个内存块数据流实例
        /// </summary>
        /// <param name="maxMemoryUsable">最大内存使用量</param>
        /// <param name="blockSize">单个内存块的分块</param>
        public MemoryBlockStream(long maxMemoryUsable, int blockSize = 4096)
        {
            BlockSize = blockSize;
            MaxMemoryUsable = Math.Max(maxMemoryUsable, blockSize);
            storage = new LinkedList<ArraySegment<byte>>();
            length = 0;
            position = 0;
            disabled = false;
            writable = true;
            current = null;
        }

        /// <summary>
        /// 克隆一个只读流
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new MemoryBlockStream
            {
                storage = storage,
                BlockSize = BlockSize,
                MaxMemoryUsable = MaxMemoryUsable,
                length = length,
                position = 0,
                disabled = false,
                writable = false,
                current = storage.First
            };
        }

        /// <summary>
        /// 获取缓冲区
        /// </summary>
        /// <returns>获取缓冲区</returns>
        public byte[] GetBuffer()
        {
            AssertDisabled();

            var buffer = new byte[length];
            var data = GetNearstBlockStartOffset(length);
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

            if (tempPosition > length)
            {
                throw new IOException("seek position is large then length(" + length + ")");
            }

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
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            AssertDisabled();

            EnsureStorage(Length + count);

            var nearstBlockStartOffset = GetNearstBlockStartOffset(position);
            var nearstBlockEndOffset = nearstBlockStartOffset + BlockSize;
            var nearstBlockCurrentOffset = (int) (position - nearstBlockStartOffset);

            if (count < nearstBlockEndOffset - position)
            {
                // 如果当前区块可以写入全部的数据
                Buffer.BlockCopy(buffer, offset, current.Value.Array, nearstBlockCurrentOffset, count);
                count = 0;
            }
            else
            {
                var blockFreeSize = (int) (nearstBlockEndOffset - position);
                Buffer.BlockCopy(buffer, offset, current.Value.Array, nearstBlockCurrentOffset, blockFreeSize);
                count -= blockFreeSize;
                offset += blockFreeSize;
                current = current.Next;
                position += count;
                Guard.Requires<AssertException>(current != null);
            }

            
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
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否进行释放</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    disabled = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
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
        /// 创建缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        protected virtual ArraySegment<byte> CreateBuffer(int blockSize)
        {
            return new ArraySegment<byte>(new byte[blockSize]);
        }

        /// <summary>
        /// 释放缓冲区
        /// </summary>
        /// <param name="buffer"></param>
        protected virtual void ReleaseBuffer(ArraySegment<byte> buffer)
        {

        }

        /// <summary>
        /// 刷新当前的游标节点
        /// </summary>
        /// <param name="position">偏移量</param>
        private void RefreshCurrentNode(long position)
        {
            var nodeIndex = position / BlockSize;
            LinkedListNode<ArraySegment<byte>> node = null;

            do
            {
                Guard.Requires<AssertException>(node == null || node.Next != null);
                node = (node == null) ? storage.First : node.Next;
            } while (nodeIndex-- > 0);

            current = node;
        }

        /// <summary>
        /// 保障需要占用的内存存储空间
        /// </summary>
        /// <param name="value">需要占用的存储空间</param>
        private void EnsureStorage(long value)
        {
            AssertMemoryUseable(value);

            if (value <= 0)
            {
                return;
            }

            var minBlockCount = (value / BlockSize) + 1;
            if (storage.Count >= minBlockCount)
            {
                return;
            }

            var needsBlock = minBlockCount - storage.Count;

            while (needsBlock-- > 0)
            {
                storage.AddLast(CreateBuffer(BlockSize));
            }
        }

        /// <summary>
        /// 获取最近区块的写入点
        /// </summary>
        /// <param name="position">基础偏移量</param>
        /// <returns>最近区块的起始偏移量</returns>
        private long GetNearstBlockStartOffset(long position)
        {
            return position / BlockSize * BlockSize;
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
        /// 断言是否能够写入
        /// </summary>
        private void AssertWritable()
        {
            if (!writable)
            {
                throw new NotSupportedException("Not supported writable");
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

