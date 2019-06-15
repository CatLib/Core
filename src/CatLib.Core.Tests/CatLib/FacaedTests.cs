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

#pragma warning disable CA1034

using CatLib.Container;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests
{
    [TestClass]
    public class FacaedTests
    {
        public class FacaedTestClass : IFacaedTestClass
        {
        }

        public interface IFacaedTestClass
        {
        }

        public class TestClassFacade : Facade<IFacaedTestClass>
        {
        }

        public class TestClassFacadeError : Facade<FacaedTestClass>
        {
        }

        [TestMethod]
        public void FacadeWatchTest()
        {
            var app = new Application();
            app.Bootstrap();
            app.Singleton<FacaedTestClass>().Alias<IFacaedTestClass>();
            var old = TestClassFacade.Instance;
            app.Unbind<FacaedTestClass>();
            app.Singleton<FacaedTestClass>().Alias<IFacaedTestClass>();

            Assert.AreNotSame(old, TestClassFacade.Instance);
        }

        [TestMethod]
        public void FacadeWatchTestWithInstance()
        {
            var app = new Application();
            app.Bootstrap();
            app.Singleton<FacaedTestClass>().Alias<IFacaedTestClass>();

            var cls = new FacaedTestClass();
            app.Instance<FacaedTestClass>(cls);

            Assert.AreSame(cls, TestClassFacade.Instance);
        }

        [TestMethod]
        public void FacadeMakeFaild()
        {
            var app = new Application();
            app.Bootstrap();
            app.Singleton<FacaedTestClass>().Alias<IFacaedTestClass>();
            var old = TestClassFacade.Instance;

            Assert.AreNotEqual(null, old);
            app.Unbind<FacaedTestClass>();

            var isError = false;
            try
            {
                var data = TestClassFacade.Instance;
            }
            catch (UnresolvableException)
            {
                isError = true;
            }

            Assert.AreEqual(true, isError);
        }

        /// <summary>
        /// 门面测试.
        /// </summary>
        [TestMethod]
        public void FacadeTest()
        {
            var app = new Application();
            app.Bootstrap();
            IFacaedTestClass obj = new FacaedTestClass();
            app.Singleton<FacaedTestClass>((c, p) =>
            {
                return obj;
            }).Alias<IFacaedTestClass>();

            Assert.AreSame(obj, TestClassFacade.Instance);

            //double run
            Assert.AreSame(obj, TestClassFacade.Instance);
            Assert.AreSame(obj, TestClassFacade.Instance);
        }

        [TestMethod]
        public void FacadeReleaseTest()
        {
            var app = new Application();
            app.Bootstrap();
            app.Singleton<FacaedTestClass>().Alias<IFacaedTestClass>();

            var data = TestClassFacade.Instance;
            Assert.AreSame(data, TestClassFacade.Instance);
            app.Release<IFacaedTestClass>();
            Assert.AreNotSame(data, TestClassFacade.Instance);
        }

        [TestMethod]
        public void TestNotStaticBindFacade()
        {
            var app = new Application();
            app.Bootstrap();
            app.Bind<FacaedTestClass>().Alias<IFacaedTestClass>();

            var data = TestClassFacade.Instance;
            Assert.AreNotSame(data, TestClassFacade.Instance);
            Assert.AreNotSame(TestClassFacade.Instance, TestClassFacade.Instance);
        }

        [TestMethod]
        public void TestBindingStateSwitchSingletonToBind()
        {
            var app = new Application();
            app.Bootstrap();
            app.Singleton<FacaedTestClass>().Alias<IFacaedTestClass>();

            var data = TestClassFacade.Instance;
            Assert.AreSame(data, TestClassFacade.Instance);

            app.Unbind<IFacaedTestClass>();
            app.Bind<FacaedTestClass>().Alias<IFacaedTestClass>();
            Assert.AreNotSame(data, TestClassFacade.Instance);
            Assert.AreNotSame(TestClassFacade.Instance, TestClassFacade.Instance);
        }

        [TestMethod]
        public void TestBindingStateSwitchBindToSingleton()
        {
            var app = new Application();
            app.Bootstrap();
            app.Bind<FacaedTestClass>().Alias<IFacaedTestClass>();

            var data = TestClassFacade.Instance;
            Assert.AreNotSame(data, TestClassFacade.Instance);
            Assert.AreNotSame(TestClassFacade.Instance, TestClassFacade.Instance);

            app.Unbind<IFacaedTestClass>();
            app.Singleton<FacaedTestClass>().Alias<IFacaedTestClass>();
            data = TestClassFacade.Instance;
            Assert.AreSame(data, TestClassFacade.Instance);
            Assert.AreSame(TestClassFacade.Instance, TestClassFacade.Instance);
        }

        [TestMethod]
        public void TestNotBind()
        {
            var app = new Application();
            app.Bootstrap();
            app.Instance<IFacaedTestClass>(new FacaedTestClass());

            var data = TestClassFacade.Instance;
            Assert.AreSame(data, TestClassFacade.Instance);

            app.Release<IFacaedTestClass>();

            var isError = false;
            try
            {
                data = TestClassFacade.Instance;
            }
            catch (UnresolvableException)
            {
                isError = true;
            }

            Assert.AreEqual(true, isError);
        }

        [TestMethod]
        public void TestStructBindAndRebound()
        {
            var app = new Application();
            app.Bootstrap();
            var makeCount = 0;
            var binder = app.Bind<int>(() =>
            {
                makeCount++;
                return 100;
            });
            Assert.AreEqual(100, Facade<int>.Instance);
            Assert.AreEqual(100, Facade<int>.Instance); // double check
            Assert.AreEqual(2, makeCount);
            binder.Unbind();
            Assert.AreEqual(0, Facade<int>.Instance);
            Assert.AreEqual(0, Facade<int>.Instance); // double check
            app.Bind<int>(() =>
            {
                makeCount++;
                return 200;
            });
            Assert.AreEqual(200, Facade<int>.Instance);
            Assert.AreEqual(200, Facade<int>.Instance); // double check
            Assert.AreEqual(5, makeCount); // 其中多出的一个计数是门面的watch导致的。
        }

        [TestMethod]
        public void TestStructSingleAndRebound()
        {
            var app = new Application();
            app.Bootstrap();
            var makeCount = 0;
            var binder = app.Singleton<int>(() =>
            {
                makeCount++;
                return 100;
            });
            Assert.AreEqual(100, Facade<int>.Instance);
            Assert.AreEqual(100, Facade<int>.Instance); // double check
            Assert.AreEqual(1, makeCount);
            app.Instance<int>(200);
            Assert.AreEqual(200, Facade<int>.Instance);
            Assert.AreEqual(200, Facade<int>.Instance); // double check
            Assert.AreEqual(1, makeCount);
            binder.Unbind();
            Assert.AreEqual(0, Facade<int>.Instance);
            Assert.AreEqual(0, Facade<int>.Instance); // double check
            Assert.AreEqual(1, makeCount);
            binder = app.Singleton<int>(() =>
            {
                makeCount++;
                return 200;
            });
            Assert.AreEqual(200, Facade<int>.Instance);
            Assert.AreEqual(200, Facade<int>.Instance); // double check
            Assert.AreEqual(2, makeCount);

            binder.Unbind(); // 三次连续测试来测试一个特殊情况
            Assert.AreEqual(0, Facade<int>.Instance);
            Assert.AreEqual(0, Facade<int>.Instance); // double check
            Assert.AreEqual(2, makeCount);
            binder = app.Singleton<int>(() =>
            {
                makeCount++;
                return 300;
            });
            Assert.AreEqual(300, Facade<int>.Instance);
            Assert.AreEqual(300, Facade<int>.Instance); // double check
            Assert.AreEqual(3, makeCount);
            app.Release<int>();
            Assert.AreEqual(300, Facade<int>.Instance);
            Assert.AreEqual(4, makeCount);
        }

        [TestMethod]
        public void TestStructSingleToBindAndRebound()
        {
            var app = new Application();
            app.Bootstrap();
            var makeCount = 0;
            var binder = app.Bind<int>(() =>
            {
                makeCount++;
                return 100;
            });
            Assert.AreEqual(100, Facade<int>.Instance);
            Assert.AreEqual(100, Facade<int>.Instance); // double check
            Assert.AreEqual(2, makeCount);

            binder.Unbind();
            Assert.AreEqual(0, Facade<int>.Instance);

            binder = app.Singleton<int>(() =>
            {
                makeCount++;
                return 200;
            });
            Assert.AreEqual(200, Facade<int>.Instance);
            Assert.AreEqual(200, Facade<int>.Instance); // double check
            Assert.AreEqual(3, makeCount);
        }

        [TestMethod]
        public void TestStructSingleRelease()
        {
            var app = new Application();
            app.Bootstrap();
            var makeCount = 0;
            var binder = app.Singleton<int>(() =>
            {
                makeCount++;
                return 100;
            });
            Assert.AreEqual(100, Facade<int>.Instance);
            Assert.AreEqual(100, Facade<int>.Instance); // double check
            Assert.AreEqual(1, makeCount);
            app.Release<int>();
            Assert.AreEqual(100, Facade<int>.Instance);
            app.Instance<int>(200);
            Assert.AreEqual(200, Facade<int>.Instance);
            app.Release<int>();
            app.Instance<int>(300);
            Assert.AreEqual(300, Facade<int>.Instance);
        }

        [TestMethod]
        public void TestStructBindToSingleRebound()
        {
            var app = new Application();
            app.Bootstrap();
            var makeCount = 0;
            var binder = app.Bind<int>(() =>
            {
                makeCount++;
                return 100;
            });
            Assert.AreEqual(100, Facade<int>.Instance);
            Assert.AreEqual(100, Facade<int>.Instance); // double check
            Assert.AreEqual(2, makeCount);

            Assert.AreEqual(false, app.Release<int>());
            binder.Unbind();
            binder = app.Singleton<int>(() =>
            {
                makeCount++;
                return 200;
            });
            Assert.AreEqual(3, makeCount);
            Assert.AreEqual(200, Facade<int>.Instance);
            Assert.AreEqual(200, Facade<int>.Instance);
            Assert.AreEqual(3, makeCount);

            Assert.AreEqual(true, app.Release<int>());
            Assert.AreEqual(200, Facade<int>.Instance);
            Assert.AreEqual(200, Facade<int>.Instance);
            Assert.AreEqual(4, makeCount);

            Assert.AreEqual(true, app.Release<int>());
            app.Instance<int>(300);
            Assert.AreEqual(300, Facade<int>.Instance);
            Assert.AreEqual(300, Facade<int>.Instance);
            Assert.AreEqual(4, makeCount);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestEmptyFacade()
        {
            var app = new Application();
            app.Bootstrap();
            var ins = Facade<IBindData>.Instance;
        }
    }
}
