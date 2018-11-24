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
using System;
using System.IO;

namespace CatLib
{
    [TestClass]
    public class CombineStreamTests
    {
        [TestMethod]
        public void TestCombineStream()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();

            var stream = new CombineStream(mem1, mem2);
            Assert.AreEqual("helloworld", stream.ToText());
        }

        [TestMethod]
        public void TestCombineStream2()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);

            var buffer = new byte[5];
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("hello", Util.Encoding.GetString(buffer));
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("world", Util.Encoding.GetString(buffer));
        }

        [TestMethod]
        public void TestCombineSeek()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);

            var buffer = new byte[5];
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("hello", Util.Encoding.GetString(buffer));
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("world", Util.Encoding.GetString(buffer));

            stream.Seek(5, SeekOrigin.Begin);
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("world", Util.Encoding.GetString(buffer));

            stream.Seek(4, SeekOrigin.Begin);
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("oworl", Util.Encoding.GetString(buffer));
            stream.Seek(6, SeekOrigin.Begin);
            Assert.AreEqual(4, stream.Read(buffer, 0, 5));
            Assert.AreEqual("orld", Util.Encoding.GetString(buffer, 0, 4));
            stream.Seek(10, SeekOrigin.Begin);
            Assert.AreEqual(0, stream.Read(buffer, 0, 5));
            Assert.AreEqual(0, stream.Read(buffer, 0, 5));
        }

        [TestMethod]
        public void TestCombineSeek2()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);

            var buffer = new byte[5];
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("hello", Util.Encoding.GetString(buffer));
            stream.Seek(2, SeekOrigin.Current);
            Assert.AreEqual(3, stream.Read(buffer, 0, 5));
            Assert.AreEqual("rld", Util.Encoding.GetString(buffer, 0, 3));
            stream.Seek(-3, SeekOrigin.End);
            Assert.AreEqual(3, stream.Read(buffer, 0, 5));
            Assert.AreEqual("rld", Util.Encoding.GetString(buffer, 0, 3));
        }

        [TestMethod]
        public void TestSetPosition()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);

            var buffer = new byte[5];
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("hello", Util.Encoding.GetString(buffer));

            Assert.AreEqual(5, stream.Position);
            stream.Position = 0;
            Assert.AreEqual(5, stream.Read(buffer, 0, 5));
            Assert.AreEqual("hello", Util.Encoding.GetString(buffer));
        }

        [TestMethod]
        public void TestGetContstValue()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);

            Assert.AreEqual(false, stream.CanWrite);
            Assert.AreEqual(true, stream.CanSeek);
            Assert.AreEqual(true, stream.CanRead);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSeekOutOfRange()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);
            stream.Seek(11, SeekOrigin.Begin);
        }

        public class CannotSeekStream : Stream
        {
            public override bool CanRead { get; }
            public override bool CanSeek => false;

            public override bool CanWrite { get; }

            public override long Position { get; set; }

            public override long Length { get; }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void TestCannotSeekStream()
        {
            var stream = new CombineStream("hello".ToStream(), new CannotSeekStream());
            Assert.AreEqual(false, stream.CanSeek);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestCannotSeekStreamSetPosition()
        {
            var stream = new CombineStream("hello".ToStream(), new CannotSeekStream());
            stream.Seek(0, SeekOrigin.Begin);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestUnknowSeek()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);
            stream.Seek(11, (SeekOrigin)100);
        }

        [TestMethod]
        public void TestDispose()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();

            Assert.AreEqual(true, mem1.CanWrite);
            Assert.AreEqual(true, mem2.CanWrite);

            using (var stream = new CombineStream(mem1, mem2, true))
            {
            }

            Assert.AreEqual(false, mem1.CanWrite);
            Assert.AreEqual(false, mem2.CanWrite);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestWrite()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);

            stream.Write(new byte[5], 0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetLength()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);

            stream.SetLength(10);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestFlush()
        {
            var mem1 = "hello".ToStream();
            var mem2 = "world".ToStream();
            var stream = new CombineStream(mem1, mem2);

            stream.Flush();
        }
    }
}
