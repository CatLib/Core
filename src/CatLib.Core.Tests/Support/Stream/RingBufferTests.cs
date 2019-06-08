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

namespace CatLib.Tests.Support.Stream
{
    [TestClass]
    public class RingBufferTests
    {
        [TestMethod]
        public void TestReadWrite()
        {
            var ringBuffer = new RingBufferStream();
            var buffer = new byte[] { 1, 2, 4, 5 };
            ringBuffer.Write(buffer, 1, 2);

            var read = Read(ringBuffer);

            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(2, read[0]);
            Assert.AreEqual(4, read[1]);

            ringBuffer = new RingBufferStream();
            buffer = new byte[] { 1, 2, 3, 4, 5 };
            ringBuffer.Write(buffer, 2, 3);
            buffer = new byte[5];
            var readCount = ringBuffer.Read(buffer, 2, buffer.Length - 2);

            Assert.AreEqual(3, readCount);
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
            Assert.AreEqual(3, buffer[2]);
            Assert.AreEqual(4, buffer[3]);
            Assert.AreEqual(5, buffer[4]);
        }

        [TestMethod]
        public void TestCanRead()
        {
            var ringBuffer = new RingBufferStream();
            var buffer = new byte[] { 1, 2, 3, 4, 5 };
            ringBuffer.Write(buffer, 0, buffer.Length);

            Assert.AreEqual(true, ringBuffer.ReadableCount >= 4);
            Assert.AreEqual(true, ringBuffer.ReadableCount >= 5);
            Assert.AreEqual(false, ringBuffer.ReadableCount >= 6);

            ringBuffer.Write(buffer, 0, buffer.Length);
            Assert.AreEqual(true, ringBuffer.ReadableCount >= 9);
            Assert.AreEqual(true, ringBuffer.ReadableCount >= 10);
            Assert.AreEqual(false, ringBuffer.ReadableCount >= 11);
        }

        [TestMethod]
        public void TestCanWrite()
        {
            var ringBuffer = new RingBufferStream(12); // 16 cap
            Assert.AreEqual(true, ringBuffer.WriteableCount >= 15);
            Assert.AreEqual(true, ringBuffer.WriteableCount >= 16);
            Assert.AreEqual(false, ringBuffer.WriteableCount >= 17);

            ringBuffer.Write(new byte[] { 1, 2, 3, 4, 5 }, 0, 5);
            Assert.AreEqual(true, ringBuffer.WriteableCount >= 10);
            Assert.AreEqual(true, ringBuffer.WriteableCount >= 11);
            Assert.AreEqual(false, ringBuffer.WriteableCount >= 12);

            ringBuffer.Write(new byte[] { 1, 2, 3, 4, 5 }, 3, 2);
            Assert.AreEqual(true, ringBuffer.WriteableCount >= 8);
            Assert.AreEqual(true, ringBuffer.WriteableCount >= 9);
            Assert.AreEqual(false, ringBuffer.WriteableCount >= 10);
        }

