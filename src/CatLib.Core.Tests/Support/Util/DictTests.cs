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

using System.Collections.Generic;
using CatLib.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Core.Tests.Support.Util
{
    [TestClass]
    public class DictTests
    {
        [TestMethod]
        public void TestGet()
        {
            var dict = new Dictionary<string,object>
            {
                { "my" , new Dictionary<string,object>
                {
                    {"name" , new Dictionary<string,object>
                    {
                        { "is" , "catlib" }
                    }},

                    {"age" , new Dictionary<string,object>
                    {
                        { "is" , 18 }
                    }},
                } }
            };

            Assert.AreEqual("catlib", Dict.Get(dict, "my.name.is"));
            Assert.AreEqual(18, Dict.Get(dict, "my.age.is"));
            Assert.AreEqual("undefiend", Dict.Get(dict, "my.age.undefiend","undefiend"));

            Assert.AreEqual("undefiend", Dict.Get(null, "my.age.undefiend", "undefiend"));
            Assert.AreEqual(dict, Dict.Get(dict, null, "undefiend"));

            Assert.AreEqual("undefiend", Dict.Get(dict, "my.age.is.name", "undefiend"));
        }

        [TestMethod]
        public void TestSetRemove()
        {
            var dict = new Dictionary<string, object>();

            Dict.Set(dict, "hello.world", "hello");
            Dict.Set(dict, "hello.name", "catlib");
            Dict.Set(dict, "hello.world.name", "c#");
            Dict.Set(dict, "hello.world.just", "j#");

            Assert.AreEqual(((Dictionary<string, object>)dict["hello"])["world"], Dict.Get(dict, "hello.world"));
            Assert.AreEqual("c#", Dict.Get(dict, "hello.world.name"));
            Assert.AreEqual("j#", Dict.Get(dict, "hello.world.just"));
            Assert.AreEqual("catlib", Dict.Get(dict, "hello.name"));

            Dict.Remove(dict, "hello.world.name");
            Dict.Remove(dict, "hello.world.just");

            Assert.AreEqual("undefiend", Dict.Get(dict, "hello.world", "undefiend"));
            Assert.AreEqual("catlib", Dict.Get(dict, "hello.name", "undefiend"));

            dict = new Dictionary<string, object>();
            Dict.Set(dict, "hello.world.name.is", "hello");
            Dict.Remove(dict, "hello.world.name.is");
            Assert.AreEqual("undefiend", Dict.Get(dict, "hello", "undefiend"));

            Assert.AreEqual(false, Dict.Remove(dict, "notexists.notexists"));
            Assert.AreEqual(false, Dict.Remove(dict, "hello.name.is.world"));
        }

        [TestMethod]
        public void TestSetNotObject()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("string", "string");
            Dict.Set(dict, "string.hello", "hello");
        }

        [TestMethod]
        public void TestKeys()
        {
            var dict = new Dictionary<string, object>
            {
                {"1", 1},
                {"2", 1},
                {"3", 1}
            };

            var arr = Dict.Keys(dict);

            Assert.AreEqual("1", arr[0]);
            Assert.AreEqual("2", arr[1]);
            Assert.AreEqual("3", arr[2]);
        }

        [TestMethod]
        public void TestValues()
        {
            var dict = new Dictionary<string, object>
            {
                {"1", 1},
                {"2", 2},
                {"3", 3}
            };

            var arr = Dict.Values(dict);

            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);
        }

        [TestMethod]
        public void TestMap()
        {
            var dict = new Dictionary<string, int>
            {
                {"1", 1},
                {"2", 2},
                {"3", 3}
            };

            var arr = Dict.Values(Dict.Map(dict, (key, val) => val * 2));

            Assert.AreEqual(2, arr[0]);
            Assert.AreEqual(4, arr[1]);
            Assert.AreEqual(6, arr[2]);
        }

        [TestMethod]
        public void TestModify()
        {
            var dict = new Dictionary<string, int>
            {
                {"1", 1},
                {"2", 2},
                {"3", 3}
            };

            Dict.Modify(dict, (k, v) => v * 2);

            var arr = Dict.Values(dict);

            Assert.AreEqual(2, arr[0]);
            Assert.AreEqual(4, arr[1]);
            Assert.AreEqual(6, arr[2]);
        }

        [TestMethod]
        public void TestRemove()
        {
            var dict = new Dictionary<string, int>
            {
                {"1", 1},
                {"2", 2},
                {"3", 3}
            };

            Dict.Remove(dict, (k, v) => v == 2);
            var arr = Dict.Values(dict);

            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(3, arr[1]);
        }

        [TestMethod]
        public void TestFilter()
        {
            var dict = new Dictionary<string, int>
            {
                {"1", 1},
                {"2", 2},
                {"3", 3}
            };

            var arr = Dict.Values(Dict.Filter(dict, (k, v) => v == 2));
            Assert.AreEqual(2, arr[0]);
        }

        [TestMethod]
        public void TestAddRang()
        {
            var dict = new Dictionary<string, int>
            {
                {"1", 1},
                {"2", 2},
                {"3", 3}
            };

            Dict.AddRange(dict, new Dictionary<string, int>
            {
                { "9", 9 },
                { "10", 10 }
            });

            ExceptionAssert.Throws(() =>
            {
                Dict.AddRange(dict, new Dictionary<string, int>
                {
                    {"9", 9},
                    {"10", 10}
                }, false);
            });

            Dict.AddRange(dict, new Dictionary<string, int>
            {
                {"10", 12}
            });

            ExceptionAssert.DoesNotThrow(() =>
            {
                Dict.AddRange(dict, null);
            });

            Assert.AreEqual(true, dict.ContainsKey("9"));
            Assert.AreEqual(true, dict.ContainsKey("10"));
            Assert.AreEqual(12, dict["10"]);
        }
    }
}
