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

namespace CatLib.Support
{
    /// <summary>
    /// Represents a logical exception encountered during execution.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogicException : RuntimeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogicException"/> class.
        /// </summary>
        public LogicException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public LogicException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public LogicException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
