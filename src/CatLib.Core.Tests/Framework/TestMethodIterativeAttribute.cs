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
using System.Collections.Generic;

namespace CatLib.Core.Tests.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestMethodIterativeAttribute : TestMethodAttribute
    {
        private readonly int stabilityThreshold;

        public TestMethodIterativeAttribute(int stabilityThreshold = 1)
        {
            this.stabilityThreshold = stabilityThreshold;
        }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            var results = new List<TestResult>();
            for (int count = 0; count < stabilityThreshold; count++)
            {
                var currentResults = base.Execute(testMethod);
                results.AddRange(currentResults);
            }

            return results.ToArray();
        }
    }
}
