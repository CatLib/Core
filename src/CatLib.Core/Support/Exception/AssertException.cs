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
    /// Represents an assertion exception.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AssertException : RuntimeException
    {
        /// <summary>
        /// Initialize a new <see cref="AssertException"/> instnace.
        /// </summary>
        public AssertException()
        {
        }

        /// <summary>
        /// Initialize a new <see cref="AssertException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public AssertException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="AssertException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public AssertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
