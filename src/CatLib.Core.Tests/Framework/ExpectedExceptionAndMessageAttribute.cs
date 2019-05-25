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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CatLib.Tests
{
    /// <summary>
    /// Expectation exception class that allows developers to make exception content decisions.
    /// </summary>
    public class ExpectedExceptionAndMessageAttribute : ExpectedExceptionBaseAttribute
    {
        private readonly Type expectedExceptionType;
        private readonly string expectedExceptionMessage;
        private readonly bool strict;

        public ExpectedExceptionAndMessageAttribute(Type expectedExceptionType)
        {
            this.expectedExceptionType = expectedExceptionType;
            expectedExceptionMessage = string.Empty;
        }

        public ExpectedExceptionAndMessageAttribute(Type expectedExceptionType, string expectedExceptionMessage, bool strict = false)
        {
            this.expectedExceptionType = expectedExceptionType;
            this.expectedExceptionMessage = expectedExceptionMessage;
            this.strict = strict;
        }

        protected override void Verify(Exception exception)
        {
            Assert.IsNotNull(exception);

            do
            {
                if (expectedExceptionType.IsAssignableFrom(exception.GetType()))
                {
                    break;
                }

                exception = exception.InnerException;
            }
            while (exception != null);

            Assert.IsInstanceOfType(exception, expectedExceptionType, "Wrong type of exception was thrown.");

            if (!expectedExceptionMessage.Length.Equals(0))
            {
                var message = exception.Message.Replace(Environment.NewLine, "\n", StringComparison.Ordinal);
                if (strict)
                {
                    Assert.AreEqual(expectedExceptionMessage, message, "Wrong exception message was returned.");
                }
                else
                {
                    if (message.IndexOf(
                        expectedExceptionMessage,
                        StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        Assert.AreEqual(expectedExceptionMessage, message, "Wrong exception message was returned.");
                    }
                }
            }
        }
    }
}
