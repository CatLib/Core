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

using CatLib.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CatLib.Tests.Support
{
    [TestClass]
    public class TestsRingBuffer
    {
        [TestMethod]
        public void TestReadWrite()
        {
            var ringBuffer = new RingBufferStream();
            ringBuffer.Write(new byte[] { 1, 2, 4, 5 }, 1, 2);

            var read = Read(ringBuffer);
            CollectionAssert.AreEqual(new byte[]
            {
                2, 4
            }, read);

            ringBuffer = new RingBufferStream();
            ringBuffer.Write(new byte[] { 1, 2, 3, 4, 5 }, 2, 3);

            var actual = new byte[5];
            var count = ringBuffer.Read(actual, 2, actual.Length - 2);

            Assert.AreEqual(3, count);
            CollectionAssert.AreEqual(new byte[]
            {
                0, 0, 3, 4, 5
            }, actual);
        }

        [TestMethod]
        public void TestCanRead()
        {
            var ringBuffer = new RingBufferStream();
            var data = new byte[] { 1, 2, 3, 4, 5 };
            ringBuffer.Write(data, 0, data.Length);

            Assert.IsTrue(ringBuffer.ReadableCount >= 4);
            Assert.IsTrue(ringBuffer.ReadableCount >= 5);
            Assert.IsFalse(ringBuffer.ReadableCount >= 6);

            ringBuffer.Write(data, 0, data.Length);
            Assert.IsTrue(ringBuffer.ReadableCount >= 9);
            Assert.IsTrue(ringBuffer.ReadableCount >= 10);
            Assert.IsFalse(ringBuffer.ReadableCount >= 11);
        }

        [TestMethod]
        public void TestCanWrite()
        {
            // 16 cap
            var ringBuffer = new RingBufferStream(12); 
            Assert.IsTrue(ringBuffer.WriteableCount >= 15);
            Assert.IsTrue(ringBuffer.WriteableCount >= 16);
            Assert.IsFalse(ringBuffer.WriteableCount >= 17);

            ringBuffer.Write(new byte[] { 1, 2, 3, 4, 5 }, 0, 5);
            Assert.IsTrue(ringBuffer.WriteableCount >= 10);
            Assert.IsTrue(ringBuffer.WriteableCount >= 11);
            Assert.IsFalse(ringBuffer.WriteableCount >= 12);

            ringBuffer.Write(new byte[] { 1, 2, 3, 4, 5 }, 3, 2);
            Assert.IsTrue(ringBuffer.WriteableCount >= 8);
            Assert.IsTrue(ringBuffer.WriteableCount >= 9);
            Assert.IsFalse(ringBuffer.WriteableCount >= 10);
        }

        [TestMethod]
        public void TestPeek()
        {
            // 16 cap
            var ringBuffer = new RingBufferStream(12); 
            var buffer = new byte[] { 1, 2, 3, 4, 5 };
            ringBuffer.Write(buffer, 0, buffer.Length);

            Assert.AreEqual(11, ringBuffer.WriteableCount);
            Assert.AreEqual(5, ringBuffer.ReadableCount);
            CollectionAssert.AreEqual(new byte[]
            {
                1, 2, 3, 4, 5
            }, Peek(ringBuffer));

            var actual = new byte[5];
            var read = ringBuffer.Peek(actual, 3, actual.Length - 3);

            Assert.AreEqual(2, read);
            CollectionAssert.AreEqual(new byte[]
            {
                0, 0, 0, 1, 2
            }, actual);

            read = ringBuffer.Peek(actual, 0, actual.Length);
            Assert.AreEqual(5, read);
            CollectionAssert.AreEqual(new byte[]
            {
                1, 2, 3, 4, 5
            }, actual);

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
            var ringBuffer = new RingBufferStream(4);
            ringBuffer.Write(new byte[] { 1, 2, 3, 4, 5 }, 0, 5);
        }

        [TestMethod]
        public void TestReadEmptyBuffer()
        {
            var ringBuffer = new RingBufferStream(4);

            Assert.AreEqual(0, Read(ringBuffer).Length);
            Assert.AreEqual(0, Peek(ringBuffer).Length);

            var count = ringBuffer.Read(new byte[5], 2, 3);
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void TestOutOffset()
        {
            var ringBuffer = new RingBufferStream(4);
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
            // 16 cap
            var ringBuffer = new RingBufferStream(12);
            Assert.AreEqual(16, ringBuffer.GetBuffer().Length);

            ringBuffer = new RingBufferStream(12, false);
            ExceptionAssert.Throws<UnauthorizedAccessException>(() =>
            {
                ringBuffer.GetBuffer();
            });
        }

        [TestMethod]
        public void TestBufferReuse()
        {
            // 16 cap
            var ringBuffer = new RingBufferStream(12);
            var expected = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            ringBuffer.Write(expected, 0, expected.Length);
            Read(ringBuffer);

            ringBuffer.Write(expected, 0, expected.Length);
            var read = Read(ringBuffer);

            CollectionAssert.AreEqual(expected, read);
        }

        [TestMethod]
        public void TestCapacity()
        {
            // 32 cap
            var ringBuffer = new RingBufferStream(18); 
            Assert.AreEqual(32, ringBuffer.Capacity);
        }

        [TestMethod]
        public void TestClear()
        {
            // 32 cap
            var ringBuffer = new RingBufferStream(18); 
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            ringBuffer.Write(data, 0, data.Length);
            ringBuffer.Clear();

            Assert.AreEqual(0, ringBuffer.ReadableCount);
            Assert.AreEqual(32, ringBuffer.WriteableCount);

            using (ringBuffer = new RingBufferStream(18))
            {
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