        [TestMethod]
        public void TestPeek()
        {
            var ringBuffer = new RingBufferStream(12); // 16 cap
            var buffer = new byte[] { 1, 2, 3, 4, 5 };
            ringBuffer.Write(buffer, 0, buffer.Length);

            Assert.AreEqual(11, ringBuffer.WriteableCount);
            Assert.AreEqual(5, ringBuffer.ReadableCount);

            var data = Peek(ringBuffer);
            Assert.AreEqual(5, data.Length);
            Assert.AreEqual(1, data[0]);
            Assert.AreEqual(2, data[1]);
            Assert.AreEqual(3, data[2]);
            Assert.AreEqual(4, data[3]);
            Assert.AreEqual(5, data[4]);

            var data2 = new byte[5];
            var read = ringBuffer.Peek(data2, 3, data2.Length - 3);
            Assert.AreEqual(2, read);
            Assert.AreEqual(5, data2.Length);
            Assert.AreEqual(0, data2[0]);
            Assert.AreEqual(0, data2[1]);
            Assert.AreEqual(0, data2[2]);
            Assert.AreEqual(1, data2[3]);
            Assert.AreEqual(2, data2[4]);

            // double peek test
            data2 = new byte[5];
            read = ringBuffer.Peek(data2, 0, data2.Length);
            Assert.AreEqual(5, read);
            Assert.AreEqual(5, data2.Length);
            Assert.AreEqual(1, data2[0]);
            Assert.AreEqual(2, data2[1]);
            Assert.AreEqual(3, data2[2]);
            Assert.AreEqual(4, data2[3]);
            Assert.AreEqual(5, data2[4]);

            Assert.AreEqual(11, ringBuffer.WriteableCount);
            Assert.AreEqual(5, ringBuffer.ReadableCount);
            Read(ringBuffer);
            Assert.AreEqual(16, ringBuffer.WriteableCount);
            Assert.AreEqual(0, ringBuffer.ReadableCount);
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeException))]
        public void TestFullBuffer()
        {
            var ringBuffer = new RingBufferStream(4); // 4 cap
            var buffer = new byte[] { 1, 2, 3, 4, 5 };

            ringBuffer.Write(buffer, 0, buffer.Length);
        }

        [TestMethod]
        public void TestReadEmptyBuffer()
        {
            var ringBuffer = new RingBufferStream(4); // 4 cap
            var read = Read(ringBuffer);
            Assert.AreEqual(0, read.Length);

            var peek = Peek(ringBuffer);
            Assert.AreEqual(0, peek.Length);

            var buffer = new byte[5];
            var r = ringBuffer.Read(buffer, 2, buffer.Length - 2);

            Assert.AreEqual(0, r);
        }

        [TestMethod]
        public void TestOutOffset()
        {
            var ringBuffer = new RingBufferStream(4); // 4 cap
            var buffer = new byte[5];
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ringBuffer.Read(buffer, 2, 10);
            });

            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                ringBuffer.Write(buffer, 2, 10);
            });
        }

        [TestMethod]
        public void TestGetBuffer()
        {
            var ringBuffer = new RingBufferStream(12); // 16 cap
            Assert.AreEqual(16, ringBuffer.GetBuffer().Length);
            ringBuffer = new RingBufferStream(12, false); // 16 cap

            ExceptionAssert.Throws<UnauthorizedAccessException>(() =>
            {
                ringBuffer.GetBuffer();
            });
        }

        [TestMethod]
        public void TestBufferReuse()
        {
            var ringBuffer = new RingBufferStream(12); // 16 cap
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            ringBuffer.Write(data, 0, data.Length);
            Read(ringBuffer);
            ringBuffer.Write(data, 0, data.Length);
            var read = Read(ringBuffer);

            Assert.AreEqual(10, read.Length);
            Assert.AreEqual(1, read[0]);
            Assert.AreEqual(10, read[9]);
        }

        [TestMethod]
        public void TestCapacity()
        {
            var ringBuffer = new RingBufferStream(18); // 32 cap
            Assert.AreEqual(32, ringBuffer.Capacity);
        }

        [TestMethod]
        public void TestClear()
        {
            var ringBuffer = new RingBufferStream(18); // 32 cap
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            ringBuffer.Write(data, 0, data.Length);
            ringBuffer.Clear();

            Assert.AreEqual(0, ringBuffer.ReadableCount);
            Assert.AreEqual(32, ringBuffer.WriteableCount);

            using (ringBuffer = new RingBufferStream(18))
            {
                data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                ringBuffer.Write(data, 0, data.Length);
            }

            Assert.AreEqual(0, ringBuffer.ReadableCount);
            Assert.AreEqual(32, ringBuffer.WriteableCount);
        }

        private static byte[] Read(RingBufferStream stream)
        {
            var buffer = new byte[stream.Length - stream.Position];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        private static byte[] Peek(RingBufferStream stream)
        {
            var buffer = new byte[stream.Length - stream.Position];
            stream.Peek(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}
