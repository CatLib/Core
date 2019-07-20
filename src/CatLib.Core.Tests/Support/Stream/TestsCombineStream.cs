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
using System.IO;
using System.Text;

#pragma warning disable CA1034

namespace CatLib.Tests.Support
{
    [TestClass]
    public class TestsCombineStream
    {
        private Encoding encoding;
        private Stream foo;
        private Stream bar;

        [TestInitialize]
        public void Initialize()
        {
            encoding = Encoding.UTF8;
            foo = "foo".ToStream();
            bar = "bar".ToStream();
        }

        [TestMethod]
        public void TestCombineStream()
        {
            var foobar = new CombineStream(foo, bar);
            Assert.AreEqual("foobar", foobar.ToText());
        }

        [TestMethod]
        public void TestRead()
        {
            var foobar = new CombineStream(foo, bar);

            var buffer = new byte[5];
            Assert.AreEqual(5, foobar.Read(buffer, 0, 5));
            Assert.AreEqual("fooba", encoding.GetString(buffer));

            Assert.AreEqual(1, foobar.Read(buffer, 0, 5));
            Assert.AreEqual("r", encoding.GetString(buffer, 0, 1));
        }

        [TestMethod]
        public void TestCombineSeek()
        {
            var foobar = new CombineStream(foo, bar);

            var buffer = new byte[3];
            Assert.AreEqual(3, foobar.Read(buffer, 0, 3));
            Assert.AreEqual("foo", encoding.GetString(buffer));
            Assert.AreEqual(3, foobar.Read(buffer, 0, 3));
            Assert.AreEqual("bar", encoding.GetString(buffer));

            foobar.Seek(3, SeekOrigin.Begin);
            Assert.AreEqual(3, foobar.Read(buffer, 0, 3));
            Assert.AreEqual("bar", encoding.GetString(buffer));

            foobar.Seek(2, SeekOrigin.Begin);
            Assert.AreEqual(3, foobar.Read(buffer, 0, 3));
            Assert.AreEqual("oba", encoding.GetString(buffer));

            foobar.Seek(4, SeekOrigin.Begin);
            Assert.AreEqual(2, foobar.Read(buffer, 0, 3));
            Assert.AreEqual("ar", encoding.GetString(buffer, 0, 2));

            foobar.Seek(6, SeekOrigin.Begin);
            Assert.AreEqual(0, foobar.Read(buffer, 0, 3));
            Assert.AreEqual(0, foobar.Read(buffer, 0, 3));

            foobar.Seek(-3, SeekOrigin.End);
            Assert.AreEqual(3, foobar.Read(buffer, 0, 3));
            Assert.AreEqual("bar", encoding.GetString(buffer));
        }

        [TestMethod]
        public void TestSetPosition()
        {
            var foobar = new CombineStream(foo, bar);
            var buffer = new byte[3];
            foobar.Position = 3;

            Assert.AreEqual(3, foobar.Read(buffer, 0, 3));
            Assert.AreEqual("bar", encoding.GetString(buffer));
        }

        [TestMethod]
        public void TestCanStatus()
        {
            var foobar = new CombineStream(foo, bar);

            Assert.IsFalse(foobar.CanWrite);
            Assert.IsTrue(foobar.CanSeek);
            Assert.IsTrue(foobar.CanRead);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestSeekOutOfRange()
        {
            var foobar = new CombineStream(foo, bar);
            foobar.Seek(999, SeekOrigin.Begin);
        }

        [TestMethod]
        public void TestCannotSeekStream()
        {
            var foobar = new CombineStream(foo, new CannotSeekStream());
            Assert.IsFalse(foobar.CanSeek);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestCannotSeekStreamSetPosition()
        {
            var foobar = new CombineStream(foo, new CannotSeekStream());
            foobar.Seek(0, SeekOrigin.Begin);
        }

        [TestMethod]
        public void TestDispose()
        {
            Assert.IsTrue(foo.CanWrite);
            Assert.IsTrue(bar.CanWrite);

            var foobar = new CombineStream(foo, bar, true);
            foobar.Dispose();

            Assert.IsFalse(foo.CanWrite);
            Assert.IsFalse(bar.CanWrite);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestWrite()
        {
            var foobar = new CombineStream(foo, bar);
            foobar.Write(new byte[5], 0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetLength()
        {
            var foobar = new CombineStream(foo, bar);
            foobar.SetLength(10);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestFlush()
        {
            var foobar = new CombineStream(foo, bar);
            foobar.Flush();
        }

        private sealed class CannotSeekStream : WrapperStream
        {
            public override bool CanSeek => false;
        }
    }
}
