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

#pragma warning disable CA1031

namespace CatLib.Tests
{
    public static class ExceptionAssert
    {
        public static Exception Throws<T>(Action action)
            where T : Exception
        {
            return Throws<T>(action, null);
        }

        public static Exception Throws<T>(Action action, string message)
            where T : Exception
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                if (ex is T)
                {
                    return ex;
                }
            }

            Assert.Fail(message ?? "need throw exception");
            return null;
        }
    }
}
