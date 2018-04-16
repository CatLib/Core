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

namespace CatLib
{
    /// <summary>
    /// 环型缓冲区
    /// </summary>
    public interface IRingBuffer
    {
        /// <summary>
        /// 缓冲区容量
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// 同步锁
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        /// 可写容量
        /// </summary>
        int WriteableCapacity { get; }

        /// <summary>
        /// 可读容量
        /// </summary>
        int ReadableCapacity { get; }

        /// <summary>
        /// 是否可以进行读取
        /// </summary>
        /// <param name="count">指定的长度</param>
        bool CanRead(int count = 1);

        /// <summary>
        /// 是否可以进行写入
        /// </summary>
        /// <param name="count">指定的长度</param>
        bool CanWrite(int count = 1);

        /// <summary>
        /// 获取环型缓冲区的原始数组
        /// </summary>
        /// <returns>原始数组</returns>
        byte[] GetBuffer();

        /// <summary>
        /// 将可以读取的数据全部返回
        /// </summary>
        /// <returns>可以读取的数据</returns>
        byte[] Read();

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <returns>实际输出的长度</returns>
        int Read(byte[] buffer);

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <param name="offset">输出数组偏移多少作为起始</param>
        /// <returns>实际输出的长度</returns>
        int Read(byte[] buffer, int offset);

        /// <summary>
        /// 将数据读取到<paramref name="buffer"/>中
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <param name="offset">输出数组偏移多少作为起始</param>
        /// <param name="count">输出的长度</param>
        /// <returns>实际输出的长度</returns>
        int Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// 将环型缓冲区的数据全部返回，但是不前移读取位置
        /// </summary>
        /// <returns>实际输出的长度</returns>
        byte[] Peek();

        /// <summary>
        /// 将环型缓冲区的数据读取到<paramref name="buffer"/>中，但是不前移读取位置
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <returns>实际输出的长度</returns>
        int Peek(byte[] buffer);

        /// <summary>
        /// 将环型缓冲区的数据读取到<paramref name="buffer"/>中，但是不前移读取位置
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <param name="offset">输出数组偏移多少作为起始</param>
        /// <returns>实际输出的长度</returns>
        int Peek(byte[] buffer, int offset);

        /// <summary>
        /// 将环型缓冲区的数据读取到<paramref name="buffer"/>中，但是不前移读取位置
        /// </summary>
        /// <param name="buffer">输出的数据</param>
        /// <param name="offset">输出数组偏移多少作为起始</param>
        /// <param name="count">输出的长度</param>
        /// <returns>实际输出的长度</returns>
        int Peek(byte[] buffer, int offset, int count);

        /// <summary>
        /// 将数据写入到环型缓冲区
        /// </summary>
        /// <param name="buffer">写入的数据</param>
        /// <returns>实际被写入的长度</returns>
        int Write(byte[] buffer);

        /// <summary>
        /// 将数据写入到环型缓冲区
        /// </summary>
        /// <param name="buffer">写入的数据</param>
        /// <param name="offset">偏移多少数据开始写入</param>
        /// <returns>实际被写入的长度</returns>
        int Write(byte[] buffer, int offset);

        /// <summary>
        /// 将数据写入到环型缓冲区
        /// </summary>
        /// <param name="buffer">写入的数据</param>
        /// <param name="offset">偏移多少数据开始写入</param>
        /// <param name="count">写入的长度</param>
        /// <returns>实际被写入的长度</returns>
        int Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// 清空缓冲区中的所有数据
        /// </summary>
        void Flush();
    }
}
