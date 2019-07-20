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

using CatLib.Core.Tests.Framework;
using CatLib.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CatLib.Tests.Support
{
    [TestClass]
    public class TestsSortSet
    {
        private SortSet<int, int> sortset;

        [TestInitialize]
        public void Init()
        {
            sortset = new SortSet<int, int>();
        }

        [TestMethod]
        public void TestRandValue()
        {
            var random = new Random();

            for (var i = 1000; i >= 1; i--)
            {
                sortset.Add(i, random.Next(0, 1000));
            }

            for (var i = 1; i <= 1000; i++)
            {
                Assert.IsTrue(sortset.Remove(i), $"can not remove index: {i}.");
            }

            Assert.AreEqual(0, sortset.Count);
        }

        [TestMethodIterative(100)]
        public void TestGetElementByRank()
        {
            sortset.Add(1000, 85);
            sortset.Add(999, 75);
            sortset.Add(998, 185);
            sortset.Add(997, 85);
            sortset.Add(996, 185);
            sortset.Add(995, 85);

            Assert.AreEqual(1, sortset.GetRank(995));
            Assert.AreEqual(995, sortset.GetElementByRank(1));
            Assert.AreEqual(997, sortset.GetElementByRank(2));
            Assert.AreEqual(1000, sortset.GetElementByRank(3));
            Assert.AreEqual(996, sortset.GetElementByRank(4));
            Assert.AreEqual(998, sortset.GetElementByRank(5));
        }

        [TestMethod]
        public void TestCustomComparer()
        {
            var foo = new SortSet<object, int>(new PriorityComparer());
            for (var i = 0; i < 10; i++)
            {
                foo.Add(i, i);
            }

            for (var i = 9; i >= 0; i--)
            {
                Assert.AreEqual(i, foo.Shift());
            }

            foo = new SortSet<object, int>();
            for (var i = 0; i < 10; i++)
            {
                foo.Add(i, i);
            }

            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(i, foo.Shift());
            }
        }

        [TestMethod]
        public void TestAddObject()
        {
            var foo = new SortSet<object, int>();
            var collection = new List<object>();

            for (var i = 0; i < 10; i++)
            {
                var obj = new object();
                collection.Add(obj);
                foo.Add(obj, i);
            }

            CollectionAssert.AreEqual(collection, foo.ToArray());
        }

        [TestMethod]
        public void TestGetElementRangeByRank()
        {
            for (var i = 0; i < 10; i++)
            {
                sortset.Add(i, i);
            }

            var foos = sortset.GetElementRangeByRank(3, 8);
            var n = 3;
            foreach (var foo in foos)
            {
                Assert.AreEqual(n++, foo);
            }
        }

        [TestMethod]
        public void TestGetElementRangeWithOutOfRange()
        {
            Assert.AreEqual(0, sortset.GetElementRangeByRank(3, 8).Length);
        }

        [TestMethod]
        public void TestGetElementRangeByScore()
        {
            for (var i = 0; i < 10; i++)
            {
                sortset.Add(i, i);
            }

            var foos = sortset.GetElementRangeByScore(3, 8);
            var n = 3;
            foreach (var foo in foos)
            {
                Assert.AreEqual(n++, foo);
            }
        }

        [TestMethod]
        public void TestGetElementRangeByScoreOutOfRange()
        {
            Assert.AreEqual(0, sortset.GetElementRangeByScore(3, 8).Length);
        }

        [TestMethodIterative(100)]
        public void TestRemoveRangeByScore()
        {
            for (var n = 0; n < 10; n++)
            {
                sortset.Add(n, n);
            }

            sortset.RemoveRangeByScore(3, 8);
            Assert.AreEqual(0, sortset.GetElementByRank(0));
            Assert.AreEqual(1, sortset.GetElementByRank(1));
            Assert.AreEqual(2, sortset.GetElementByRank(2));
            Assert.AreEqual(9, sortset.GetElementByRank(3));
            for (var n = 3; n < 9; n++)
            {
                sortset.Add(n, n);
            }

            sortset.Add(33, 3);
            sortset.RemoveRangeByScore(3, 3);
            Assert.AreEqual(0, sortset.GetElementByRank(0));
            Assert.AreEqual(1, sortset.GetElementByRank(1));
            Assert.AreEqual(2, sortset.GetElementByRank(2));
            Assert.AreEqual(4, sortset.GetElementByRank(3));
        }

        [TestMethodIterative(100)]
        public void TestGetElementByRevRank()
        {
            for (var i = 0; i < 10; i++)
            {
                sortset.Add(i, i);
            }

            Assert.AreEqual(6, sortset.GetElementByRevRank(3));
            Assert.AreEqual(9, sortset.GetElementByRevRank(0));
            Assert.AreEqual(0, sortset.GetElementByRevRank(9));
        }

        [TestMethod]
        public void TestReversEnumerator()
        {
            int count = 50000;
            for (int i = 0; i < count; i++)
            {
                sortset.Add(i, i);
            }

            sortset.ReverseIterator();

            int n = 0;
            foreach (var i in sortset)
            {
                Assert.AreEqual(count - (++n), i);
            }
        }

        [TestMethod]
        public void TestScoreRangeCountBound()
        {
            sortset.Add(6, 6);
            Assert.AreEqual(1, sortset.GetRangeCount(0, 100));
            Assert.AreEqual(0, sortset.GetRangeCount(7, 100));
            Assert.AreEqual(0, sortset.GetRangeCount(0, 5));
            Assert.AreEqual(1, sortset.GetRangeCount(6, 100));

            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Assert.AreEqual(0, sortset.GetRangeCount(800, 100));
            });
        }

        [TestMethod]
        public void TestGetRangeCountWithOutOfRange()
        {
            Assert.AreEqual(0, sortset.GetRangeCount(0, 100));
        }

        [TestMethod]
        public void TestScoreRangeCount()
        {
            var rand = new Random();
            var expected = new List<int>();
            int count = 50000;

            for (var i = 0; i < count; i++)
            {
                var score = rand.Next(0, 1000);
                sortset.Add(i, score);

                if (score <= 100)
                {
                    expected.Add(i);
                }
            }

            Assert.AreEqual(expected.Count, sortset.GetRangeCount(0, 100));
        }

        [TestMethod]
        public void TestAdd()
        {
            int count = 50000;
            var rand = new Random();

            for (var i = 0; i < count; i++)
            {
                var value = rand.Next();
                sortset.Add(value, value);
            }

            var max = 0;
            foreach (var value in sortset)
            {
                if (max <= value)
                {
                    max = value;
                }
                else
                {
                    Assert.Fail("Element sorting is incorrect.");
                }
            }
        }

        [TestMethod]
        public void TestRemove()
        {
            int count = 50000;
            var collection = new List<int>();
            var rand = new Random();

            for (var i = 0; i < count; i++)
            {
                var value = rand.Next();
                collection.Add(value);
                sortset.Add(value, value);
            }

            foreach (int value in collection)
            {
                sortset.Remove(value);
            }

            Assert.AreEqual(0, sortset.Count);
        }

        [TestMethod]
        public void TestGetElementByRankAndIndexer()
        {
            var count = 50000;
            for (var i = 0; i < count; i++)
            {
                sortset.Add(i, i);
            }

            var rand = new Random();
            for (var i = 0; i < Math.Min(count, 100); i++)
            {
                var expected = rand.Next(0, count);
                Assert.AreEqual(expected, sortset.GetElementByRank(expected));
                Assert.AreEqual(expected, sortset[expected]);
            }
        }

        [TestMethod]
        public void TestGetRevRank()
        {
            for (int i = 0; i < 10; i++)
            {
                sortset.Add(i, i);
            }

            Assert.AreEqual(6, sortset.GetRevRank(3));
        }

        [TestMethodIterative(100)]
        public void TestGetRank()
        {
            var count = 100;
            var collection = new List<int>();
            var rand = new Random();

            for (var i = 0; i < count; i++)
            {
                if (rand.NextDouble() < 0.1)
                {
                    collection.Add(i);
                }

                sortset.Add(i, i);
            }

            foreach (var expected in collection)
            {
                Assert.AreEqual(expected, sortset.GetRank(expected));
            }

            Assert.AreEqual(-1, sortset.GetRank(-1));
        }

        [TestMethod]
        public void TestSequentialAdd()
        {
            for (var i = 0; i < 50000; i++)
            {
                sortset.Add(i, i);
            }

            var expected = 0;
            foreach (var i in sortset)
            {
                Assert.AreEqual(expected++, i);
            }
        }

        [TestMethod]
        public void TestEmptyListForeach()
        {
            foreach (var item in sortset)
            {
                Assert.Fail("Iteration is not allowed.");
            }

            sortset.ReverseIterator();

            foreach (var item in sortset)
            {
                Assert.Fail("Iteration is not allowed.");
            }
        }

        [TestMethod]
        public void TestOverrideElement()
        {
            sortset.Add(10, 100);
            sortset.Add(10, 200);
            Assert.AreEqual(200, sortset.GetScore(10));
        }

        [TestMethod]
        public void TestContains()
        {
            sortset.Add(10, 100);

            Assert.IsTrue(sortset.Contains(10));
            Assert.IsFalse(sortset.Contains(11));
        }

        [TestMethod]
        public void TestGetScore()
        {
            sortset.Add(10, 100);
            Assert.AreEqual(100, sortset.GetScore(10));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestGetElementByRankOverflow()
        {
            sortset.Add(10, 10);
            sortset.GetElementByRank(1000);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetElementByRankEmpty()
        {
            sortset.GetElementByRank(100);
        }

        [TestMethod]
        public void TestGetRankOverflow()
        {
            sortset.Add(10, 100);
            Assert.AreEqual(-1, sortset.GetRank(100));
        }

        [TestMethod]
        public void TestGetRevRankOverflow()
        {
            sortset.Add(10, 100);

            Assert.AreEqual(-1, sortset.GetRevRank(100));
            Assert.AreEqual(0, sortset.GetRevRank(10));
        }

        [TestMethod]
        public void TestMaxLevelLimit()
        {
            sortset = new SortSet<int, int>(0.5, 3);
            for (var i = 0; i < 2048; i++)
            {
                sortset.Add(i, i);
            }
        }

        [TestMethod]
        public void TestClear()
        {
            for (var i = 0; i < 65536; i++)
            {
                sortset.Add(i, i);
            }

            sortset.Clear();
            for (var i = 0; i < 65536; i++)
            {
                sortset.Add(i, i);
            }

            for (var i = 0; i < 65536; i++)
            {
                Assert.AreEqual(i, sortset.GetRank(i));
            }

            Assert.AreEqual(65536, sortset.Count);
        }

        [TestMethod]
        public void TestFirstLast()
        {
            for (var i = 0; i < 65536; i++)
            {
                sortset.Add(i, i);
            }

            Assert.AreEqual(0, sortset.First());
            Assert.AreEqual(65535, sortset.Last());

            for (var i = 0; i < 65536; i++)
            {
                Assert.AreEqual(i, sortset.First());
                Assert.AreEqual(i, sortset.Shift());
            }

            Assert.AreEqual(0, sortset.Count);

            sortset.Clear();
            for (var i = 0; i < 65536; i++)
            {
                sortset.Add(i, i);
            }

            for (var i = 0; i < 65536; i++)
            {
                Assert.AreEqual(65535 - i, sortset.Last());
                Assert.AreEqual(65535 - i, sortset.Pop());
            }
        }

        [TestMethod]
        public void TestPop()
        {
            for (var i = 0; i < 65536; i++)
            {
                sortset.Add(i, i);
            }

            for (var i = 0; i < 65536; i++)
            {
                Assert.AreEqual(65535 - i, sortset.Pop());
            }

            Assert.AreEqual(0, sortset.Count);
        }

        [TestMethod]
        public void TestShift()
        {
            for (var i = 0; i < 65536; i++)
            {
                sortset.Add(i, i);
            }

            for (var i = 0; i < 65536; i++)
            {
                Assert.AreEqual(i, sortset.Shift());
            }

            Assert.AreEqual(0, sortset.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestFirstBound()
        {
            sortset.First();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestLastBound()
        {
            sortset.Last();
        }

        [TestMethod]
        public void TestToArray()
        {
            sortset.Add(1, 1);
            sortset.Add(9, 9);
            sortset.Add(2, 2);
            sortset.Add(8, 8);
            sortset.Add(0, 0);
            sortset.Add(4, 4);
            sortset.Add(5, 5);
            sortset.Add(6, 6);
            sortset.Add(3, 3);
            sortset.Add(7, 7);

            var expected = 0;
            foreach (var actual in sortset.ToArray())
            {
                Assert.AreEqual(expected++, actual);
            }
        }

        [TestMethod]
        public void TestSameScore()
        {
            // According to the ordered set rule, the same
            // score after the insertion will be traversed
            // to the priority
            sortset.Add(10, 10);
            sortset.Add(90, 10);
            sortset.Add(20, 10);
            sortset.Add(80, 10);

            var actual = sortset.ToArray();

            Assert.AreEqual(80, actual[0]);
            Assert.AreEqual(20, actual[1]);
            Assert.AreEqual(90, actual[2]);
            Assert.AreEqual(10, actual[3]);
        }

        [TestMethod]
        public void TestRemoveNotExistsElement()
        {
            Assert.IsFalse(sortset.Remove(8888));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestPopEmpty()
        {
            sortset.Pop();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestShiftEmpty()
        {
            sortset.Shift();
        }

        [TestMethod]
        public void TestIterationsDeleted()
        {
            for (var i = 0; i < 100; i++)
            {
                sortset.Add(i, i);
            }

            var expected = new List<int>() { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90 };
            var actuals = new List<int>();
            foreach (var i in sortset)
            {
                actuals.Add(i);
                sortset.Remove(i);
                sortset.Remove(i + 1);
                sortset.Remove(i + 2);
                sortset.Remove(i + 3);
                sortset.Remove(i + 4);
                sortset.Remove(i + 5);
                sortset.Remove(i + 6);
                sortset.Remove(i + 7);
                sortset.Remove(i + 8);
                sortset.Remove(i + 9);
            }

            var n = 0;
            foreach (var actual in actuals)
            {
                Assert.AreEqual(expected[n++], actual);
            }

            Assert.AreEqual(expected.Count, actuals.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void TestGetScoreNotFound()
        {
            sortset.GetScore(100);
        }

        private class PriorityComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return y - x;
            }
        }
    }
}
