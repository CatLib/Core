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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Stl
{
    [TestClass]
    public class RingBufferTests
    {
        [TestMethod]
        public void TestReadWrite()
        {
            var ringBuffer = new RingBuffer();
            var buffer = new byte[] { 1, 2, 4, 5 };
            ringBuffer.Write(buffer, 1, 2);
            var read = ringBuffer.Read();

            Assert.AreEqual(2, read.Length);
            Assert.AreEqual(2, read[0]);
            Assert.AreEqual(4, read[1]);

            ringBuffer = new RingBuffer();
            buffer = new byte[] { 1, 2, 3, 4, 5 };
            ringBuffer.Write(buffer, 2);
            buffer = new byte[5];
            var readCount = ringBuffer.Read(buffer, 2);

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
            var ringBuffer = new RingBuffer();
            var buffer = new byte[] { 1, 2, 3, 4, 5 };
            ringBuffer.Write(buffer);

            Assert.AreEqual(true, ringBuffer.CanRead(4));
            Assert.AreEqual(true, ringBuffer.CanRead(5));
            Assert.AreEqual(false, ringBuffer.CanRead(6));
            ringBuffer.Write(buffer);
            Assert.AreEqual(true, ringBuffer.CanRead(9));
            Assert.AreEqual(true, ringBuffer.CanRead(10));
            Assert.AreEqual(false, ringBuffer.CanRead(11));
        }

        [TestMethod]
        public void TestCanWrite()
        {
            var ringBuffer = new RingBuffer(12); // 16 cap
            Assert.AreEqual(true, ringBuffer.CanWrite(15));
            Assert.AreEqual(true, ringBuffer.CanWrite(16));
            Assert.AreEqual(false, ringBuffer.CanWrite(17));

            ringBuffer.Write(new byte[] { 1, 2, 3, 4, 5 });
            Assert.AreEqual(true, ringBuffer.CanWrite(10));
            Assert.AreEqual(true, ringBuffer.CanWrite(11));
            Assert.AreEqual(false, ringBuffer.CanWrite(12));

            ringBuffer.Write(new byte[] { 1, 2, 3, 4, 5 }, 3);
            Assert.AreEqual(true, ringBuffer.CanWrite(8));
            Assert.AreEqual(true, ringBuffer.CanWrite(9));
            Assert.AreEqual(false, ringBuffer.CanWrite(10));
        }

        [TestMethod]
        public void TestPeek()
        {
            var ringBuffer = new RingBuffer(12); // 16 cap
            var buffer = new byte[] { 1, 2, 3, 4, 5 };
            ringBuffer.Write(buffer);

            Assert.AreEqual(11, ringBuffer.WriteableCapacity);
            Assert.AreEqual(5, ringBuffer.ReadableCapacity);

            var data = ringBuffer.Peek();
            Assert.AreEqual(5, data.Length);
            Assert.AreEqual(1, data[0]);
            Assert.AreEqual(2, data[1]);
            Assert.AreEqual(3, data[2]);
            Assert.AreEqual(4, data[3]);
            Assert.AreEqual(5, data[4]);

            var data2 = new byte[5];
            var read = ringBuffer.Peek(data2, 3);
            Assert.AreEqual(2, read);
            Assert.AreEqual(5, data2.Length);
            Assert.AreEqual(0, data2[0]);
            Assert.AreEqual(0, data2[1]);
            Assert.AreEqual(0, data2[2]);
            Assert.AreEqual(1, data2[3]);
            Assert.AreEqual(2, data2[4]);

            // double peek test
            data2 = new byte[5];
            read = ringBuffer.Peek(data2);
            Assert.AreEqual(5, read);
            Assert.AreEqual(5, data2.Length);
            Assert.AreEqual(1, data2[0]);
            Assert.AreEqual(2, data2[1]);
            Assert.AreEqual(3, data2[2]);
            Assert.AreEqual(4, data2[3]);
            Assert.AreEqual(5, data2[4]);

            Assert.AreEqual(11, ringBuffer.WriteableCapacity);
            Assert.AreEqual(5, ringBuffer.ReadableCapacity);
            ringBuffer.Read();
            Assert.AreEqual(16, ringBuffer.WriteableCapacity);
            Assert.AreEqual(0, ringBuffer.ReadableCapacity);
        }

        [TestMethod]
        public void TestFullBuffer()
        {
            var ringBuffer = new RingBuffer(4); // 4 cap
            var buffer = new byte[] { 1, 2, 3, 4, 5 };

            var write = ringBuffer.Write(buffer);
            Assert.AreEqual(4, write);

            Assert.AreEqual(0, ringBuffer.WriteableCapacity);
            Assert.AreEqual(4, ringBuffer.ReadableCapacity);

            write = ringBuffer.Write(buffer);
            Assert.AreEqual(0, write);

            Assert.AreEqual(0, ringBuffer.WriteableCapacity);
            Assert.AreEqual(4, ringBuffer.ReadableCapacity);
        }

        [TestMethod]
        public void TestReadEmptyBuffer()
        {
            var ringBuffer = new RingBuffer(4); // 4 cap
            var read = ringBuffer.Read();
            Assert.AreEqual(0, read.Length);

            var peek = ringBuffer.Peek();
            Assert.AreEqual(0, peek.Length);

            var buffer = new byte[5];
            var r = ringBuffer.Read(buffer, 2);

            Assert.AreEqual(0, r);
        }

        [TestMethod]
        public void TestOutOffset()
        {
            var ringBuffer = new RingBuffer(4); // 4 cap
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
            var ringBuffer = new RingBuffer(12); // 16 cap
            Assert.AreEqual(16, ringBuffer.GetBuffer().Length);
            ringBuffer = new RingBuffer(12, false); // 16 cap

            ExceptionAssert.Throws<UnauthorizedAccessException>(() =>
            {
                ringBuffer.GetBuffer();
            });
        }

        [TestMethod]
        public void TestBufferReuse()
        {
            var ringBuffer = new RingBuffer(12); // 16 cap
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            ringBuffer.Write(data);
            ringBuffer.Read();
            ringBuffer.Write(data);
            var read = ringBuffer.Read();

            Assert.AreEqual(10, read.Length);
            Assert.AreEqual(1, read[0]);
            Assert.AreEqual(10, read[9]);
        }

        [TestMethod]
        public void TestGetAsync()
        {
            var ringBuffer = new RingBuffer(12); // 16 cap
            Assert.AreEqual(ringBuffer.SyncRoot, ringBuffer.SyncRoot);
        }

        [TestMethod]
        public void TestCapacity()
        {
            var ringBuffer = new RingBuffer(18); // 32 cap
            Assert.AreEqual(32, ringBuffer.Capacity);
        }

        [TestMethod]
        public void TestDispose()
        {
            var ringBuffer = new RingBuffer(18); // 32 cap
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            ringBuffer.Write(data);
            ringBuffer.Flush();

            Assert.AreEqual(0, ringBuffer.ReadableCapacity);
            Assert.AreEqual(32, ringBuffer.WriteableCapacity);

            using (ringBuffer = new RingBuffer(18))
            {
                data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                ringBuffer.Write(data);
            }

            Assert.AreEqual(0, ringBuffer.ReadableCapacity);
            Assert.AreEqual(32, ringBuffer.WriteableCapacity);
        }
    }
}
