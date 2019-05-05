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
    /// Failed to resolve the service exception.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UnresolvableException : RuntimeException
    {
        /// <summary>
        /// Initialize a new one <see cref="UnresolvableException"/> instnace.
        /// </summary>
        public UnresolvableException()
        {

        }

        /// <summary>
        /// Initialize a new one <see cref="UnresolvableException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public UnresolvableException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initialize a new one <see cref="UnresolvableException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnresolvableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
