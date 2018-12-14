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

namespace CatLib.Core.Tests
{
    [TestClass]
    public class AppTests
    {
        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestCloseAutoInstance()
        {
            App.AutoInstance = false;
            try
            {
                var handler = App.Handler;
            }
            finally
            {
                App.AutoInstance = true;
            }
        }
    }
}
