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

namespace CatLib.Tests.Support
{
    [TestClass]
    public class TestsSegmentStream
    {
        private Encoding encoding;
        private Stream foobarbaz;

        [TestInitialize]
        public void Initialize()
        {
            encoding = Encoding.UTF8;
            foobarbaz = "foo bar baz".ToStream();
        }

        [TestMethod]
        public void TestRead()
        {
            var segment = new SegmentStream(foobarbaz, 3);
            Assert.AreEqual("foo", segment.ToText());
        }

        [TestMethod]
        public void TestReadMiddle()
        {
            foobarbaz.Seek(4, SeekOrigin.Begin);
            var segment = new SegmentStream(foobarbaz, 3);
            Assert.AreEqual("bar", segment.ToText());
        }

        [TestMethod]
        public void TestReadEnd()
        {
            foobarbaz.Seek(4, SeekOrigin.Begin);
            var segment = new SegmentStream(foobarbaz);
            Assert.AreEqual("bar baz", segment.ToText());
        }

        [TestMethod]
        public void TestSeekEnd()
        {
            var segment = new SegmentStream(foobarbaz, 7);
            segment.Seek(-3, SeekOrigin.End);
            Assert.AreEqual("bar", segment.ToText());
        }

        [TestMethod]
        public void TestSeekCurrent()
        {
            foobarbaz.Seek(2, SeekOrigin.Begin);
            var segment = new SegmentStream(foobarbaz, 5);
            segment.Seek(2, SeekOrigin.Current);
            Assert.AreEqual("bar", segment.ToText());
        }

        [TestMethod]
        public void TestSeekBegin()
        {
            var segment = new SegmentStream(foobarbaz, 7);
            segment.ToText(null, false);

            segment.Seek(0, SeekOrigin.Begin);
            Assert.AreEqual("foo bar", segment.ToText());
        }

        [TestMethod]
        public void TestGetLength()
        {
            var segmentStream = new SegmentStream(foobarbaz, 7);
            Assert.AreEqual(7, segmentStream.Length);
        }

        [TestMethod]
        public void TestGetPosition()
        {
            foobarbaz.Seek(2, SeekOrigin.Begin);
            var segment = new SegmentStream(foobarbaz, 5)
            {
                Position = 4
            };

            Assert.AreEqual(4, segment.Position);
        }

        [TestMethod]
        public void TestReadBuffer()
        {
            var segment = new SegmentStream(foobarbaz, 7);
            var actual = new byte[255];
            Assert.AreEqual(7, segment.Read(actual, 0, 255));
            Assert.AreEqual("foo bar", encoding.GetString(actual, 0, 7));
        }

        [TestMethod]
        public void TestReadBufferEnd()
        {
            var segment = new SegmentStream(foobarbaz, 7);
            var actual = new byte[255];
            Assert.AreEqual(7, segment.Read(actual, 0, 255));
            Assert.AreEqual("foo bar", encoding.GetString(actual, 0, 7));
            Assert.AreEqual(0, segment.Read(actual, 0, 255));
        }

        [TestMethod]
        public void TestReadBufferMin()
        {
            var segment = new SegmentStream(foobarbaz, 7);
            var buffer = new byte[3];
            Assert.AreEqual(3, segment.Read(buffer, 0, 3));
            Assert.AreEqual("foo", encoding.GetString(buffer));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSetLength()
        {
            var segment = new SegmentStream(foobarbaz);
            segment.SetLength(100);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestWrite()
        {
            var segment = new SegmentStream(foobarbaz);
            var data = encoding.GetBytes("foo");
            segment.Write(data, 0, data.Length);
        }

        [TestMethod]
        public void TestSeekEndToRead()
        {
            var segment = new SegmentStream(foobarbaz);
            segment.Seek(999, SeekOrigin.End);
            Assert.AreEqual(string.Empty, segment.ToText());
        }

        [TestMethod]
        public void TestSeekSmallThenStart()
        {
            var segment = new SegmentStream(foobarbaz);
            segment.Seek(-999, SeekOrigin.Begin);
            Assert.AreEqual("foo bar baz", segment.ToText());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGivenCanNotSeek()
        {
            new SegmentStream(new CanNotSeekStream());
        }

        private class CanNotSeekStream : WrapperStream
        {
            public override bool CanSeek => false;
        }
    }
}
