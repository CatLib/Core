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
using System.Threading;

namespace CatLib
{
    /// <summary>
    /// 内存存储
    /// </summary>
    public class MemoryStorage : IStorage
    {
        /// <summary>
        /// 区块元数据
        /// </summary>
        private class BlockMeta
        {
            /// <summary>
            /// 区块下标
            /// </summary>
            public int BlockIndex;

            /// <summary>
            /// 起始偏移量
            /// </summary>
            public long StartOffset;

            /// <summary>
            /// 终止偏移量
            /// </summary>
            public long EndOffset;

            /// <summary>
            /// 存储数据
            /// </summary>
            public ArraySegment<byte> Storage;

            /// <summary>
            /// 获取相对偏移量
            /// </summary>
            /// <param name="position">全局偏移量</param>
            /// <returns>相对偏移量</returns>
            public int GetRelativeOffset(long position)
            {
                return (int)(position - StartOffset);
            }

            /// <summary>
            /// 获取当前区块剩余量
            /// </summary>
            /// <param name="position">全局偏移量</param>
            /// <returns>剩余量</returns>
            public int GetFreeSize(long position)
            {
                return (int)(EndOffset - position);
            }
        }

        /// <summary>
        /// 当前存储最大内存使用量
        /// </summary>
        public long MaxMemoryUsable { get; private set; }

        /// <summary>
        /// 单个内存块的大小
        /// </summary>
        public int BlockSize { get; private set; }

        /// <summary>
        /// 数据长度
        /// </summary>
        public long Length
        {
            get
            {
                AssertDisabled();
                return length;
            }
        }

        /// <summary>
        /// 存储的数据
        /// </summary>
        private BlockMeta[] storage;

        /// <summary>
        /// 实际存储的长度
        /// </summary>
        private long length;

        /// <summary>
        /// 读写锁
        /// </summary>
        private ReaderWriterLockSlim locker;

        /// <summary>
        /// 读写锁
        /// </summary>
        public ReaderWriterLockSlim Locker
        {
            get
            {
                AssertDisabled();
                return locker ?? (locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion));
            }
        }

        /// <summary>
        /// 是否已经被释放的
        /// </summary>
        public bool Disabled { get; private set; }

        /// <summary>
        /// 构建一个新的内存存储
        /// </summary>
        /// <param name="blockBuffer">块缓冲区大小</param>
        /// <param name="capacity">起始块数量</param>
        public MemoryStorage(int blockBuffer = 4096, int capacity = 16)
            : this(long.MaxValue, blockBuffer, capacity)
        {

        }

        /// <summary>
        /// 构建一个新的内存存储
        /// </summary>
        /// <param name="maxMemoryUsable">最大内存使用量</param>
        /// <param name="blockBuffer">块缓冲区大小</param>
        /// <param name="capacity">起始块数量</param>
        public MemoryStorage(long maxMemoryUsable, int blockBuffer = 4096, int capacity = 16)
        {
            MaxMemoryUsable = maxMemoryUsable;
            BlockSize = blockBuffer;
            length = 0;
            storage = new BlockMeta[GetPrime(capacity)];
        }

        /// <summary>
        /// 将指定缓冲区的数据写入到内存存储
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区的起始位置</param>
        /// <param name="count">缓冲区的长度</param>
        /// <param name="index">内存存储的起始位置</param>
        public void Write(byte[] buffer, int offset, int count, long index = 0)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(index >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(buffer.Length - offset >= count);
            AssertDisabled();

            EnsureStorageBlock(index + count);
            var blockMeta = GetBlockByPosition(index);

            do
            {
                if (count <= blockMeta.EndOffset - index)
                {
                    // 如果当前区块可以写入全部的数据
                    Buffer.BlockCopy(buffer, offset, blockMeta.Storage.Array, blockMeta.GetRelativeOffset(index),
                        count);
                    index += count;
                    count = 0;
                }
                else
                {
                    var blockFreeSize = blockMeta.GetFreeSize(index);
                    Buffer.BlockCopy(buffer, offset, blockMeta.Storage.Array, blockMeta.GetRelativeOffset(index),
                        blockFreeSize);
                    index += blockFreeSize;
                    offset += blockFreeSize;
                    count -= blockFreeSize;
                    blockMeta = GetBlockByIndex(blockMeta.BlockIndex + 1);
                }
            } while (count > 0);

            length = Math.Max(length, index + count);
        }

        /// <summary>
        /// 将指定缓冲区的数据追加到内存存储
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区的起始位置</param>
        /// <param name="count">缓冲区的长度</param>
        public void Append(byte[] buffer, int offset, int count)
        {
            Write(buffer, offset, count, length);
        }

