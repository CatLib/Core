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
using CatLib.EventDispatcher;
using CatLib.Exception;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace CatLib.Tests
{
    [TestClass]
    public class TestsApplication
    {
        private Application application;
        private IEventDispatcher dispatcher;

        [TestInitialize]
        public void Initialize()
        {
            application = new Application();
            dispatcher = application.Make<IEventDispatcher>();
        }

        [TestMethod]
        public void TestGetFileVersion()
        {
            var expected = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            Assert.AreEqual(expected, Application.Version);
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestBootstrapWithRepeat()
        {
            application.BootstrapWith();
            application.Boot();
            application.BootstrapWith();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestInitRepeat()
        {
            application.BootstrapWith();
            application.Boot();
            application.Boot();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestInitNoBootstrap()
        {
            application.Boot();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestRegisterRepeat()
        {
            var foo = new Mock<IServiceProvider>();
            application.Register(foo.Object);
            application.Register(foo.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestInitingRegister()
        {
            var foo = new Mock<IServiceProvider>();
            var bar = new Mock<IServiceProvider>();

            application.Booting((app) =>
            {
                application.Register(foo.Object);
            });

            application.Register(bar.Object);
            application.BootstrapWith();
            application.Boot();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestTerminateRegisterThrowException()
        {
            var foo = new Mock<IServiceProvider>();

            application.Terminating((app) =>
            {
                application.Register(foo.Object);
            });

            application.BootstrapWith();
            application.Boot();
            application.Terminate();
        }

        [TestMethod]
        public void TestTerminateSequenceOfEvents()
        {
            var count = 0;

            application.Terminating((app) =>
            {
                Assert.AreEqual(0, count++);
            });

            application.Terminated((app) =>
            {
                Assert.AreEqual(1, count++);
            });

            application.Terminate();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void TestGetProcess()
        {
            Assert.AreEqual(StartProcess.Construct, application.Process);
        }

        [TestMethod]
        [DataRow(DebugLevel.Development)]
        [DataRow(DebugLevel.Production)]
        [DataRow(DebugLevel.Staging)]
        public void TestDebugLevel(DebugLevel expected)
        {
            application.DebugLevel = expected;
            Assert.AreEqual(expected, application.DebugLevel);
        }

        [TestMethod]
        public void TestGetRuntimeId()
        {
            Assert.AreNotEqual(application.GetRuntimeId(), application.GetRuntimeId());
        }

        [TestMethod]
        public async Task TestIsMainThread()
        {
            Assert.AreEqual(true, application.IsMainThread);
            await Task.Run(() =>
            {
                Assert.AreEqual(false, application.IsMainThread);
            }).ConfigureAwait(false);
        }

        [TestMethod]
        public void TestInitAfterRegister()
        {
            var foo = new Mock<IServiceProvider>();
            var bar = new Mock<IServiceProvider>();

            application.BootstrapWith();
            application.Register(foo.Object);
            application.Boot();
            application.Register(bar.Object);

            foo.Verify((o) => o.Register(), Times.Once);
            bar.Verify((o) => o.Register(), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestRegistingMake()
        {
            var foo = new Mock<IServiceProvider>();
            foo.Setup((o) => o.Register()).Callback(() =>
            {
                application.Make("foo");
            });

            application.Register(foo.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestBootstrapWithSameBootstrap()
        {
            var foo = new Mock<IBootstrap>().Object;
            application.BootstrapWith(foo, foo);
        }

        [TestMethod]
        public void TestBoostrapWithOrder()
        {
            var foo = new Mock<IBootstrap>();
            var bar = new Mock<IBootstrap>();
            var baz = new Mock<IBootstrap>();

            var count = 0;
            foo.Setup((o) => o.Bootstrap()).Callback(() =>
            {
                Assert.AreEqual(0, count++);
            });

            bar.Setup((o) => o.Bootstrap()).Callback(() =>
            {
                Assert.AreEqual(1, count++);
            });

            baz.Setup((o) => o.Bootstrap()).Callback(() =>
            {
                Assert.AreEqual(2, count++);
            });

            application.BootstrapWith(foo.Object, bar.Object, baz.Object);
            Assert.AreEqual(3, count);
        }
    }
}
