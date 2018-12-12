/*
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

namespace CatLib
{
    /// <summary>
    /// 代码逻辑异常
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogicException : RuntimeException
    {
        /// <summary>
        /// 代码逻辑异常
        /// </summary>
        public LogicException()
        {

        }

        /// <summary>
        /// 代码逻辑异常
        /// </summary>
        /// <param name="message">异常消息</param>
        public LogicException(string message) : base(message)
        {
        }

        /// <summary>
        /// 代码逻辑异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public LogicException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}