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
    /// Represents a generic runtime exception.
    /// </summary>
    public class RuntimeException : Exception
    {
        /// <summary>
        /// Initialize a new <see cref="RuntimeException"/> instnace.
        /// </summary>
        public RuntimeException()
        {

        }

        /// <summary>
        /// Initialize a new <see cref="RuntimeException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public RuntimeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="RuntimeException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}