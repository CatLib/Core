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
using System.Collections.Generic;

namespace CatLib
{
    /// <summary>
    /// Guard the code.
    /// </summary>
    public sealed class Guard
    {
        private static Guard that;

        /// <summary>
        /// Gets the singleton instance of the Guard functionality.
        /// </summary>
        /// <remarks>Users can use this to plug-in custom assertions through csharp extension methods.</remarks>
        public static Guard That
        {
            get
            {
                if (that == null)
                {
                    that = new Guard();
                }

                return that;
            }
        }

        /// <summary>
        /// Verifies a condition and throws an exception if the condition of the contract fails.
        /// </summary>
        /// <typeparam name="TException">Exception triggered when validation fails.</typeparam>
        /// <param name="condition">The condition of the contract.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Requires<TException>(bool condition) where TException : Exception, new()
        {
            if (condition)
            {
                return;
            }
            throw new TException();
        }

        /// <summary>
        /// Verifies is not empty or null.
        /// </summary>
        /// <param name="argumentValue">The parameter name.</param>
        /// <param name="argumentName">The parameter name.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void NotEmptyOrNull(string argumentValue, string argumentName)
        {
            if (string.IsNullOrEmpty(argumentValue))
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Verifies the length is greater than 0
        /// </summary>
        /// <typeparam name="T">The type of parameter.</typeparam>
        /// <param name="argumentValue">The parameter value.</param>
        /// <param name="argumentName">The parameter name.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void CountGreaterZero<T>(IList<T> argumentValue, string argumentName)
        {
            if (argumentValue.Count <= 0)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Verifies the element not empty or null.
        /// </summary>
        /// <param name="argumentValue">The parameter value.</param>
        /// <param name="argumentName">The parameter name.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void ElementNotEmptyOrNull(IList<string> argumentValue, string argumentName)
        {
            foreach (var val in argumentValue)
            {
                if (string.IsNullOrEmpty(val))
                {
                    throw new ArgumentNullException(argumentName, $"Argument element can not be {nameof(string.Empty)} or null.");
                }
            }
        }

        /// <summary>
        /// Verifies the parameter not null.
        /// </summary>
        /// <param name="argumentValue">The parameter value.</param>
        /// <param name="argumentName">The parameter name.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void NotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}