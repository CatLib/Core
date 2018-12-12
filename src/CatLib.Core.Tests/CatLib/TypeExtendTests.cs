/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Core.Tests
{
    [TestClass]
    public class TypeExtendTests
    {
        [TestMethod]
        public void TestGetService()
        {
            Assert.AreEqual(App.Type2Service<TypeExtendTests>(), typeof(TypeExtendTests).ToService());
        }
    }
}
