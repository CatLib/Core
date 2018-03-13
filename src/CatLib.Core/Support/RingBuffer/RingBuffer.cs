using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CatLib
{
    /// <summary>
    /// 环型缓冲区
    /// </summary>
    public sealed class RingBufferStream : IDisposable
    {
        /// <summary>
        /// 容量
        /// </summary>
        private readonly long capacity;

        /// <summary>
        /// 缓冲区容量
        /// </summary>
        public int Capacity
        {
            get { return (int)capacity; }
        }

        /// <summary>
        /// 缓冲区大小
        /// </summary>
        private readonly byte[] buffer;

        /// <summary>
        /// 原始数组是否可以返回给开发者
        /// </summary>
        private readonly bool exposable;

        /// <summary>
        /// 写入的游标
        /// </summary>
        private long write;

        /// <summary>
        /// 读取的游标
        /// </summary>
        private long read;

        /// <summary>
        /// 遮罩层
        /// <para>为了快速计算出,环回中的写入点</para>
        /// </summary>
        private readonly long mask;

        /// <summary>
        /// 同步锁
        /// </summary>
        private object syncRoot;

        /// <summary>
        /// 同步锁
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
        /// 可以进行读取
        /// </summary>
        public bool CanRead
        {
            get { return GetCanReadSize() > 0; }
        }

        /// <summary>
        /// 是否可以进行写入
        /// </summary>
        public bool CanWrite()
        {
            return CanWrite(1);
        }

        /// <summary>
        /// 是否可以进行写入
        /// </summary>
        /// <param name="count">指定的长度</param>
        public bool CanWrite(int count)
        {
            return GetCanWriteSize() >= count;
        }

        /// <summary>
        /// 构建一个新的环型缓冲区实例
        /// </summary>
        /// <param name="capacity">容量</param>
        /// <param name="exposable">是否可以访问内部数组</param>
        public RingBufferStream(int capacity, bool exposable = true)
        {
            buffer = new byte[this.capacity = GetPrime(capacity)];
            mask = this.capacity - 1;
            write = 0;
            read = 0;
            this.exposable = exposable;
        }

        /// <summary>
        /// 获取环型缓冲区的原始数组
        /// </summary>
        /// <returns>原始数组</returns>
        public byte[] GetBuffer()
        {
            if (!exposable)
            {
                throw new UnauthorizedAccessException("Unable to access original array");
            }
            return buffer;
        }

        /// <summary>
        /// 将可以读取的数据全部返回
        /// </summary>
        /// <returns>可以读取的数据</returns>
        public byte[] Read()
        {
            var readSize = GetCanReadSize();
            if (readSize <= 0)
            {
                return null;
            }

            var result = new byte[readSize];
            Read(result);
            return result;
        }

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <returns>实际输出的长度</returns>
        public int Read(byte[] buffer)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <param name="offset">输出数组偏移多少作为起始</param>
        /// <returns>实际输出的长度</returns>
        public int Read(byte[] buffer, int offset)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Read(buffer, offset, buffer.Length - offset);
        }

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <param name="offset">输出数组偏移多少作为起始</param>
        /// <param name="count">输出的长度</param>
        /// <returns>实际输出的长度</returns>
        public int Read(byte[] buffer, int offset, int count)
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

            read = nextReadPos;
            return (int)readSize;
        }

        /// <summary>
        /// 将数据写入到环型缓冲区
        /// </summary>
        /// <param name="buffer">写入的数据</param>
        /// <returns>实际被写入的长度</returns>
        public int Write(byte[] buffer)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 将数据写入到环型缓冲区
        /// </summary>
        /// <param name="buffer">写入的数据</param>
        /// <param name="offset">偏移多少数据开始写入</param>
        /// <returns>实际被写入的长度</returns>
        public int Write(byte[] buffer, int offset)
        {
            Guard.Requires<ArgumentNullException>(buffer != null);
            return Write(buffer, offset, buffer.Length - offset);
        }

        /// <summary>
        /// 将数据写入到环型缓冲区
        /// </summary>
        /// <param name="buffer">写入的数据</param>
        /// <param name="offset">偏移多少数据开始写入</param>
        /// <param name="count">写入的长度</param>
        /// <returns>实际被写入的长度</returns>
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
        /// 清空缓冲区中的所有数据
        /// </summary>
        public void Flush()
        {
            write = 0;
            read = 0;
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            Flush();
        }

        /// <summary>
        /// 获取可以被读取的字节流大小
        /// </summary>
        /// <returns></returns>
        private long GetCanReadSize()
        {
            return write - read;
        }

        /// <summary>
        /// 得到可以被写入的字节流大小
        /// </summary>
        private long GetCanWriteSize()
        {
            return Math.Max(0, capacity - GetCanReadSize());
        }

        /// <summary>
        /// 计算规定值最近的二的次幂的容量
        /// </summary>
        /// <param name="min">规定值值</param>
        /// <returns>容量</returns>
        private static int GetPrime(int min)
        {
            min = Math.Max(0, min);

            for (var i = 2; i < int.MaxValue; i = i << 1)
            {
                if (i >= min)
                {
                    return i;
                }
            }

            throw new RuntimeException("Can not get prime");
        }
    }
}
