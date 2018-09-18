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
/*
namespace CatLib
{
    
    /// <summary>
    /// 内存块数据流
    /// </summary>
    public class MemoryBlockStream : Stream
    {
        /// <summary>
        /// 单个内存块的大小
        /// </summary>
        public int BlockSize { get; private set; }

        /// <summary>
        /// 内存块数据流
        /// </summary>
        /// <param name="blockSize">单个内存块的分块</param>
        public MemoryBlockStream(int blockSize = 4096)
        {
            BlockSize = blockSize;
        }

        /// <summary>
        /// 计算规定值最近的二的次幂的容量
        /// </summary>
        /// <param name="min">规定值值</param>
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
    }
}
*/
