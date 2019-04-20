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
    /// Represents a code specification exception that raises this exception due to incorrect use of the framework.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CodeStandardException : LogicException
    {
        /// <summary>
        /// Initialize a new <see cref="CodeStandardException"/> instnace.
        /// </summary>
        public CodeStandardException()
        {

        }

        /// <summary>
        /// Initialize a new <see cref="CodeStandardException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public CodeStandardException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="CodeStandardException"/> instnace.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CodeStandardException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}