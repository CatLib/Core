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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Core.Tests.Support.Container
{
    [TestClass]
    class MethodContainerTests
    {
        [TestMethod]
        public void TestBindMethod()
        {
            App.BindMethod("TestMethod", () =>
            {
                return null;
            });
        }
    }
}
