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

using CatLib.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;
using SException = System.Exception;

namespace CatLib.Tests
{
    /// <summary>
    /// Expected exception with the specified message.
    /// </summary>
    public class ExpectedExceptionAndMessageAttribute : ExpectedExceptionBaseAttribute
    {
        private readonly Type expectedExceptionType;
        private readonly string expectedExceptionMessage;
        private readonly bool strict;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedExceptionAndMessageAttribute"/> class.
        /// </summary>
        /// <param name="expectedExceptionType">The expected exception.</param>
        /// <param name="expectedExceptionMessage">The expected exception message.</param>
        /// <param name="strict">Whether strict match exception message. otherwise it will pass if it contains the exception message.</param>
        public ExpectedExceptionAndMessageAttribute(Type expectedExceptionType, string expectedExceptionMessage, bool strict = false)
        {
            this.expectedExceptionType = expectedExceptionType;
            this.expectedExceptionMessage = expectedExceptionMessage.Replace(Environment.NewLine, "\n", StringComparison.OrdinalIgnoreCase);
            this.strict = strict;
        }

        protected override void Verify(SException exception)
        {
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, expectedExceptionType, "Wrong type of exception was thrown.");

            if (expectedExceptionMessage.Length <= 0)
            {
                return;
            }

            var message = exception.Message.Replace(Environment.NewLine, "\n", StringComparison.OrdinalIgnoreCase);
            if (strict || !Is(expectedExceptionMessage, message))
            {
                Assert.AreEqual(expectedExceptionMessage, message, "Wrong exception message was returned.");
            }
        }

        private static bool Is(string pattern, string value)
        {
            return pattern == value || Regex.IsMatch(value, Str.AsteriskWildcard(pattern));
        }
    }
}
