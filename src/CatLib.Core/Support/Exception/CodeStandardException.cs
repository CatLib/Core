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
    /// 代码规范异常，引发本异常一般由于不正确的使用框架
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CodeStandardException : LogicException
    {
        /// <summary>
        /// 代码规范异常
        /// </summary>
        public CodeStandardException()
        {

        }

        /// <summary>
        /// 代码规范异常
        /// </summary>
        /// <param name="message">异常消息</param>
        public CodeStandardException(string message) : base(message)
        {
        }

        /// <summary>
        /// 代码规范异常
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public CodeStandardException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}