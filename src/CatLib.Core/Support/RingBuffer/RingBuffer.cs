﻿/*
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
using System.Threading;

// todo: rebuild.
namespace CatLib
{
    /// <summary>
    /// 环型缓冲区.
    /// </summary>
    public sealed class RingBuffer : IRingBuffer, IDisposable
    {
        /// <summary>
        /// 容量.
        /// </summary>
        private readonly long capacity;

        /// <summary>
        /// 缓冲区大小.
        /// </summary>
        private readonly byte[] buffer;

        /// <summary>
        /// 原始数组是否可以返回给开发者.
        /// </summary>
        private readonly bool exposable;

        /// <summary>
        /// 遮罩层.
        /// <para>为了快速计算出,环回中的写入点.</para>
        /// </summary>
        private readonly long mask;

        /// <summary>
        /// 写入的游标.
        /// </summary>
        private long write;

        /// <summary>
        /// 读取的游标.
        /// </summary>
        private long read;

        /// <summary>
        /// 同步锁.
        /// </summary>
        private object syncRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="RingBuffer"/> class.
        /// </summary>
        /// <param name="capacity">容量,将为临近2的次方(向上取）.</param>
        /// <param name="exposable">是否可以访问内部数组.</param>
        public RingBuffer(int capacity = 8192, bool exposable = true)
        {
            Guard.Requires<ArgumentOutOfRangeException>(capacity > 0);
#pragma warning disable S1121
            buffer = new byte[this.capacity = capacity.ToPrime()];
#pragma warning restore S1121
            mask = this.capacity - 1;
            write = 0;
            read = 0;
            this.exposable = exposable;
        }

        /// <summary>
        /// Gets 缓冲区容量.
        /// </summary>
        public int Capacity => (int)capacity;

        /// <summary>
        /// Gets 同步锁.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    Interlocked.CompareExchange(ref syncRoot, new object(), null);
                }

                return syncRoot;
            }
        }

        /// <summary>
        /// Gets 可写容量.
        /// </summary>
        public int WriteableCapacity => (int)GetCanWriteSize();

        /// <summary>
        /// Gets 可读容量.
        /// </summary>
        public int ReadableCapacity => (int)GetCanReadSize();

        /// <summary>
        /// 是否可以进行读取.
        /// </summary>
        /// <param name="count">指定的长度.</param>
        /// <returns>todo:1.</returns>
        public bool CanRead(int count = 1)
        {
            Guard.Requires<ArgumentOutOfRangeException>(capacity > 0);
            return GetCanReadSize() >= count;
        }

        /// <summary>
        /// 是否可以进行写入.
        /// </summary>
        /// <param name="count">指定的长度.</param>
        /// <returns>todo:1.</returns>
        public bool CanWrite(int count = 1)
        {
            Guard.Requires<ArgumentOutOfRangeException>(capacity > 0);
            return GetCanWriteSize() >= count;
        }

        /// <summary>
        /// 获取环型缓冲区的原始数组.
        /// </summary>
        /// <returns>原始数组.</returns>
        public byte[] GetBuffer()
        {
            if (!exposable)
            {
                throw new UnauthorizedAccessException("Unable to access original array");
            }

            return buffer;
        }

        /// <summary>
        /// 将可以读取的数据全部返回.
        /// </summary>
        /// <returns>可以读取的数据.</returns>
        public byte[] Read()
        {
            var internalBuffer = MakeReadableBuffer();
            if (internalBuffer == null)
            {
                return Array.Empty<byte>();
            }

            Read(internalBuffer);
            return internalBuffer;
        }

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中.
        /// </summary>
        /// <param name="buffer">输出的数据.</param>
        /// <returns>实际输出的长度.</returns>
        public int Read(byte[] buffer)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中.
        /// </summary>
        /// <param name="buffer">输出的数据.</param>
        /// <param name="offset">输出数组偏移多少作为起始.</param>
        /// <returns>实际输出的长度.</returns>
        public int Read(byte[] buffer, int offset)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Read(buffer, offset, buffer.Length - offset);
        }

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中.
        /// </summary>
        /// <param name="buffer">输出的数据.</param>
        /// <param name="offset">输出数组偏移多少作为起始.</param>
        /// <param name="count">输出的长度.</param>
        /// <returns>实际输出的长度.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            var readSize = Peek(buffer, offset, count);
            read += readSize;
            return readSize;
        }

        /// <summary>
        /// 将环型缓冲区的数据全部返回，但是不前移读取位置.
        /// </summary>
        /// <returns>实际输出的长度.</returns>
        public byte[] Peek()
        {
            var internalBuffer = MakeReadableBuffer();
            if (internalBuffer == null)
            {
                return Array.Empty<byte>();
            }

            Peek(internalBuffer, 0, internalBuffer.Length);
            return internalBuffer;
        }

        /// <summary>
        /// 将环型缓冲区的数据读取到<paramref name="buffer"/>中，但是不前移读取位置.
        /// </summary>
        /// <param name="buffer">输出的数据.</param>
        /// <returns>实际输出的长度.</returns>
        public int Peek(byte[] buffer)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Peek(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 将环型缓冲区的数据读取到<paramref name="buffer"/>中，但是不前移读取位置.
        /// </summary>
        /// <param name="buffer">输出的数据.</param>
        /// <param name="offset">输出数组偏移多少作为起始.</param>
        /// <returns>实际输出的长度.</returns>
        public int Peek(byte[] buffer, int offset)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Peek(buffer, offset, buffer.Length - offset);
        }

        /// <summary>
        /// 将环型缓冲区的数据读取到<paramref name="buffer"/>中，但是不前移读取位置.
        /// </summary>
        /// <param name="buffer">输出的数据.</param>
        /// <param name="offset">输出数组偏移多少作为起始.</param>
        /// <param name="count">输出的长度.</param>
        /// <returns>实际输出的长度.</returns>
        public int Peek(byte[] buffer, int offset, int count)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            Guard.Requires<ArgumentOutOfRangeException>((buffer.Length - offset) >= count);

            var readSize = GetCanReadSize();
            if (readSize > count)
            {
                readSize = count;
            }

            if (readSize <= 0)
            {
                return 0;
            }

            var nextReadPos = read + readSize;

            var realReadPos = read & mask;
            var realNextReadPos = nextReadPos & mask;

            if (realNextReadPos >= realReadPos)
            {
                Buffer.BlockCopy(this.buffer, (int)realReadPos, buffer, offset, (int)readSize);
            }
            else
            {
                var tail = (int)(capacity - realReadPos);
                Buffer.BlockCopy(this.buffer, (int)realReadPos, buffer, offset, tail);

                if (readSize - tail > 0)
                {
                    Buffer.BlockCopy(this.buffer, 0, buffer, offset + tail, (int)readSize - tail);
                }
            }

            return (int)readSize;
        }

        /// <summary>
        /// 将数据写入到环型缓冲区.
        /// </summary>
        /// <param name="buffer">写入的数据.</param>
        /// <returns>实际被写入的长度.</returns>
        public int Write(byte[] buffer)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 将数据写入到环型缓冲区.
        /// </summary>
        /// <param name="buffer">写入的数据.</param>
        /// <param name="offset">偏移多少数据开始写入.</param>
        /// <returns>实际被写入的长度.</returns>
        public int Write(byte[] buffer, int offset)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Write(buffer, offset, buffer.Length - offset);
        }

        /// <summary>
        /// 将数据写入到环型缓冲区.
        /// </summary>
        /// <param name="buffer">写入的数据.</param>
        /// <param name="offset">偏移多少数据开始写入.</param>
        /// <param name="count">写入的长度.</param>
        /// <returns>实际被写入的长度.</returns>
        public int Write(byte[] buffer, int offset, int count)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            Guard.Requires<ArgumentOutOfRangeException>(offset >= 0);
            Guard.Requires<ArgumentOutOfRangeException>(count >= 0);
            Guard.Requires<ArgumentOutOfRangeException>((buffer.Length - offset) >= count);

            // 得到可以被写入的字节流大小
            var writeSize = GetCanWriteSize();
            if (writeSize > count)
            {
                writeSize = count;
            }

            if (writeSize <= 0)
            {
                return 0;
            }

            // 当前输入结束后下一次开始的写入点
            var nextWritePos = write + writeSize;

            // 通过&运算遮罩快速获得环回中的写入点
            var realWritePos = write & mask;
            var realNextWritePos = nextWritePos & mask;

            if (realNextWritePos >= realWritePos)
            {
                // 不会产生环回,只需要单纯写入
                Buffer.BlockCopy(buffer, offset, this.buffer, (int)realWritePos, (int)writeSize);
            }
            else
            {
                // 从写入位置到buffer流尾部的长度
                var tail = (int)(capacity - realWritePos);
                Buffer.BlockCopy(buffer, offset, this.buffer, (int)realWritePos, tail);

                if ((writeSize - tail) > 0)
                {
                    Buffer.BlockCopy(buffer, offset + tail, this.buffer, 0, (int)writeSize - tail);
                }
            }

            write = nextWritePos;
            return (int)writeSize;
        }

        /// <summary>
        /// 清空缓冲区中的所有数据.
        /// </summary>
        public void Flush()
        {
            write = 0;
            read = 0;
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 释放.
        /// </summary>
        public void Dispose()
        {
            Flush();
        }

        /// <summary>
        /// 获取可以被读取的字节流大小.
        /// </summary>
        /// <returns>todo:1.</returns>
        private long GetCanReadSize()
        {
            return write - read;
        }

        /// <summary>
        /// 得到可以被写入的字节流大小.
        /// </summary>
        private long GetCanWriteSize()
        {
            return Math.Max(0, capacity - GetCanReadSize());
        }

        /// <summary>
        /// 获取当前可读的buffer.
        /// </summary>
        /// <returns>可以被读取的buffer.</returns>
        private byte[] MakeReadableBuffer()
        {
            var readSize = GetCanReadSize();
            return readSize <= 0 ? null : new byte[readSize];
        }
    }
}
