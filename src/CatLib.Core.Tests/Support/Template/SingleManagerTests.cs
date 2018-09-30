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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Stl
{
    [TestClass]
    public class SingleManagerTests
    {
        public interface ITestInterface
        {
            string Call();
        }

        public class InterfaceImpl : ITestInterface
        {
            public string Call()
            {
                return "InterfaceImpl";
            }
        }

        public class TestManager : SingleManager<ITestInterface>
        {

        }

        [TestMethod]
        public void TestSingleGet()
        {
            var manager = new TestManager();
            manager.Extend(() => new InterfaceImpl());
            Assert.AreSame(manager.Default, manager.Get());
        }

        [TestMethod]
        public void TestCoverToSingleManagerGet()
        {
            var manager = new TestManager();
            manager.Extend(() => new InterfaceImpl());
            var manager2 = manager as SingleManager<ITestInterface>;
            Assert.AreSame(manager.Default, manager2.Get());
        }

        [TestMethod]
        public void TestContains()
        {
            var manager = new TestManager();
            manager.Extend(() => new InterfaceImpl());
            bool resolve = false, release = false;
            manager.OnResolving += (_) =>
            {
                resolve = true;
            };
            manager.OnRelease += (_) =>
            {
                release = true;
            };
            Assert.AreEqual(false, manager.Contains());
            Assert.AreEqual(false, resolve);
            Assert.AreEqual(false, release);
            manager.Get();
            Assert.AreEqual(true, resolve);
            Assert.AreEqual(true, manager.Contains());
            manager.Release();
            Assert.AreEqual(true, release);
            Assert.AreEqual(false, manager.Contains());
        }

        [TestMethod]
        public void TestCoverToInterfaceSingleManagerGet()
        {
            var manager = new TestManager();
            manager.Extend(() => new InterfaceImpl());
            var manager2 = manager as ISingleManager<ITestInterface>;
            Assert.AreSame(manager.Default, manager2.Get());
        }

        [TestMethod]
        public void TestManagerRelease()
        {
            var manager = new TestManager();
            manager.Extend(() => new InterfaceImpl());
            var manager2 = manager as ISingleManager<ITestInterface>;
            var def = manager.Default;
            manager.Release();

            Assert.AreNotSame(def, manager2.Default);
        }

        [TestMethod]
        public void TestContainsExtend()
        {
            var manager = new TestManager();
            manager.Extend(() => new InterfaceImpl(), "hello");

            Assert.AreEqual(false, manager.ContainsExtend());
            Assert.AreEqual(true, manager.ContainsExtend("hello"));
        }

        [TestMethod]
        public void TestReleaseExtend()
        {
            var manager = new TestManager();
            manager.Extend(() => new InterfaceImpl(), "hello");

            Assert.AreEqual(true, manager.ContainsExtend("hello"));
            manager.ReleaseExtend("hello");
            Assert.AreEqual(false, manager.ContainsExtend());
            Assert.AreEqual(false, manager.ContainsExtend("hello"));
        }
    }
}