        /// <summary>
        /// 读取数据到指定缓冲区
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区的起始位置</param>
        /// <param name="count">读取的长度</param>
        /// <param name="index">存储的起始位置</param>
        /// <returns>实际读取的长度</returns>
        public int Read(byte[] buffer, int offset, int count, long index = 0)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(index >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(buffer.Length - offset >= count);
            AssertDisabled();

            var read = 0;
            var blockMeta = GetBlockByPosition(index, true);

            if (count + index > length)
            {
                count = (int)(length - index);
            }

            if (count <= 0)
            {
                return 0;
            }

            do
            {
                if (blockMeta == null)
                {
                    // 如果是跳空块那么直接给定数据
                    Array.Clear(buffer, offset, BlockSize);
                    read += BlockSize;
                    index += BlockSize;
                    offset += BlockSize;
                    count -= BlockSize;
                    blockMeta = GetBlockByPosition(index, true);
                    continue;
                }

                var startIndex = blockMeta.GetRelativeOffset(index);
                var blockFreeSize = blockMeta.GetFreeSize(index);

                if (count <= blockFreeSize)
                {
                    Buffer.BlockCopy(blockMeta.Storage.Array, startIndex, buffer, offset, count);
                    read += count;
                    count = 0;
                }
                else
                {
                    Buffer.BlockCopy(blockMeta.Storage.Array, startIndex, buffer, offset, blockFreeSize);
                    read += blockFreeSize;
                    index += blockFreeSize;
                    offset += blockFreeSize;
                    count -= blockFreeSize;
                    blockMeta = GetBlockByIndex(blockMeta.BlockIndex + 1, true);
                }
            } while (count > 0);

            return read;
        }

        /// <summary>
        /// 获取指定位置的区块下标
        /// </summary>
        /// <param name="position">指定位置</param>
        /// <param name="allowNull">是否允许为空</param>
        /// <returns>区块数据</returns>
        private BlockMeta GetBlockByPosition(long position, bool allowNull = false)
        {
            return GetBlockByIndex((int)(position / BlockSize));
        }

        /// <summary>
        /// 获取指定区块下标的区块数据
        /// </summary>
        /// <param name="index">区块下标</param>
        /// <param name="allowNull">是否允许为空</param>
        /// <returns>区块数据</returns>
        private BlockMeta GetBlockByIndex(int index, bool allowNull = false)
        {
            Guard.Requires<AssertException>(index < storage.Length);
            return storage[index] ?? (allowNull ? null : (storage[index] = CreateBlock(BlockSize, index)));
        }

        /// <summary>
        /// GC回收时
        /// </summary>
        ~MemoryStorage()
        {
            Dispose(!Disabled);
        }

        /// <summary>
        /// 释放内存存储
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 创建缓冲区
        /// </summary>
        /// <returns>缓冲区</returns>
        private BlockMeta CreateBlock(int blockSize, int index)
        {
            return new BlockMeta
            {
                BlockIndex = index,
                StartOffset = index * blockSize,
                EndOffset = (index + 1) * blockSize,
                Storage = CreateBuffer(blockSize)
            };
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
        /// <param name="blockMeta">区块元数据</param>
        private void ReleaseBlock(BlockMeta blockMeta)
        {
            if (blockMeta != null)
            {
                ReleaseBuffer(blockMeta.Storage);
            }
        }

        /// <summary>
        /// 释放缓冲区
        /// </summary>
        /// <param name="buffer">缓冲大小</param>
        protected virtual void ReleaseBuffer(ArraySegment<byte> buffer)
        {

        }

        /// <summary>
        /// 保障存储块满足存储条件
        /// </summary>
        /// <param name="value">总共需要占用的空间</param>
        private void EnsureStorageBlock(long value)
        {
            AssertMemoryUseable(value);

            if (value <= 0)
            {
                return;
            }

            var minBlockCount = (int) ((value / BlockSize) + ((value % BlockSize) == 0 ? 0 : 1));
            if (storage.Length >= minBlockCount)
            {
                return;
            }

            var newStorage = new BlockMeta[GetPrime(minBlockCount)];
            Array.Copy(storage, 0, newStorage, 0, storage.Length);
            storage = newStorage;
        }

        /// <summary>
        /// 释放内存存储
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || Disabled)
            {
                return;
            }

            if (locker != null)
            {
                locker.Dispose();
                locker = null;
            }

            Disabled = true;
            foreach (var block in storage)
            {
                ReleaseBlock(block);
            }
            storage = null;
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
        /// 断言是否已经被释放
        /// </summary>
        private void AssertDisabled()
        {
            if (Disabled)
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
                if (MaxMemoryUsable >= 1048576)
                {
                    throw new OutOfMemoryException("Memory exceeds usage limit " + (MaxMemoryUsable / 1048576) + " MB");
                }

                if (MaxMemoryUsable >= 1024)
                {
                    throw new OutOfMemoryException("Memory exceeds usage limit " + (MaxMemoryUsable / 1024) + " KB");
                }

                throw new OutOfMemoryException("Memory exceeds usage limit " + MaxMemoryUsable + " bit");
            }
        }
    }
}
