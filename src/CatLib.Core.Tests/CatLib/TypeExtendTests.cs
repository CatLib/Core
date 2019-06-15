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

namespace CatLib.Tests
{
    [TestClass]
    public class TypeExtendTests
    {
        [TestMethod]
        public void TestGetService()
        {
            var app = new Application();
            Assert.AreEqual(App.Type2Service<TypeExtendTests>(), typeof(TypeExtendTests).ToService());
        }
    }
}
