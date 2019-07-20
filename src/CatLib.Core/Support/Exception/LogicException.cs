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
using System.Runtime.Serialization;

namespace CatLib.Support
{
    /// <summary>
    /// Represents a logical exception encountered during execution.
    /// </summary>
    /// <remarks>Logical exceptions are caused by logical errors during runtime.</remarks>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicException"/> class.
        /// </summary>
        /// <param name="serializationInfo">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="streamingContext">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected LogicException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
