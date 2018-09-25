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
    /// 基础存储
    /// </summary>
    public interface IStorage : IDisposable
    {
        /// <summary>
        /// 存储数据的长度
        /// </summary>
        long Length { get; }

        /// <summary>
        /// 读写锁
        /// </summary>
        ReaderWriterLockSlim Locker { get; }

        /// <summary>
        /// 是否应被释放
        /// </summary>
        bool Disabled { get; }

        /// <summary>
        /// 将指定缓冲区的数据写入到内存存储
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区的起始位置</param>
        /// <param name="count">缓冲区的长度</param>
        /// <param name="index">存储的起始位置</param>
        void Write(byte[] buffer, int offset, int count, long index = 0);

        /// <summary>
        /// 将指定缓冲区的数据追加到内存存储
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区的起始位置</param>
        /// <param name="count">写入的长度</param>
        void Append(byte[] buffer, int offset, int count);

        /// <summary>
        /// 读取数据到指定缓冲区
        /// </summary>
        /// <param name="buffer">指定缓冲区</param>
        /// <param name="offset">缓冲区的起始位置</param>
        /// <param name="count">读取的长度</param>
        /// <param name="index">存储的起始位置</param>
        /// <returns>实际读取的长度</returns>
        int Read(byte[] buffer, int offset, int count, long index = 0);
    }
}
