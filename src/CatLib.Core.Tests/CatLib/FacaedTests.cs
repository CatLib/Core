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
        public void FacadeErrorTest()
        {
            var app = new Application();
            app.Bootstrap();
            app.Singleton<FacaedTestClass>();

            var isError = false;
            try
            {
                var data = TestClassFacadeError.Instance;
            }
            catch (TypeInitializationException)
            {
                isError = true;
            }

            Assert.AreEqual(true, isError);
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
        /// 门面测试
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
    }
}
