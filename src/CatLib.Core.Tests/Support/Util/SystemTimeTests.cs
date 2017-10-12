/*
 * This file is part of the CatLib package.
 *
 * (c) Yu Bin <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

#if UNITY_EDITOR || NUNIT
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CatLib.Core.Tests.Support.Util
{
    [TestClass]
    public sealed class SystemTimeTests
    {
        [TestMethod]
        public void TestGetTimestamp()
        {
            var timestamp = SystemTime.UtcTime.ToLocalTime().Timestamp();
            Assert.AreEqual(0, timestamp);
        }

        [TestMethod]
        public void TestGetDateTime()
        {
            var timestamp = SystemTime.UtcTime.ToLocalTime().Timestamp();
            Assert.AreEqual(SystemTime.UtcTime.ToLocalTime().ToString(), SystemTime.ToDateTime(timestamp).ToString());
        }
    }
}
