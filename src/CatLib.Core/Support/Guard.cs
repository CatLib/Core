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
using System.Reflection;

namespace CatLib.Support
{
    /// <summary>
    /// Guard the code.
    /// </summary>
    public sealed class Guard
    {
        private static Guard that;
        private static IDictionary<Type, Func<string, Exception, object, Exception>> exceptionFactory;

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
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference.</param>
        /// <param name="state">State will be passed to the registered exception build factory.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Requires<TException>(bool condition, string message = null, Exception innerException = null, object state = null)
            where TException : Exception, new()
        {
            Requires(typeof(TException), condition, message, innerException, state);
        }

        /// <summary>
        /// Verifies a condition and throws an exception if the condition of the contract fails.
        /// </summary>
        /// <param name="exception">Exception triggered when validation fails.</param>
        /// <param name="condition">The condition of the contract.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference.</param>
        /// <param name="state">State will be passed to the registered exception build factory.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Requires(Type exception, bool condition, string message = null, Exception innerException = null, object state = null)
        {
            if (condition)
            {
                return;
            }

            throw CreateExceptionInstance(exception, message, innerException, state);
        }

        /// <summary>
        /// The verification parameter is not Null.
        /// </summary>
        /// <param name="argumentValue">The parameter value.</param>
        /// <param name="argumentName">The parameter name.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void ParameterNotNull(object argumentValue, string argumentName, string message = null, Exception innerException = null)
        {
            if (argumentValue != null)
            {
                return;
            }

            message = message ?? $"Parameter {argumentName} not allowed for null. please check the function input.";
            var exception = new ArgumentNullException(argumentName, message);

            if (innerException != null)
            {
                SetField(exception, "_innerException", innerException);
            }

            throw exception;
        }

        /// <summary>
        /// Extend an exception generation factory.
        /// </summary>
        /// <typeparam name="T">The type of exception.</typeparam>
        /// <param name="factory">The exception factory.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Extend<T>(Func<string, Exception, object, Exception> factory)
        {
            Extend(typeof(T), factory);
        }

        /// <summary>
        /// Extend an exception generation factory.
        /// </summary>
        /// <param name="exception">The type of exception.</param>
        /// <param name="factory">The exception factory.</param>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Extend(Type exception, Func<string, Exception, object, Exception> factory)
        {
            VerfiyExceptionFactory();
            exceptionFactory[exception] = factory;
        }

        private static Exception CreateExceptionInstance(Type exceptionType, string message, Exception innerException, object state)
        {
            if (!typeof(Exception).IsAssignableFrom(exceptionType))
            {
                throw new ArgumentException(
                    $"Type: {exceptionType} must be inherited from: {typeof(Exception)}.",
                    nameof(exceptionType));
            }

            VerfiyExceptionFactory();

            if (exceptionFactory.TryGetValue(exceptionType, out Func<string, Exception, object, Exception> factory))
            {
                return factory(message, innerException, state);
            }

            var exception = Activator.CreateInstance(exceptionType);
            if (!string.IsNullOrEmpty(message))
            {
                SetField(exception, "_message", message);
            }

            if (innerException != null)
            {
                SetField(exception, "_innerException", innerException);
            }

            return (Exception)exception;
        }

        private static void VerfiyExceptionFactory()
        {
            if (exceptionFactory == null)
            {
                exceptionFactory = new Dictionary<Type, Func<string, Exception, object, Exception>>();
            }
        }

        private static void SetField(object obj, string field, object value)
        {
            var flag = BindingFlags.Instance | BindingFlags.NonPublic;
            var fieldInfo = obj.GetType().GetField(field, flag);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }
    }
}
