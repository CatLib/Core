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

using System.IO;
using System.Text;

namespace CatLib
{
    /// <summary>
    /// 字符串扩展
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 将指定字符串转为Stream流
        /// </summary>
        /// <param name="str">指定字符串</param>
        /// <param name="encoding">使用的编码</param>
        /// <returns></returns>
        public static Stream ToStream(this string str, Encoding encoding = null)
        {
            return new MemoryStream((encoding ?? Util.Encoding).GetBytes(str));
        }
    }
}
