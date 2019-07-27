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
using System.Collections.Generic;

#pragma warning disable CA1031

namespace CatLib.Tests.Support
{
    [TestClass]
    public sealed class TestsArr
    {
        private string[] foo;
        private string[] bar;
        private string[] foobar;
        private string[] foobarbar;
        private string[] aux;

        [TestInitialize]
        public void Initialize()
        {
            foo = new[] { "foo" };
            bar = new[] { "bar", "baz" };
            foobar = new[] { "foo", "bar", "baz" };
            foobarbar = new[] { "foo", "bar", "baz", "bar", "baz" };
            aux = null;
        }

        [TestMethod]
        public void TestMerge()
        {
            var actual = Arr.Merge(foo, bar);

            CollectionAssert.AreEqual(foobar, actual);
        }

        [TestMethod]
        public void TestMergeAllEmpty()
        {
            var actual = Arr.Merge(Array.Empty<int>(), Array.Empty<int>());
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void TestMergeNull()
        {
            var actual = Arr.Merge(foo, aux, bar);

            CollectionAssert.AreEqual(foobar, actual);
        }

        [TestMethod]
        public void TestMergeEmpty()
        {
            var actual = Arr.Merge(foo, Array.Empty<string>(), bar);

            CollectionAssert.AreEqual(foobar, actual);
        }

        [TestMethod]
        public void TestRandom()
        {
            var source = new[] { "foo", "bar", "baz", "aux" };
            var actual = Arr.Rand(source);

            var i = 0;
            while (Arr.Rand(source)[0] == actual[0])
            {
                if (i++ > 1000)
                {
                    Assert.Fail("Random look's not work.");
                }
            }

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz", "aux",
            }, source);
        }

        [TestMethod]
        public void TestSplice()
        {
            var removed = Arr.Splice(ref foobar, 1, 1, bar);

            Assert.AreEqual("bar", removed[0]);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestBaseNegativeSplice()
        {
            var removed = Arr.Splice(ref foobar, -1, null, bar);

            Assert.AreEqual("baz", removed[0]);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "bar", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestSimpleArgsSplice()
        {
            var removed = Arr.Splice(ref foobar, 1);

            CollectionAssert.AreEqual(
                new[]
            {
                "bar", "baz",
            }, removed);
            CollectionAssert.AreEqual(new[] { "foo" }, foobar);
        }

        [TestMethod]
        public void TestSimpleNegativeStart()
        {
            var removed = Arr.Splice(ref foobar, -1);

            CollectionAssert.AreEqual(new[] { "baz" }, removed);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar",
            }, foobar);
        }

