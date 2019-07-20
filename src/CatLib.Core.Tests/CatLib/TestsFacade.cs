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
            var foo = Facade<IFoo>.That;

            Assert.AreSame(foo, Facade<IFoo>.That);
        }

        [TestMethod]
        public void TestAlwaysWatchNewer()
        {
            application.Singleton<IFoo, Foo>();
            var older = Facade<IFoo>.That;

            application.Unbind<IFoo>();
            application.Singleton<IFoo, Foo>();
            var newer = Facade<IFoo>.That;

            Assert.AreNotSame(older, newer);
        }

        [TestMethod]
        public void TestAlwaysWatchWithInstance()
        {
            application.Singleton<IFoo, Foo>();
            Assert.AreNotEqual(null, Facade<IFoo>.That);

            var foo = new Foo();
            application.Instance<IFoo>(foo);
            Assert.AreSame(foo, Facade<IFoo>.That);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestFacadeMakeFaild()
        {
            application.Singleton<IFoo, Foo>();
            Assert.AreNotEqual(null, Facade<IFoo>.That);

            application.Unbind<IFoo>();
            _ = Facade<IFoo>.That;
        }

        [TestMethod]
        public void TestFacadeRelease()
        {
            application.Singleton<IFoo, Foo>();

            var foo = Facade<IFoo>.That;
            Assert.AreSame(foo, Facade<IFoo>.That);

            application.Release<IFoo>();
            Assert.AreNotSame(foo, Facade<IFoo>.That);
        }

        [TestMethod]
        public void TestNotStaticBindFacade()
        {
            application.Bind<IFoo, Foo>();

            var foo = Facade<IFoo>.That;
            Assert.AreNotSame(foo, Facade<IFoo>.That);
        }

        [TestMethod]
        public void TestSingletonChangeToBind()
        {
            application.Singleton<IFoo, Foo>();

            var foo = Facade<IFoo>.That;
            Assert.AreSame(foo, Facade<IFoo>.That);

            application.Unbind<IFoo>();
            application.Bind<IFoo, Foo>();

            Assert.AreNotSame(foo, Facade<IFoo>.That);
            Assert.AreNotSame(Facade<IFoo>.That, Facade<IFoo>.That);
        }

        [TestMethod]
        public void TestBindChangeToSingleton()
        {
            application.Bind<IFoo, Foo>();

            var foo = Facade<IFoo>.That;
            Assert.AreNotSame(foo, Facade<IFoo>.That);

            application.Unbind<IFoo>();
            application.Singleton<IFoo, Foo>();

            Assert.AreNotSame(foo, Facade<IFoo>.That);
            Assert.AreSame(Facade<IFoo>.That, Facade<IFoo>.That);
        }

        [TestMethod]
        public void TestInstance()
        {
            application.Instance<IFoo>(new Foo());

            var foo = Facade<IFoo>.That;
            Assert.AreSame(foo, Facade<IFoo>.That);
        }
    }
}
