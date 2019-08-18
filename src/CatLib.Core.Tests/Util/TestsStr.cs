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

using CatLib.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CatLib.Tests.Util
{
    [TestClass]
    public class TestsStr
    {
        [TestMethod]
        [DataRow(@"foo\.\?\+/bar/ba.*?z", "foo.?+/bar/ba*z", "Special characters should be escaped")]
        public void TestAsteriskWildcard(string expected, string input, string message)
        {
            Assert.AreEqual(expected, Str.AsteriskWildcard(input), message);
        }

        [TestMethod]
        [DataRow(true, "foo.?+/bar/b*z", "foo.?+/bar/baz")]
        [DataRow(true, "foob*r", "fooboooooooor")]
        [DataRow(true, "foo", "foo")]
        [DataRow(false, "foo.?+/bar/b*z", "foobar.?+/bar/baz")]
        [DataRow(false, "foob*r", "fooboooooooorp")]
        public void TestIs(bool expected, string pattern, string value)
        {
            Assert.AreEqual(expected, Str.Is(pattern, value));
        }

        [TestMethod]
        public void TestSplit()
        {
            var actual = Str.Split("foobaro", 2);
            Assert.AreEqual("fo", actual[0]);
            Assert.AreEqual("ob", actual[1]);
            Assert.AreEqual("ar", actual[2]);
            Assert.AreEqual("o", actual[3]);
            Assert.AreEqual(4, actual.Length);

            actual = Str.Split("foobar", 2);
            Assert.AreEqual("fo", actual[0]);
            Assert.AreEqual("ob", actual[1]);
            Assert.AreEqual("ar", actual[2]);
            Assert.AreEqual(3, actual.Length);
        }

        [TestMethod]
        public void TestSplitEmpty()
        {
            var actual = Str.Split(string.Empty, 2);
            Assert.AreEqual(0, actual.Length);
        }

        [TestMethod]
        public void TestRepeat()
        {
            Assert.AreEqual("foofoo", Str.Repeat("foo", 2));
            Assert.AreEqual("foo", Str.Repeat("foo", 1));
            Assert.AreEqual(string.Empty, Str.Repeat("foo", 0));
        }

        [TestMethod]
        public void TestShuffle()
        {
            var i = 0;
            var foo = "foobar";
            while (Str.Shuffle(foo) == "foobar")
            {
                if (i++ > 1000)
                {
                    Assert.Fail("Must disturb the string.");
                }
            }
        }

        [TestMethod]
        public void TestShuffleEmpty()
        {
            Assert.AreEqual(string.Empty, Str.Shuffle(string.Empty));
        }

        [TestMethod]
        public void TestSubstringCount()
        {
            var count = Str.SubstringCount("foobarbaz", "ba");
            Assert.AreEqual(2, count);

            count = Str.SubstringCount("foobar", "o");
            Assert.AreEqual(2, count);

            count = Str.SubstringCount("foobarbaz", "b", 5);
            Assert.AreEqual(1, count);

            count = Str.SubstringCount("foobarfoobar", "r", 5, 5);
            Assert.AreEqual(1, count);

            count = Str.SubstringCount("foobar", "l", 0, null, StringComparison.CurrentCulture);
            Assert.AreEqual(0, count);

            count = Str.SubstringCount("abcabcab", "abcab");
            Assert.AreEqual(1, count);

            count = Str.SubstringCount(string.Empty, "foo");
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void TestSubstringStartLargeThanLength()
        {
            Assert.AreEqual(0, Str.SubstringCount("foobar", "foo", 999, 9));
        }

        [TestMethod]
        public void TestReverse()
        {
            Assert.AreEqual("oof", Str.Reverse("foo"));
        }

        [TestMethod]
        public void TestPad()
        {
            var actual = Str.Pad(10, "foo", "barbaz", Str.PadType.Both);
            Assert.AreEqual("barfoobarb", actual);

            actual = Str.Pad(10, "foo", "barbaz");
            Assert.AreEqual("foobarbazb", actual);

            actual = Str.Pad(10, "foo", "barbaz", Str.PadType.Left);
            Assert.AreEqual("barbazbfoo", actual);

            actual = Str.Pad(3, "foo", "barbaz", Str.PadType.Left);
            Assert.AreEqual("foo", actual);

            actual = Str.Pad(10, "foo", null, Str.PadType.Left);
            Assert.AreEqual("       foo", actual);

            actual = Str.Pad(10, "foo", string.Empty, Str.PadType.Left);
            Assert.AreEqual("       foo", actual);
        }

        [TestMethod]
        public void TestStrPadEmpty()
        {
            var actual = Str.Pad(10, string.Empty, "foo", Str.PadType.Left);
            Assert.AreEqual("foofoofoof", actual);

            actual = Str.Pad(10, string.Empty, "foobar", Str.PadType.Both);
            Assert.AreEqual("foobafooba", actual);

            actual = Str.Pad(10, null, "foobar");
            Assert.AreEqual("foobarfoob", actual);
        }

        [TestMethod]
        [DataRow("bar", "foobar", "foo")]
        [DataRow("bar", "foobar", "oo")]
        [DataRow("", "foobar", "bar")]
        [DataRow("foobar", "foobar", "baz")]
        public void TestAfter(string expected, string value, string search)
        {
            Assert.AreEqual(expected, Str.After(value, search));
        }

        [TestMethod]
        public void TestContains()
        {
            Assert.AreEqual(true, Str.Contains("foobar", "baz", "foo"));
            Assert.AreEqual(false, Str.Contains("foobar", "baz", "aux"));
        }

        [TestMethod]
        public void TestReplace()
        {
            var actual = Str.Replace(new[] { "+", "-", "bar" }, string.Empty, "fo+obar,f-+oobar");
            Assert.AreEqual("foo,foo", actual);

            actual = Str.Replace(new[] { "+", "-", "oo" }, string.Empty, "fo+obar,f-+oobar");
            Assert.AreEqual("fbar,fbar", actual);
        }

        [TestMethod]
        public void TestReplaceFirst()
        {
            var actual = Str.ReplaceFirst("foo", "baz", "foobar,foobar");
            Assert.AreEqual("bazbar,foobar", actual);

            actual = Str.ReplaceFirst("bar", "baz", "foo,bar");
            Assert.AreEqual("foo,baz", actual);
        }

        [TestMethod]
        public void TestReplaceLast()
        {
            var actual = Str.ReplaceLast("foo", "baz", "foobar,foobar");
            Assert.AreEqual("foobar,bazbar", actual);

            actual = Str.ReplaceLast("foobar", "baz", "foobar,foobar");
            Assert.AreEqual("foobar,baz", actual);
        }

        [TestMethod]
        public void TestRandom()
        {
            var i = 0;
            var str = Str.Random();
            while (Str.Random() == str)
            {
                if (i++ > 1000)
                {
                    Assert.Fail("Need random numbers.");
                }
            }

            Assert.AreEqual(16, str.Length, "The random length defaults to 16.");
        }

        [TestMethod]
        public void TestSpace()
        {
            Assert.AreEqual(" ", Str.Space);
        }

        [TestMethod]
        public void TestTruncate()
        {
            var actual = Str.Truncate("hello world , the sun is shine", 11);
            Assert.AreEqual("hello wo...", actual);

            actual = Str.Truncate("hello world , the sun is shine", 11, Str.Space);
            Assert.AreEqual("hello...", actual);

            actual = Str.Truncate("hello world , the sun is shine", 15, Str.Space);
            Assert.AreEqual("hello world...", actual);

            actual = Str.Truncate("hello world sun sname", 15, Str.Space);
            Assert.AreEqual("hello world...", actual);

            var regex = new Regex("orl");
            actual = Str.Truncate("hello worldrldddorl sun sname", 15, regex);
            Assert.AreEqual("hello w...", actual);

            regex = new Regex("rld");
            actual = Str.Truncate("hello worldrldddorl sun sname", 17, regex);
            Assert.AreEqual("hello world...", actual);

            actual = Str.Truncate(null, 17, regex);
            Assert.AreEqual(null, actual);

            actual = Str.Truncate("hello world", 17, regex);
            Assert.AreEqual("hello world", actual);

            regex = new Regex("rld", RegexOptions.RightToLeft);
            actual = Str.Truncate("hello worldrldddorl sun sname", 17, regex);
            Assert.AreEqual("hello world...", actual);

            actual = Str.Truncate("hel", 2);
            Assert.AreEqual("...", actual);

            actual = Str.Truncate(string.Empty, -1);
            Assert.AreEqual("...", actual);

            actual = Str.Truncate("喵h喵e越l来l越l漂o亮!了", 12, "l");
            Assert.AreEqual("喵h喵e越l来...", actual);

            actual = Str.Truncate("喵h喵e越l来l越l漂o亮!了", 12);
            Assert.AreEqual("喵h喵e越l来l越...", actual);
        }

        [TestMethod]
        public void TestMethod()
        {
            Assert.AreEqual("GetNameSpace", Str.Method("Helloworld.GetNameSpace"));
            Assert.AreEqual("GetNameSpace", Str.Method("Helloworld.GetNameSpace()"));
            Assert.AreEqual("Space", Str.Method("Helloworld.GetName@Space()"));
            Assert.AreEqual("_Space", Str.Method("Helloworld.GetName@_Space()"));
            Assert.AreEqual("g8975GetNameSpace", Str.Method("Helloworld.g8975GetNameSpace()"));
            Assert.AreEqual("GetNameSpace", Str.Method("Helloworld.8975GetNameSpace()"));
            Assert.AreEqual("ame_Space", Str.Method("Helloworld.8975GetN(;)ame_Space()"));
            Assert.AreEqual("GetName_Space", Str.Method("Helloworld.8GetName_Space()"));
            Assert.AreEqual(string.Empty, Str.Method(null));
            Assert.AreEqual(string.Empty, Str.Method(string.Empty));
        }

        [TestMethod]
        public void TestIsArray()
        {
            var actual = Arr.Filter(
                new string[]
            {
                "CatLib.Core",
                "CatLib.ILRuntime",
                "CatLib.Route",
                "Hello.World",
            }, (assembly) => Str.Is(
                new string[]
            {
                "CatLib.*",
            }, assembly));

            CollectionAssert.AreEqual(
                new[]
                {
                    "CatLib.Core",
                    "CatLib.ILRuntime",
                    "CatLib.Route",
                }, actual);
        }

        [TestMethod]
        public void TestLevenshtein()
        {
            Assert.AreEqual(4, Str.Levenshtein("hello", "world"));
            Assert.AreEqual(5, Str.Levenshtein("hello", "catlib"));
            Assert.AreEqual(10, Str.Levenshtein("hello", "catlib-world"));
        }

        [TestMethod]
        public void TestLevenshteinLargeThan255()
        {
            var builder = new StringBuilder(256);
            for (var i = 0; i < 256; i++)
            {
                builder.Append('a');
            }

            Assert.AreEqual(256, builder.Length);
            Assert.AreEqual(-1, Str.Levenshtein(builder.ToString(), "foo"));
        }

        [TestMethod]
        public void TestLevenshteinNull()
        {
            Assert.AreEqual(-1, Str.Levenshtein(null, "foo"));
        }

        [TestMethod]
        public void TestJoinList()
        {
            var actual = Str.JoinList(new[] { "foo", "bar", "baz" }, "/");

            CollectionAssert.AreEqual(
                new[]
                {
                    "foo",
                    "foo/bar",
                    "foo/bar/baz",
                }, actual);
        }

        [TestMethod]
        public void TestJoinListChar()
        {
            var actual = Str.JoinList(new[] { "foo", "bar", "baz" }, '@');

            CollectionAssert.AreEqual(
                new[]
                {
                    "foo",
                    "foo@bar",
                    "foo@bar@baz",
                }, actual);
        }
    }
}
