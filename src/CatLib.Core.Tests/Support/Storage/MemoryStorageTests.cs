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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib
{
    [TestClass]
    public class MemoryStorageTests
    {
        [TestMethod]
        public void TestWrite()
        {
            // Append() is alias as Write
            var storage = new MemoryStorage(4);
            storage.Append(Encoding.Default.GetBytes("hello world"), 0, 11);
            var data = new byte[16];
            Assert.AreEqual(11, storage.Read(data, 0, data.Length));
            Assert.AreEqual("hello world", Encoding.Default.GetString(Arr.Slice(data, 0, 11)));
        }

        [TestMethod]
        public void TestAppend()
        {
            var storage = new MemoryStorage(4);
            storage.Append(Encoding.Default.GetBytes("hello world"), 0, 11);
            storage.Append(Encoding.Default.GetBytes("1"), 0, 1);
            var data = new byte[16];
            Assert.AreEqual(12, storage.Read(data, 0, data.Length));
            Assert.AreEqual("hello world1", Encoding.Default.GetString(Arr.Slice(data, 0, 12)));

            storage.Append(Encoding.Default.GetBytes("2"), 0, 1);
            Assert.AreEqual(13, storage.Read(data, 0, data.Length));
            Assert.AreEqual("hello world12", Encoding.Default.GetString(Arr.Slice(data, 0, 13)));
        }

        [TestMethod]
        public void TestBlockAutomaticExpansion()
        {
            var storage = new MemoryStorage(4, 2);
            storage.Append(Encoding.Default.GetBytes("12345678"), 0, 8);
            storage.Append(Encoding.Default.GetBytes("9"), 0, 1);

            var data = new byte[16];
            Assert.AreEqual(9, storage.Read(data, 0, data.Length));
            Assert.AreEqual("123456789", Encoding.Default.GetString(Arr.Slice(data, 0, 9)));
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestDispose()
        {
            MemoryStorage storage;
            using (storage = new MemoryStorage(4, 2))
            {
                storage.Append(Encoding.Default.GetBytes("12345678"), 0, 8);
            }

            var data = new byte[16];
            storage.Read(data, 0, data.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestDispose2()
        {
            MemoryStorage storage;
            using (storage = new MemoryStorage(4, 2))
            {
                
            }
            storage.Append(Encoding.Default.GetBytes("12345678"), 0, 8);
        }

        [TestMethod]
        public void TestJumpWrite()
        {
            var storage = new MemoryStorage(4, 2);
            storage.Append(Encoding.Default.GetBytes("1234"), 0, 4);
            storage.Write(Encoding.Default.GetBytes("5678"), 0, 4, 13);
            Assert.AreEqual(17, storage.Length);

            var data = new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
                17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32
            };
            Assert.AreEqual(17, storage.Read(data, 0, data.Length));
            Assert.AreEqual("1234\0\0\0\0\0\0\0\0\05678", Encoding.Default.GetString(Arr.Slice(data, 0, 17)));
        }

        [TestMethod]
        [ExpectedException(typeof(OutOfMemoryException))]
        public void TestOutOfMemory()
        {
            var storage = new MemoryStorage(8, 4, 2);
            try
            {
                storage.Append(Encoding.Default.GetBytes("12345678"), 0, 8);
            }
            catch (OutOfMemoryException)
            {
                
            }

            storage.Append(Encoding.Default.GetBytes("1"), 0, 1);
        }

        [TestMethod]
        public void TestDoubleDispose()
        {
            var storage = new MemoryStorage(8, 4, 2);
            Assert.AreEqual(false, storage.Disabled);
            storage.Dispose();
            Assert.AreEqual(true, storage.Disabled);
            storage.Dispose();
            Assert.AreEqual(true, storage.Disabled);
        }

        [TestMethod]
        public void TestGetLocker()
        {
            using (var storage = new MemoryStorage(8, 4, 2))
            {
                Assert.AreNotEqual(null, storage.Locker);
            }
        }
    }
}
