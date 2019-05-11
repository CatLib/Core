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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Support.Util
{
    [TestClass]
    public sealed class ArrTests
    {
        [TestMethod]
        public void TestMerge()
        {
            var arr1 = new[] { "1", "2" };
            var arr2 = new[] { "3" };

            var newArr = Arr.Merge(arr1, arr2);

            var i = 0;
            foreach (var result in newArr)
            {
                Assert.AreEqual((++i).ToString(), result);
            }
            Assert.AreEqual(3, newArr.Length);
        }

        [TestMethod]
        public void TestMergeAllEmpty()
        {
            var newArr = Arr.Merge(new int[0], new int[0]);
            Assert.AreEqual(0, newArr.Length);
        }

        [TestMethod]
        public void TestMergeNull()
        {
            var arr1 = new[] { "1", "2" };
            string[] arr2 = null;
            var arr3 = new[] { "3" };
            var newArr = Arr.Merge(arr1, arr2, arr3);
            Assert.AreEqual(3, newArr.Length);
            var i = 0;
            foreach (var result in newArr)
            {
                Assert.AreEqual((++i).ToString(), result);
            }
        }

        [TestMethod]
        public void TestMergeEmpty()
        {
            var arr1 = new[] { "1", "2" };
            var arr2 = new string[0];
            var arr3 = new[] { "3" };
            var newArr = Arr.Merge(arr1, arr2, arr3);
            Assert.AreEqual(3, newArr.Length);
            var i = 0;
            foreach (var result in newArr)
            {
                Assert.AreEqual((++i).ToString(), result);
            }
        }

        [TestMethod]
        public void TestRandom()
        {
            var arr = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
            var results = Arr.Rand(arr);

            var i = 0;
            while (Arr.Rand(arr)[0] == results[0])
            {
                if (i++ > 1000)
                {
                    Assert.Fail();
                }
            }

            Assert.AreEqual("1" , arr[0]);
            Assert.AreEqual("2", arr[1]);
            Assert.AreEqual("3", arr[2]);
            Assert.AreEqual("4", arr[3]);
            Assert.AreEqual("5", arr[4]);
            Assert.AreEqual("6", arr[5]);
            Assert.AreEqual("7", arr[6]);
            Assert.AreEqual("8", arr[7]);
            Assert.AreEqual("9", arr[8]);
            Assert.AreEqual("0", arr[9]);
        }

        [TestMethod]
        public void TestBaseSplice()
        {
            var arr1 = new[] { "red", "orange", "white" };
            var arr2 = new[] { "dog", "cat" };

            var remove = Arr.Splice(ref arr1, 1, 1, arr2);
            Assert.AreEqual("orange", remove[0]);

            Assert.AreEqual("red", arr1[0]);
            Assert.AreEqual("dog", arr1[1]);
            Assert.AreEqual("cat", arr1[2]);
            Assert.AreEqual("white", arr1[3]);
        }

        [TestMethod]
        public void TestBaseNegativeSplice()
        {
            var arr1 = new[] { "red", "orange", "white" };
            var arr2 = new[] { "dog", "cat" };

            var remove = Arr.Splice(ref arr1, -1, null, arr2);
            Assert.AreEqual("white", remove[0]);

            Assert.AreEqual("red", arr1[0]);
            Assert.AreEqual("orange", arr1[1]);
            Assert.AreEqual("dog", arr1[2]);
            Assert.AreEqual("cat", arr1[3]);
        }

        [TestMethod]
        public void TestSimpleArgsSplice()
        {
            var arr1 = new[] { "red", "orange", "white" };
            var remove = Arr.Splice(ref arr1, 1);
            Assert.AreEqual("orange", remove[0]);
            Assert.AreEqual("white", remove[1]);
            Assert.AreEqual(2, remove.Length);

            Assert.AreEqual("red", arr1[0]);
            Assert.AreEqual(1, arr1.Length);
        }

        [TestMethod]
        public void TestSimpleNegativeStart()
        {
            var arr1 = new[] { "red", "orange", "white" };
            var remove = Arr.Splice(ref arr1, -1);
            Assert.AreEqual("white", remove[0]);
            Assert.AreEqual(1, remove.Length);

            Assert.AreEqual("red", arr1[0]);
            Assert.AreEqual("orange", arr1[1]);
            Assert.AreEqual(2, arr1.Length);
        }

        [TestMethod]
        public void TestZeroStart()
        {
            var arr1 = new[] { "red", "orange", "white" };
            var remove = Arr.Splice(ref arr1, 0);
            Assert.AreEqual("red", remove[0]);
            Assert.AreEqual("orange", remove[1]);
            Assert.AreEqual("white", remove[2]);
            Assert.AreEqual(3, remove.Length);
            Assert.AreEqual(0, arr1.Length);
        }

        [TestMethod]
        public void TestOverflowNegativeStart()
        {
            var arr1 = new[] { "red", "orange", "white" };
            var remove = Arr.Splice(ref arr1, -999);
            Assert.AreEqual("red", remove[0]);
            Assert.AreEqual("orange", remove[1]);
            Assert.AreEqual("white", remove[2]);
            Assert.AreEqual(3, remove.Length);
            Assert.AreEqual(0, arr1.Length);
        }

        [TestMethod]
        public void TestOverflowStart()
        {
            var arr1 = new[] { "red", "orange", "white" };
            var remove = Arr.Splice(ref arr1, 999);
            Assert.AreEqual(0, remove.Length);
            Assert.AreEqual("red", arr1[0]);
            Assert.AreEqual("orange", arr1[1]);
            Assert.AreEqual("white", arr1[2]);
            Assert.AreEqual(3, arr1.Length);
        }

        [TestMethod]
        public void TestOverflowStartRepl()
        {
            var arr1 = new[] { "red", "orange", "white" };
            var arr2 = new[] { "dog", "cat" };
            var remove = Arr.Splice(ref arr1, 999, -999, arr2);

            Assert.AreEqual(0, remove.Length);
            Assert.AreEqual("red", arr1[0]);
            Assert.AreEqual("orange", arr1[1]);
            Assert.AreEqual("white", arr1[2]);
            Assert.AreEqual("dog", arr1[3]);
            Assert.AreEqual("cat", arr1[4]);
            Assert.AreEqual(5, arr1.Length);
        }

        [TestMethod]
        public void TestChunk()
        {
            var arr1 = new[] { "red", "orange", "white", "dog", "cat" };

            var result = Arr.Chunk(arr1, 2);

            Assert.AreEqual("red", result[0][0]);
            Assert.AreEqual("orange", result[0][1]);
            Assert.AreEqual(2, result[0].Length);
            Assert.AreEqual("white", result[1][0]);
            Assert.AreEqual("dog", result[1][1]);
            Assert.AreEqual(2, result[1].Length);
            Assert.AreEqual("cat", result[2][0]);
            Assert.AreEqual(1, result[2].Length);
        }

        [TestMethod]
        public void TestChunk2()
        {
            var arr1 = new[] { "red", "orange", "white", "dog", "cat", "flower" };

            var result = Arr.Chunk(arr1, 2);

            Assert.AreEqual("red", result[0][0]);
            Assert.AreEqual("orange", result[0][1]);
            Assert.AreEqual(2, result[0].Length);
            Assert.AreEqual("white", result[1][0]);
            Assert.AreEqual("dog", result[1][1]);
            Assert.AreEqual(2, result[1].Length);
            Assert.AreEqual("cat", result[2][0]);
            Assert.AreEqual("flower", result[2][1]);
            Assert.AreEqual(2, result[2].Length);
        }

        [TestMethod]
        public void TestChunk3()
        {
            var arr1 = new[] { "red" };

            var result = Arr.Chunk(arr1, 2);

            Assert.AreEqual("red", result[0][0]);
            Assert.AreEqual(1, result[0].Length);
        }

        [TestMethod]
        public void TestChunk4()
        {
            var arr1 = new[] { "red", "white" };

            var result = Arr.Chunk(arr1, 2);

            Assert.AreEqual("red", result[0][0]);
            Assert.AreEqual("white", result[0][1]);
            Assert.AreEqual(2, result[0].Length);
        }

        [TestMethod]
        public void TestChunk5()
        {
            var arr1 = new[] { "red", "white", "dog" };

            var result = Arr.Chunk(arr1, 2);

            Assert.AreEqual("red", result[0][0]);
            Assert.AreEqual("white", result[0][1]);
            Assert.AreEqual(2, result[0].Length);
            Assert.AreEqual("dog", result[1][0]);
            Assert.AreEqual(1, result[1].Length);
        }

        [TestMethod]
        public void TestFill()
        {
            var result = Arr.Fill(1, 5, "aaa");
            Assert.AreEqual(null, result[0]);
            Assert.AreEqual("aaa", result[1]);
            Assert.AreEqual("aaa", result[2]);
            Assert.AreEqual("aaa", result[3]);
            Assert.AreEqual("aaa", result[4]);
            Assert.AreEqual("aaa", result[5]);
        }

        [TestMethod]
        public void TestFillZeroStart()
        {
            var result = Arr.Fill(0, 5, "aaa");
            Assert.AreEqual("aaa", result[0]);
            Assert.AreEqual("aaa", result[1]);
            Assert.AreEqual("aaa", result[2]);
            Assert.AreEqual("aaa", result[3]);
            Assert.AreEqual("aaa", result[4]);
        }

        [TestMethod]
        public void TestFillWithSource()
        {
            var data = new[] {"dog", "cat", "white", "red", "world"};
            var result = Arr.Fill(2, 3, "aaa", data);
            Assert.AreEqual("dog", result[0]);
            Assert.AreEqual("cat", result[1]);
            Assert.AreEqual("aaa", result[2]);
            Assert.AreEqual("aaa", result[3]);
            Assert.AreEqual("aaa", result[4]);
            Assert.AreEqual("white", result[5]);
            Assert.AreEqual("red", result[6]);
            Assert.AreEqual("world", result[7]);

            Assert.AreEqual("dog" , data[0]);
            Assert.AreEqual("cat", data[1]);
            Assert.AreEqual("white", data[2]);
            Assert.AreEqual("red", data[3]);
            Assert.AreEqual("world", data[4]);
        }

        [TestMethod]
        public void TestFillBoundWithSource()
        {
            var result = Arr.Fill(5, 3, "aaa", new[] { "dog", "cat", "white", "red", "world" });
            Assert.AreEqual("dog", result[0]);
            Assert.AreEqual("cat", result[1]);
            Assert.AreEqual("white", result[2]);
            Assert.AreEqual("red", result[3]);
            Assert.AreEqual("world", result[4]);
            Assert.AreEqual("aaa", result[5]);
            Assert.AreEqual("aaa", result[6]);
            Assert.AreEqual("aaa", result[7]);
        }

        [TestMethod]
        public void TestFillBound2WithSource()
        {
            var result = Arr.Fill(6, 3, "aaa", new[] { "dog", "cat", "white", "red", "world" });
            Assert.AreEqual("dog", result[0]);
            Assert.AreEqual("cat", result[1]);
            Assert.AreEqual("white", result[2]);
            Assert.AreEqual("red", result[3]);
            Assert.AreEqual("world", result[4]);
            Assert.AreEqual(null, result[5]);
            Assert.AreEqual("aaa", result[6]);
            Assert.AreEqual("aaa", result[7]);
            Assert.AreEqual("aaa", result[8]);
        }

        [TestMethod]
        public void TestFillZeroWithSource()
        {
            var result = Arr.Fill(0, 3, "aaa", new[] { "dog", "cat", "white", "red", "world" });
            Assert.AreEqual("aaa", result[0]);
            Assert.AreEqual("aaa", result[1]);
            Assert.AreEqual("aaa", result[2]);
            Assert.AreEqual("dog", result[3]);
            Assert.AreEqual("cat", result[4]);
            Assert.AreEqual("white", result[5]);
            Assert.AreEqual("red", result[6]);
            Assert.AreEqual("world", result[7]);
        }

        [TestMethod]
        public void TestFillException()
        {
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Arr.Fill(-1, 3, "aaa", new[] { "dog", "cat", "white", "red", "world" });
            });

            ExceptionAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Arr.Fill(0, 0, "aaa", new[] { "dog", "cat", "white", "red", "world" });
            });
        }

        [TestMethod]
        public void TestFilter()
        {
            var result = Arr.Filter(new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, (i) => i % 2 == 0);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(6, result[2]);
            Assert.AreEqual(8, result[3]);
            Assert.AreEqual(0, result[4]);
            Assert.AreEqual(5, result.Length);
        }

        [TestMethod]
        public void TestFilterExpected()
        {
            var result = Arr.Filter(new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0}, (i) => i % 2 == 0, false);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(3, result[1]);
            Assert.AreEqual(5, result[2]);
            Assert.AreEqual(7, result[3]);
            Assert.AreEqual(9, result[4]);
            Assert.AreEqual(5, result.Length);
        }

        [TestMethod]
        public void TestFilterIEnumerable()
        {
            var result = Arr.Filter(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, (i) => i % 2 == 0);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(6, result[2]);
            Assert.AreEqual(8, result[3]);
            Assert.AreEqual(0, result[4]);
            Assert.AreEqual(5, result.Length);
        }

        [TestMethod]
        public void TestMap()
        {
            var data = new[] {1, 2, 3};
            var result = Arr.Map(data, (i) => i * 2);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(6, result[2]);

            Assert.AreEqual(1, data[0]);
            Assert.AreEqual(2, data[1]);
            Assert.AreEqual(3, data[2]);
        }

        [TestMethod]
        public void TestMapIEnumerable()
        {
            var data = new List<int>
            {
                1,2,3
            };

            var result = Arr.Map(data, (i) => i * 2);
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(4, result[1]);
            Assert.AreEqual(6, result[2]);

            Assert.AreEqual(1, data[0]);
            Assert.AreEqual(2, data[1]);
            Assert.AreEqual(3, data[2]);
        }

        [TestMethod]
        public void TestMapIntToString()
        {
            var data = new[] { 1, 2, 3 };
            var result = Arr.Map(data, (i) => i.ToString());
            Assert.AreEqual("1", result[0]);
            Assert.AreEqual("2", result[1]);
            Assert.AreEqual("3", result[2]);
        }

        [TestMethod]
        public void TestPop()
        {
            var elements = new[] { 1, 2, 3 };
            var result = Arr.Pop(ref elements);

            Assert.AreEqual(3, result);
            Assert.AreEqual(2, elements.Length);
            Assert.AreEqual(1, elements[0]);
            Assert.AreEqual(2, elements[1]);
        }

        [TestMethod]
        public void TestPush()
        {
            var elements = new[] { 1, 2, 3 };
            var result = Arr.Push(ref elements, 4, 5);
            Assert.AreEqual(5, elements.Length);
            Assert.AreEqual(1, elements[0]);
            Assert.AreEqual(2, elements[1]);
            Assert.AreEqual(3, elements[2]);
            Assert.AreEqual(4, elements[3]);
            Assert.AreEqual(5, elements[4]);
            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void TestReduce()
        {
            var result = Arr.Reduce(new[] { "a", "b", "c" }, (v1, v2) => v1 + "-" + v2, "hello");

            Assert.AreEqual("hello-a-b-c", result);
        }

        [TestMethod]
        public void TestReduceNull()
        {
            var result = Arr.Reduce(new[] { "a", "b", "c" }, (v1, v2) => null, "hello");

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void TestSlice()
        {
            var result = Arr.Slice(new[] { "a", "b", "c" }, 1, -1);
            Assert.AreEqual("b", result[0]);
            Assert.AreEqual(1, result.Length);
        }

        [TestMethod]
        public void TestShift()
        {
            var data = new[] { "a", "b", "c" };
            var result = Arr.Shift(ref data);

            Assert.AreEqual("a", result);
            Assert.AreEqual(2, data.Length);
            Assert.AreEqual("b", data[0]);
            Assert.AreEqual("c", data[1]);
        }

        [TestMethod]
        public void TestUnShift()
        {
            var data = new[] { "c" };
            var result = Arr.Unshift(ref data, "a", "b");

            Assert.AreEqual(3, result);
            Assert.AreEqual(3, data.Length);
            Assert.AreEqual("a", data[0]);
            Assert.AreEqual("b", data[1]);
            Assert.AreEqual("c", data[2]);
        }

        [TestMethod]
        public void TestReverse()
        {
            var data = new[] { "a", "b", "c" };
            var result = Arr.Reverse(data);

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("c", result[0]);
            Assert.AreEqual("b", result[1]);
            Assert.AreEqual("a", result[2]);

            Assert.AreEqual("a", data[0]);
            Assert.AreEqual("b", data[1]);
            Assert.AreEqual("c", data[2]);

            data = new[] {"a"};
            result = Arr.Reverse(data);

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("a", data[0]);
        }

        [TestMethod]
        public void TestReverseWithStartLength()
        {
            var data = new[] { "a", "b", "c", "d", "e" };
            var result = Arr.Reverse(data, 1, 2);

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("c", result[0]);
            Assert.AreEqual("b", result[1]);

            Assert.AreEqual("a", data[0]);
            Assert.AreEqual("b", data[1]);
            Assert.AreEqual("c", data[2]);
            Assert.AreEqual("d", data[3]);
            Assert.AreEqual("e", data[4]);
        }

        [TestMethod]
        public void TestIndexOf()
        {
            var data = new[] { 'a', 'b', 'c', 'd', 'e' };
            var result = Arr.IndexOf(data, new[] { 'c', 'd' });

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void TestIndexOfString()
        {
            var data = new[] { "a", "b", "c", "d", "e" };
            var result = Arr.IndexOf(data, new[] { "d", "e" });

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void TestIndexOfString2()
        {
            var data = new[] { "a", "b", "c", "d", "e" };
            var result = Arr.IndexOf(data, new[] { "d"});

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public void TestIndexOfNull()
        {
            Assert.AreEqual(-1, Arr.IndexOf(null, new[] {"d", "e"}));
        }

        [TestMethod]
        public void TestIndexNotFind()
        {
            var data = new[] { "a", "b", "c", "d", "e" };
            var result = Arr.IndexOf(data, new[] { "e", "d" });

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void TestIndexOfAny()
        {
            var data = new[] { 'a', 'b', 'c', 'd', 'e' };
            var result = Arr.IndexOfAny(data, new[] { 'e', 'd' , 'b' });

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void TestIndexAnyNotFind()
        {
            var data = new[] { 'a', 'b', 'c', 'd', 'e' };
            var result = Arr.IndexOfAny(data, new[] { 'z' });

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void TestIndexOfAnyNull()
        {
            var data = new[] { 'a', 'b', 'c', 'd', 'e' };
            var result = Arr.IndexOfAny(data, null);

            Assert.AreEqual(-1, result);
            result = Arr.IndexOfAny<string>(null, null);

            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        public void TestDifference()
        {
            var data = new[] { "a", "b", "c", "d", "e" };
            var result = Arr.Difference(data, new[] {"e", "d"});

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("a", result[0]);
            Assert.AreEqual("b", result[1]);
            Assert.AreEqual("c", result[2]);
        }

        [TestMethod]
        public void TestDifferenceEmptyMatch()
        {
            var data = new[] { "a", "b", "c", "d", "e" };
            var result = Arr.Difference(data, null);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual("a", result[0]);
            Assert.AreEqual("b", result[1]);
            Assert.AreEqual("c", result[2]);
            Assert.AreEqual("d", result[3]);
            Assert.AreEqual("e", result[4]);
        }

        [TestMethod]
        public void TestRemoveAt()
        {
            var data = new[] { "a", "b", "c", "d", "e" };
            var result = Arr.RemoveAt(ref data, 1);
            Assert.AreEqual("b", result);
            Assert.AreEqual("a", data[0]);
            Assert.AreEqual("c", data[1]);
            Assert.AreEqual("d", data[2]);
            Assert.AreEqual("e", data[3]);
            Assert.AreEqual(4, data.Length);

            result = Arr.RemoveAt(ref data, 3);
            Assert.AreEqual("e", result);
            Assert.AreEqual("a", data[0]);
            Assert.AreEqual("c", data[1]);
            Assert.AreEqual("d", data[2]);
            Assert.AreEqual(3, data.Length);

            result = Arr.RemoveAt(ref data, 0);
            Assert.AreEqual("a", result);
            Assert.AreEqual("c", data[0]);
            Assert.AreEqual("d", data[1]);
            Assert.AreEqual(2, data.Length);

            bool ex = false;
            try
            {
                result = Arr.RemoveAt(ref data, 2);
            }
            catch (Exception)
            {
                ex = true;
            }
            Assert.AreEqual(true, ex);

            result = Arr.RemoveAt(ref data, 0);
            Assert.AreEqual("c", result);
            Assert.AreEqual("d", data[0]);
            Assert.AreEqual(1, data.Length);
        }

        [TestMethod]
        public void TestRemoveAtNegativeNumber()
        {
            var data = new[] { "a", "b", "c", "d", "e" };
            var result = Arr.RemoveAt(ref data, -2);
            Assert.AreEqual("d", result);
            Assert.AreEqual("a", data[0]);
            Assert.AreEqual("b", data[1]);
            Assert.AreEqual("c", data[2]);
            Assert.AreEqual("e", data[3]);
            Assert.AreEqual(4, data.Length);

            result = Arr.RemoveAt(ref data, -1);
            Assert.AreEqual("e", result);
            Assert.AreEqual("a", data[0]);
            Assert.AreEqual("b", data[1]);
            Assert.AreEqual("c", data[2]);
            Assert.AreEqual(3, data.Length);

            result = Arr.RemoveAt(ref data, -999);
            Assert.AreEqual("a", result);
            Assert.AreEqual("b", data[0]);
            Assert.AreEqual("c", data[1]);
            Assert.AreEqual(2, data.Length);

            result = Arr.RemoveAt(ref data, -1);
            Assert.AreEqual("c", result);
            Assert.AreEqual("b", data[0]);
            Assert.AreEqual(1, data.Length);

            result = Arr.RemoveAt(ref data, -1);
            Assert.AreEqual("b", result);
            Assert.AreEqual(0, data.Length);

            data = new string[] { };
            result = Arr.RemoveAt(ref data, -1);
            Assert.AreEqual(null, result);
            Assert.AreEqual(0, data.Length);
        }

        [TestMethod]
        public void TestCut()
        {
            var data = new char[] { '1', '2', '3', '4', '5' };

            Arr.Cut(ref data, 1);
            Assert.AreEqual('2', data[0]);
            Assert.AreEqual(4, data.Length);
            Arr.Cut(ref data, -1);
            Assert.AreEqual('4', data[2]);
            Assert.AreEqual(3, data.Length);
            Arr.Cut(ref data, 0);
            Assert.AreEqual(3, data.Length);
            Arr.Cut(ref data, 10);
            Assert.AreEqual(0, data.Length);
            data = new char[] { '1', '2', '3', '4', '5' };
            Arr.Cut(ref data, -10);
            Assert.AreEqual(0, data.Length);
            data = new char[] { '1', '2', '3', '4', '5' };
            Arr.Cut(ref data, 3);
            Assert.AreEqual('4', data[0]);
            Assert.AreEqual(2, data.Length);
            data = new char[] { '1', '2', '3', '4', '5' };
            Arr.Cut(ref data, -3);
            Assert.AreEqual('2', data[1]);
            Assert.AreEqual(2, data.Length);
            char[] obj = null;
            Arr.Cut(ref obj, 100);
            obj = new char[0];
            Arr.Cut(ref obj, 100);
        }

        [TestMethod]
        public void TestRemove()
        {
            var data = new char[] { '1', '2', '3', '4', '5' };
            var result = Arr.Remove(ref data, (c) => c == '3');
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual('3', result[0]);
            Assert.AreEqual(4, data.Length);
            Assert.AreEqual('4', data[2]);

            var data2 = new int[] { 1, 2, 3, 4, 5 };
            var result2 = Arr.Remove(ref data2, (c) => c % 2 == 0);
            Assert.AreEqual(2, result2.Length);
            Assert.AreEqual(2, result2[0]);
            Assert.AreEqual(4, result2[1]);
            Assert.AreEqual(3, data2.Length);
            Assert.AreEqual(1, data2[0]);
            Assert.AreEqual(3, data2[1]);
            Assert.AreEqual(5, data2[2]);

            var data3 = new int[0];
            var result3 = Arr.Remove(ref data3, (c) => c % 2 == 0);
            Assert.AreEqual(0, result3.Length);
            Assert.AreEqual(0, data3.Length);
        }

        [TestMethod]
        public void TestTest()
        {
            var assembiles = new string[]
            {
                "CatLib.Core",
                "CatLib.ILRuntime",
                "CatLib.Route",
                "Hello.World"
            };

            Assert.AreEqual(true, Arr.Test(assembiles, (assembly) => Str.Is("CatLib.*", assembly)));
        }

        [TestMethod]
        public void TestSetReplace()
        {
            var set = new[] {"a", "ab", "abc", "abcd", "abcdef"};

            Arr.Set(ref set, (element) => element == "abc", "hello");
            Assert.AreEqual("a", set[0]);
            Assert.AreEqual("ab", set[1]);
            Assert.AreEqual("hello", set[2]);
            Assert.AreEqual("abcd", set[3]);
            Assert.AreEqual("abcdef", set[4]);
        }

        [TestMethod]
        public void TestSetPush()
        {
            var set = new [] { "a", "ab", "abc", "abcd", "abcdef" };

            Arr.Set(ref set, (element) => element == "none", "hello");
            Assert.AreEqual("a", set[0]);
            Assert.AreEqual("ab", set[1]);
            Assert.AreEqual("abc", set[2]);
            Assert.AreEqual("abcd", set[3]);
            Assert.AreEqual("abcdef", set[4]);
            Assert.AreEqual("hello", set[5]);
        }
    }
}
