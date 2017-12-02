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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Stl
{
    [TestClass]
    public sealed class ManagerTests
    {
        private class TestManagerClass : Manager<TestManagerClass>
        {
            public Func<TestManagerClass> GetResolvePublic(string name)
            {
                return GetExtend(name);
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

            cls.ReleaseExtend();

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

            Assert.AreNotEqual(null, cls.GetResolvePublic(null));
        }
    }
}
