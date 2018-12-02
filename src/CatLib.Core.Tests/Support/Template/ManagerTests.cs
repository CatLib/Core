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

using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Stl
{
    [TestClass]
    public sealed class ManagerTests
    {
        private class TestManagerClass : Manager<TestManagerClass>
        {
            public void GetResolvePublic(string name)
            {
                MakeExtend(name);
            }
        }

        [TestMethod]
        public void TestExtend()
        {
            var cls = new TestManagerClass();

            TestManagerClass tmp = null;
            cls.Extend(() =>
            {
                return tmp = new TestManagerClass();
            });

            var result = cls[null];

            Assert.AreSame(tmp, result);
        }

        [TestMethod]
        public void TestReleaseExtendAndMake()
        {
            var cls = new TestManagerClass();

            TestManagerClass tmp = null;
            cls.Extend(() =>
            {
                return tmp = new TestManagerClass();
            });

            cls.RemoveExtend();

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                cls.Get();
            });
        }

        [TestMethod]
        public void TestRepeatExtend()
        {
            var cls = new TestManagerClass();

            cls.Extend(() => null, "test");
            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                cls.Extend(() => null, "test");
            });
        }

        [TestMethod]
        public void TestGetExtend()
        {
            var cls = new TestManagerClass();

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                cls.GetResolvePublic(null);
            });

            cls.Extend(() =>
            {
                return new TestManagerClass();
            });
        }

        [TestMethod]
        public void TestOnAfterExtend()
        {
            var cls = new TestManagerClass();

            TestManagerClass tmp = null;
            cls.Extend(() =>
            {
                return tmp = new TestManagerClass();
            });

            var isResolving = false;
            var isAfterResolving = false;
            cls.OnAfterResolving += (instance) =>
            {
                Assert.AreEqual(true, isResolving);
                isAfterResolving = true;
            };
            cls.OnResolving += (instance) =>
            {
                isResolving = true;
            };

            var result = cls[null];
            Assert.AreEqual(true, isAfterResolving);
            Assert.AreSame(tmp, result);
        }
    }
}
