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

namespace CatLib.Support.Tests
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        public void TestToStream()
        {
            Assert.AreNotEqual(null, "hello world".ToStream());
        }
    }
}