        [TestMethod]
        public void TestZeroStart()
        {
            var removed = Arr.Splice(ref foobar, 0);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz",
            }, removed);

            CollectionAssert.AreEqual(Array.Empty<string>(), foobar);
        }

        [TestMethod]
        public void TestOverflowNegativeStart()
        {
            var removed = Arr.Splice(ref foobar, -999);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz",
            }, removed);

            CollectionAssert.AreEqual(Array.Empty<string>(), foobar);
        }

        [TestMethod]
        public void TestOverflowStart()
        {
            var removed = Arr.Splice(ref foobar, 999);

            CollectionAssert.AreEqual(Array.Empty<string>(), removed);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestOverflowStartRepl()
        {
            var removed = Arr.Splice(ref foobar, 999, -999, bar);

            CollectionAssert.AreEqual(Array.Empty<string>(), removed);
            CollectionAssert.AreEqual(foobarbar, foobar);
        }

        [TestMethod]
        public void TestChunk()
        {
            var actual = Arr.Chunk(foobarbar, 2);

            CollectionAssert.AreEqual(new[] { "foo", "bar" }, actual[0]);
            CollectionAssert.AreEqual(new[] { "baz", "bar" }, actual[1]);
            CollectionAssert.AreEqual(new[] { "baz" }, actual[2]);
        }

        [TestMethod]
        public void TestChunkInsufficientQuantity()
        {
            var source = new[] { "foo" };
            var actual = Arr.Chunk(source, 2);

            CollectionAssert.AreEqual(new[] { "foo" }, actual[0]);
        }

        [TestMethod]
        public void TestChunkBound()
        {
            var actual = Arr.Chunk(foobar, 2);
            CollectionAssert.AreEqual(new[] { "foo", "bar" }, actual[0]);
            CollectionAssert.AreEqual(new[] { "baz" }, actual[1]);
        }

        [TestMethod]
        public void TestFill()
        {
            var actual = Arr.Fill(1, 5, "foo");

            CollectionAssert.AreEqual(
                new[]
            {
                null, "foo", "foo", "foo", "foo", "foo",
            }, actual);
        }

        [TestMethod]
        public void TestFillZeroStart()
        {
            var actual = Arr.Fill(0, 5, "foo");
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "foo", "foo", "foo", "foo",
            }, actual);
        }

        [TestMethod]
        public void TestFillWithSource()
        {
            var actual = Arr.Fill(2, 3, "foo", foobar);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "foo", "foo", "foo", "baz",
            }, actual);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestFillBoundWithSource()
        {
            var actual = Arr.Fill(3, 3, "foo", foobar);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz", "foo", "foo", "foo",
            }, actual);
        }

        [TestMethod]
        public void TestFillOutOfRangeWithSource()
        {
            var actual = Arr.Fill(4, 2, "foo", foobar);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz", null, "foo", "foo",
            }, actual);
        }

        [TestMethod]
        public void TestFillZeroWithSource()
        {
            var actual = Arr.Fill(0, 3, "foo", foobar);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "foo", "foo", "foo", "bar", "baz",
            }, actual);
        }

        [TestMethod]
        public void TestFillThrowException()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                Arr.Fill(-1, 3, "foo");
            });

            Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                Arr.Fill(0, 0, "foo");
            });
        }

        [TestMethod]
        public void TestFilter()
        {
            var actual = Arr.Filter(foobar, (f) => f == "foo");

            CollectionAssert.AreEqual(foo, actual);
        }

        [TestMethod]
        public void TestFilterExpected()
        {
            var actual = Arr.Filter(foobar, (f) => f == "foo", false);

            CollectionAssert.AreEqual(bar, actual);
        }

        [TestMethod]
        public void TestFilterIEnumerable()
        {
            var actual = Arr.Filter(
                new List<string>(foobar),
                (f) => f == "foo");

            CollectionAssert.AreEqual(foo, actual);
        }

        [TestMethod]
        public void TestMap()
        {
            var actual = Arr.Map(foobar, (o) => o + o);

            CollectionAssert.AreEqual(
                new[]
            {
                "foofoo", "barbar", "bazbaz",
            }, actual);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestMapIEnumerable()
        {
            var data = new List<string>(foobar);
            var actual = Arr.Map(data, (o) => o + o);

            CollectionAssert.AreEqual(
                new[]
            {
                "foofoo", "barbar", "bazbaz",
            }, actual);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz",
            }, data);
        }

        [TestMethod]
        public void TestPop()
        {
            var actual = Arr.Pop(ref foobar);

            Assert.AreEqual("baz", actual);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar",
            }, foobar);
        }

        [TestMethod]
        public void TestPush()
        {
            var actual = Arr.Push(ref foobar, "aux", "foobar");
            Assert.AreEqual(5, actual);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz", "aux", "foobar",
            }, foobar);
        }

        [TestMethod]
        public void TestReduce()
        {
            var actual = Arr.Reduce(foobar, (left, right) => $"{left}-{right}", "heros");

            Assert.AreEqual("heros-foo-bar-baz", actual);
        }

        [TestMethod]
        public void TestSlice()
        {
            var actual = Arr.Slice(foobar, 1, -1);

            CollectionAssert.AreEqual(
                new[]
            {
                "bar",
            }, actual);
        }

        [TestMethod]
        public void TestShift()
        {
            var actual = Arr.Shift(ref foobar);

            Assert.AreEqual("foo", actual);
            CollectionAssert.AreEqual(
                new[]
            {
                "bar", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestUnShift()
        {
            var actual = Arr.Unshift(ref foobar, "aux", "foobar");

            Assert.AreEqual(5, actual);
            CollectionAssert.AreEqual(
                new[]
            {
                "aux", "foobar", "foo", "bar", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestReverse()
        {
            var actual = Arr.Reverse(foobar);

            CollectionAssert.AreEqual(
                new[]
            {
                "baz", "bar", "foo",
            }, actual);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestReverseWithStartLength()
        {
            var actual = Arr.Reverse(foobarbar, 1, 2);

            CollectionAssert.AreEqual(
                new[]
            {
                "baz", "bar",
            }, actual);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz", "bar", "baz",
            }, foobarbar);
        }

        [TestMethod]
        public void TestIndexOf()
        {
            var actual = Arr.IndexOf(foobar, new[] { "bar", "baz" });

            Assert.AreEqual(1, actual);
        }

        [TestMethod]
        public void TestIndexOfNull()
        {
            Assert.AreEqual(-1, Arr.IndexOf(null, new[] { "foo" }));
        }

        [TestMethod]
        public void TestIndexNotFind()
        {
            var actual = Arr.IndexOf(foobar, new[] { "baz", "bar" });

            Assert.AreEqual(-1, actual);
        }

        [TestMethod]
        public void TestIndexOfAny()
        {
            var actual = Arr.IndexOfAny(foobar, new[] { "baz", "bar" });

            Assert.AreEqual(1, actual);
        }

        [TestMethod]
        public void TestIndexAnyNotFind()
        {
            var actual = Arr.IndexOfAny(foobar, new[] { "auz" });

            Assert.AreEqual(-1, actual);
        }

        [TestMethod]
        public void TestIndexOfAnyNull()
        {
            var actual = Arr.IndexOfAny(foobar, null);
            Assert.AreEqual(-1, actual);

            actual = Arr.IndexOfAny<string>(null, null);
            Assert.AreEqual(-1, actual);
        }

        [TestMethod]
        public void TestDifference()
        {
            var actual = Arr.Difference(foobar, new[] { "bar", "baz" });

            CollectionAssert.AreEqual(
                new[]
            {
                "foo",
            }, actual);
        }

        [TestMethod]
        public void TestDifferenceEmptyMatch()
        {
            var actual = Arr.Difference(foobar, null);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz",
            }, actual);
        }

        [TestMethod]
        public void TestRemoveAt()
        {
            var actual = Arr.RemoveAt(ref foobar, 1);

            Assert.AreEqual("bar", actual);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestRemoveAtWithDefault()
        {
            Assert.AreEqual("foo", Arr.RemoveAt(ref foobar, 999, "foo"));
        }

        [TestMethod]
        public void TestRemoveAtNegativeNumber()
        {
            var actual = Arr.RemoveAt(ref foobar, -2);

            Assert.AreEqual("bar", actual);
            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "baz",
            }, foobar);

            actual = Arr.RemoveAt(ref foobar, -999);

            Assert.AreEqual("foo", actual);
            CollectionAssert.AreEqual(
                new[]
            {
                "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestCut()
        {
            Arr.Cut(ref foobar, 1);
            CollectionAssert.AreEqual(
                new[]
            {
                "bar", "baz",
            }, foobar);

            Arr.Cut(ref foobar, -1);
            CollectionAssert.AreEqual(
                new[]
            {
                "bar",
            }, foobar);

            Arr.Cut(ref foobar, 0);
            CollectionAssert.AreEqual(
                new[]
            {
                "bar",
            }, foobar);

            Arr.Cut(ref foobar, 999);
            CollectionAssert.AreEqual(Array.Empty<string>(), foobar);

            Arr.Cut(ref bar, -999);
            CollectionAssert.AreEqual(Array.Empty<string>(), bar);
        }

        [TestMethod]
        public void TestRemove()
        {
            var actual = Arr.Remove(ref foobar, (o) => o == "bar");

            CollectionAssert.AreEqual(
                new[]
            {
                "bar",
            }, actual);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "baz",
            }, foobar);

            actual = Arr.Remove(ref foobarbar, (o) => o == "bar");

            CollectionAssert.AreEqual(
                new[]
            {
                "bar", "bar",
            }, actual);

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "baz", "baz",
            }, foobarbar);
        }

        [TestMethod]
        public void TestTest()
        {
            Assert.AreEqual(true, Arr.Test(foobar, (o) => o == "foo"));
        }

        [TestMethod]
        public void TestSetReplace()
        {
            Arr.Set(ref foobar, (o) => o == "bar", "aux");

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "aux", "baz",
            }, foobar);
        }

        [TestMethod]
        public void TestSetPush()
        {
            Arr.Set(ref foobar, (o) => o == "not-found", "aux");

            CollectionAssert.AreEqual(
                new[]
            {
                "foo", "bar", "baz", "aux",
            }, foobar);
        }
    }
}
