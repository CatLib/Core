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

using CatLib.Support;
using System;
using System.Diagnostics.CodeAnalysis;

namespace CatLib.Container
{
    /// <summary>
    /// Failed to resolve the service exception.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UnresolvableException : RuntimeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvableException"/> class.
        /// </summary>
        public UnresolvableException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvableException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public UnresolvableException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvableException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnresolvableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
