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
using System;
using System.IO;
using System.Text;
using System.Threading;

#pragma warning disable CA1720

namespace CatLib.Support.Tests
{
    [TestClass]
    public class PipelineStreamTests
    {
        private PipelineStream stream;

        [TestMethod]
        public void TestPipeline()
        {
            stream = new PipelineStream(256);
            ThreadPool.QueueUserWorkItem(WriteThread);

            var readed = false;
            stream.OnRead += (_) =>
            {
                readed = true;
            };
            var data = new byte[100];
            int read;
            var actual = new StringBuilder();
            var rand = new Random();
            while ((read = stream.Read(data, 0, data.Length)) != 0)
            {
                var str = Encoding.UTF8.GetString(data, 0, read);
                actual.Append(str);
                Thread.Sleep(rand.Next(1, 5));
            }

            stream.Dispose();

            var expected = new StringBuilder();
            for (var i = 0; i < 1000; i++)
            {
                expected.Append("0123456789");
            }

            Assert.AreEqual(expected.ToString(), actual.ToString());
            Assert.AreEqual(true, readed);
        }

        public void WriteThread(object obj)
        {
            var data = Encoding.UTF8.GetBytes("0123456789");
            for (var i = 0; i < 1000; i++)
            {
                stream.Write(data, 0, data.Length);
            }

            stream.Close();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestClosedAndWrite()
        {
            var stream = new PipelineStream(256);
            Assert.AreEqual(true, stream.CanWrite);
            stream.Write(Encoding.UTF8.GetBytes("0123456789"), 0, 10);
            stream.Close();
            Assert.AreEqual(false, stream.CanWrite);
            Assert.AreEqual(true, stream.CanRead);
            Assert.AreEqual(true, stream.Closed);
            stream.Write(Encoding.UTF8.GetBytes("0123456789"), 0, 10);
        }


        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestDisposeAndWrite()
        {
            var stream = new PipelineStream(256);
            Assert.AreEqual(true, stream.CanWrite);
            Assert.AreEqual(false, stream.CanRead);
            stream.Dispose();
            Assert.AreEqual(false, stream.CanWrite);
            Assert.AreEqual(false, stream.CanRead);
            stream.Write(Encoding.UTF8.GetBytes("0123456789"), 0, 10);
        }

        [TestMethod]
        public void TestCanReadCanWrite()
        {
            var stream = new PipelineStream(256);
            stream.Write(Encoding.UTF8.GetBytes("0123456789"), 0, 10);
            Assert.AreEqual(true, stream.CanWrite);
            Assert.AreEqual(true, stream.CanRead);
        }

        [TestMethod]
        public void TestCanSeek()
        {
            var stream = new PipelineStream(256);
            Assert.AreEqual(false, stream.CanSeek);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSeek()
        {
            var stream = new PipelineStream(256);
            stream.Seek(0, SeekOrigin.Begin);
        }

        [TestMethod]
        public void TestSetLength()
        {
            var stream = new PipelineStream(256);
            stream.SetLength(0);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetPosition()
        {
            var stream = new PipelineStream(256);
            stream.Position = 10;
        }

        [TestMethod]
        public void TestGetPosition()
        {
            var stream = new PipelineStream(256);
            Assert.AreEqual(0, stream.Position);
        }

        [TestMethod]
        public void TestGetLength()
        {
            var stream = new PipelineStream(256);
            Assert.AreEqual(0, stream.Length);
        }
    }
}
