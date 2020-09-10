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

using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.IO
{
    [TestClass]
    public class TestsStreamExtension
    {
        [TestMethod]
        public void TestAppendTo()
        {
            var stream1 = "foo".ToStream();
            var stream2 = new MemoryStream();

            Assert.AreEqual(3, stream1.AppendTo(stream2));
            Assert.AreEqual("foo", stream2.ToText());
        }

        [TestMethod]
        public void TestStreamToText()
        {
            var stream = "foo".ToStream();
            Assert.AreEqual("foo", stream.ToText());
        }

        [TestMethod]
        public void TestStreamToTextLarage()
        {
            var stream = new MemoryStream();
            var builder = new StringBuilder();

            var buffer = new byte[4096];
            for (var i = 0; i < (buffer.Length / 10) + 1; i++)
            {
                stream.Write(Encoding.Default.GetBytes("1234567890"), 0, 10);
                builder.Append("1234567890");
            }

            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(builder.ToString(), stream.ToText());
        }

        [TestMethod]
        public void TestDoubleWidthText()
        {
            var stream = new MemoryStream();
            var builder = new StringBuilder();

            var buffer = new byte[4096];
            for (var i = 0; i < (buffer.Length / 10) + 1; i++)
            {
                var data = Encoding.UTF8.GetBytes("12双宽度34测试567890");
                stream.Write(data, 0, data.Length);
                builder.Append("12双宽度34测试567890");
            }

            stream.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(builder.ToString(), stream.ToText());
        }

        [TestMethod]
        public void TestStreamToTextEmpty()
        {
            var stream = new MemoryStream(0);
            Assert.AreEqual(string.Empty, stream.ToText());
        }

        [TestMethod]
        public void TestStreamClosed()
        {
            var stream = new MemoryStream(0);

            Assert.AreEqual(string.Empty, stream.ToText(null, false));
            Assert.AreEqual(true, stream.CanWrite);
            Assert.AreEqual(string.Empty, stream.ToText());
            Assert.AreEqual(false, stream.CanWrite);
        }
    }
}
