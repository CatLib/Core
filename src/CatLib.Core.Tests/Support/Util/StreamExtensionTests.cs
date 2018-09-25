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

using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Core.Tests.Support.Util
{
    [TestClass]
    public class StreamExtensionTests
    {
        [TestMethod]
        public void TestAppendTo()
        {
            var stream1 = new MemoryStream(Encoding.Default.GetBytes("Hello world"));
            var stream2 = new MemoryStream(16);

            var count = stream1.AppendTo(stream2);
            Assert.AreEqual(11, count);
            Assert.AreEqual("Hello world", Encoding.Default.GetString(stream2.GetBuffer(), 0, (int)stream2.Length));
        }
    }
}
