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
using System.Diagnostics.CodeAnalysis;

namespace CatLib
{
    /// <summary>
    /// Represents a logical exception encountered during execution.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogicException : RuntimeException
    {
        /// <summary>
        /// Initialize a new <see cref="LogicException"/> instnace.
        /// </summary>
        public LogicException()
        {

        }

        /// <summary>
        /// Initialize a new <see cref="LogicException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public LogicException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="LogicException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public LogicException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}