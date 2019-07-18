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

using CatLib.Container;
using CatLib.Tests.Fixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests
{
    [TestClass]
    public class TestsFacade
    {
        private Application application;

        [TestInitialize]
        public void Initialize()
        {
            application = new Application();
            application.Bootstrap();
        }

        [TestMethod]
        public void TestFacade()
        {
            application.Singleton<IFoo, Foo>();
            var foo = Facade<IFoo>.Instance;

            Assert.AreSame(foo, Facade<IFoo>.Instance);
        }

        [TestMethod]
        public void TestAlwaysWatchNewer()
        {
            application.Singleton<IFoo, Foo>();
            var older = Facade<IFoo>.Instance;

            application.Unbind<IFoo>();
            application.Singleton<IFoo, Foo>();
            var newer = Facade<IFoo>.Instance;

            Assert.AreNotSame(older, newer);
        }

        [TestMethod]
        public void TestAlwaysWatchWithInstance()
        {
            application.Singleton<IFoo, Foo>();
            Assert.AreNotEqual(null, Facade<IFoo>.Instance);

            var foo = new Foo();
            application.Instance<IFoo>(foo);
            Assert.AreSame(foo, Facade<IFoo>.Instance);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestFacadeMakeFaild()
        {
            application.Singleton<IFoo, Foo>();
            Assert.AreNotEqual(null, Facade<IFoo>.Instance);

            application.Unbind<IFoo>();
            _ = Facade<IFoo>.Instance;
        }

        [TestMethod]
        public void TestFacadeRelease()
        {
            application.Singleton<IFoo, Foo>();

            var foo = Facade<IFoo>.Instance;
            Assert.AreSame(foo, Facade<IFoo>.Instance);

            application.Release<IFoo>();
            Assert.AreNotSame(foo, Facade<IFoo>.Instance);
        }

        [TestMethod]
        public void TestNotStaticBindFacade()
        {
            application.Bind<IFoo, Foo>();

            var foo = Facade<IFoo>.Instance;
            Assert.AreNotSame(foo, Facade<IFoo>.Instance);
        }

        [TestMethod]
        public void TestSingletonChangeToBind()
        {
            application.Singleton<IFoo, Foo>();

            var foo = Facade<IFoo>.Instance;
            Assert.AreSame(foo, Facade<IFoo>.Instance);

            application.Unbind<IFoo>();
            application.Bind<IFoo, Foo>();

            Assert.AreNotSame(foo, Facade<IFoo>.Instance);
            Assert.AreNotSame(Facade<IFoo>.Instance, Facade<IFoo>.Instance);
        }

        [TestMethod]
        public void TestBindChangeToSingleton()
        {
            application.Bind<IFoo, Foo>();

            var foo = Facade<IFoo>.Instance;
            Assert.AreNotSame(foo, Facade<IFoo>.Instance);

            application.Unbind<IFoo>();
            application.Singleton<IFoo, Foo>();

            Assert.AreNotSame(foo, Facade<IFoo>.Instance);
            Assert.AreSame(Facade<IFoo>.Instance, Facade<IFoo>.Instance);
        }

        [TestMethod]
        public void TestInstance()
        {
            application.Instance<IFoo>(new Foo());

            var foo = Facade<IFoo>.Instance;
            Assert.AreSame(foo, Facade<IFoo>.Instance);
        }
    }
}
