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
using System.IO;
using System.Text;

namespace CatLib.Support.Tests
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

        [TestMethod]
        public void TestStreamToText()
        {
            var stream1 = new MemoryStream(Encoding.Default.GetBytes("Hello world"));
            Assert.AreEqual("Hello world", stream1.ToText());
        }

        [TestMethod]
        public void TestStreamToTextOtherStream()
        {
            var stream1 = new MemoryStream();
            stream1.Write(Encoding.Default.GetBytes("Hello world"), 0, 11);
            stream1.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual("Hello world", stream1.ToText());
        }

        [TestMethod]
        public void TestStreamToTextLarage()
        {
            var stream1 = new MemoryStream();
            var builder = new StringBuilder();
            var buffer = new byte[4096];
            for (var i = 0; i < (buffer.Length / 10) + 1; i++)
            {
                stream1.Write(Encoding.Default.GetBytes("1234567890"), 0, 10);
                builder.Append("1234567890");
            }

            stream1.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(builder.ToString(), stream1.ToText());
        }

        [TestMethod]
        public void TestChineseText()
        {
            var stream1 = new MemoryStream();
            var builder = new StringBuilder();
            var buffer = new byte[4096];
            for (var i = 0; i < (buffer.Length / 10) + 1; i++)
            {
                var data = Encoding.UTF8.GetBytes("12中文34测试的5这是67890");
                stream1.Write(data, 0, data.Length);
                builder.Append("12中文34测试的5这是67890");
            }

            stream1.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual(builder.ToString(), stream1.ToText());
        }

        [TestMethod]
        public void TestStreamToTextEmpty()
        {
            var stream1 = new MemoryStream(0);
            Assert.AreEqual(string.Empty, stream1.ToText());
        }

        [TestMethod]
        public void TestStreamClosed()
        {
            var stream1 = new MemoryStream(0);
            Assert.AreEqual(string.Empty, stream1.ToText(null, false));
            Assert.AreEqual(true, stream1.CanWrite);
            Assert.AreEqual(string.Empty, stream1.ToText());
            Assert.AreEqual(false, stream1.CanWrite);
        }
    }
}
