/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib
{
    [TestClass]
    public class StorageStreamTests
    {
        [TestMethod]
        public void TestWrite()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }
            using (var readStream = new StorageStream(storage, false))
            {
                var data = new byte[16];
                Assert.AreEqual(9, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("123456789", Encoding.Default.GetString(data, 0, 9));
            }
        }

        [TestMethod]
        public void TestReadMult()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }

            StorageStream readStream1 = null;
            StorageStream readStream2 = null;

            try
            {
                readStream1 = new StorageStream(storage, false);
                readStream2 = new StorageStream(storage, false);

                var data = new byte[16];
                Assert.AreEqual(9, readStream1.Read(data, 0, data.Length));
                Assert.AreEqual("123456789", Encoding.Default.GetString(data, 0, 9));

                Assert.AreEqual(9, readStream2.Read(data, 0, data.Length));
                Assert.AreEqual("123456789", Encoding.Default.GetString(data, 0, 9));
            }
            finally
            {
                if (readStream1 != null)
                {
                    readStream1.Dispose();
                }

                if (readStream2 != null)
                {
                    readStream2.Dispose();
                }
            }
        }

        [TestMethod]
        public void TestAttr()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
                Assert.AreEqual(true, stream.CanWrite);
            }

            var readStream = new StorageStream(storage, false);
            Assert.AreEqual(9, readStream.Length);
            Assert.AreEqual(false, readStream.CanWrite);
            Assert.AreEqual(true, readStream.CanRead);
            Assert.AreEqual(true, readStream.CanSeek);
            Assert.AreEqual(false, readStream.CanTimeout);

            readStream.Dispose();
            Assert.AreEqual(false, readStream.CanWrite);
            Assert.AreEqual(false, readStream.CanRead);
            Assert.AreEqual(false, readStream.CanSeek);
            Assert.AreEqual(false, readStream.CanTimeout);
        }

        [TestMethod]
        public void TestSeek()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }

            using (var readStream = new StorageStream(storage, false))
            {
                var data = new byte[16];
                Assert.AreEqual(9, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("123456789", Encoding.Default.GetString(data, 0, 9));
                Assert.AreEqual(0, readStream.Read(data, 0, data.Length));

                readStream.Seek(1, SeekOrigin.Begin);
                Assert.AreEqual(8, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("23456789", Encoding.Default.GetString(data, 0, 8));

                readStream.Seek(-1, SeekOrigin.Current);
                Assert.AreEqual(1, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("9", Encoding.Default.GetString(data, 0, 1));

                readStream.Seek(0, SeekOrigin.Begin);
                Assert.AreEqual(9, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("123456789", Encoding.Default.GetString(data, 0, 9));

                readStream.Seek(-1, SeekOrigin.End);
                Assert.AreEqual(1, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("9", Encoding.Default.GetString(data, 0, 1));
            }
        }

        [TestMethod]
        public void TestSetPosition()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }

            using (var readStream = new StorageStream(storage, false))
            {
                var data = new byte[16];
                Assert.AreEqual(9, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("123456789", Encoding.Default.GetString(data, 0, 9));
                Assert.AreEqual(0, readStream.Read(data, 0, data.Length));

                readStream.Position = 1;
                Assert.AreEqual(8, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("23456789", Encoding.Default.GetString(data, 0, 8));
            }
        }

        [TestMethod]
        public void TestGetPosition()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }

            using (var readStream = new StorageStream(storage, false))
            {
                var data = new byte[16];
                Assert.AreEqual(9, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("123456789", Encoding.Default.GetString(data, 0, 9));
                Assert.AreEqual(0, readStream.Read(data, 0, data.Length));
                Assert.AreEqual(9, readStream.Position);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void TestSeekSmallZero()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }

            using (var readStream = new StorageStream(storage, false))
            {
                readStream.Seek(-1, SeekOrigin.Begin);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void TestSeekLargeLength()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }

            using (var readStream = new StorageStream(storage, false))
            {
                readStream.Seek(1, SeekOrigin.End);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetLength()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.SetLength(0);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestStorageDispose()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
                Assert.AreEqual(true, stream.CanWrite);
            }
            storage.Dispose();
            var a = storage.Length;
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestDisposeStorage()
        {
            var storage = new MemoryStorage();
            storage.Dispose();
            var stream = new StorageStream(storage);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestCannotWriteable()
        {
            var storage = new MemoryStorage();
            using (var stream = new StorageStream(storage, false))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }
        }

        [TestMethod]
        public void TestDoubleDispose()
        {
            var storage = new MemoryStorage();
            StorageStream stream;
            using (stream = new StorageStream(storage))
            {
                stream.Write(Encoding.Default.GetBytes("123456789"), 0, 9);
            }
            stream.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestDisposeGetPosition()
        {
            var storage = new MemoryStorage();
            var stream = new StorageStream(storage, false);
            stream.Dispose();
            var a = stream.Position;
        }

        [TestMethod]
        public void TestNotAppend()
        {
            var storage = new MemoryStorage(4, 4);

            var stream = new StorageStream(storage);
            stream.Write(Encoding.Default.GetBytes("hello world"), 0, 11);
            stream.Dispose();

            stream = new StorageStream(storage);
            stream.Write(Encoding.Default.GetBytes("hello 12345"), 0, 11);

            using (var readStream = new StorageStream(storage, false))
            {
                var data = new byte[32];
                Assert.AreEqual(11, readStream.Read(data, 0, data.Length));
                Assert.AreEqual("hello 12345", Encoding.Default.GetString(data, 0, 11));
                Assert.AreEqual(0, readStream.Read(data, 0, data.Length));
            }
        }
    }
}
