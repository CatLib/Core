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
    }
}
