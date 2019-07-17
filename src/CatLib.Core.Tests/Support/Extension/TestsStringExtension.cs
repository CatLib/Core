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

using CatLib.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Support
{
    [TestClass]
    public class TestsStringExtension
    {
        [TestMethod]
        public void TestToStream()
        {
            var stream = "foo".ToStream();
            Assert.AreEqual("foo", stream.ToText());
        }
    }
}
