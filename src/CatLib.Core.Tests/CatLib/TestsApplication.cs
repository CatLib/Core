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
        public void TestBootstrapRepeat()
        {
            application.Bootstrap();
            application.Init();
            application.Bootstrap();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestInitRepeat()
        {
            application.Bootstrap();
            application.Init();
            application.Init();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestInitNoBootstrap()
        {
            application.Init();
        }

        [TestMethod]
        public void TestBootstrapSkip()
        {
            var foo = new Mock<IBootstrap>();
            var bar = new Mock<IBootstrap>();

            dispatcher.AddListener<BootingEventArgs>(args =>
            {
                if (args.GetBootstrap() == foo.Object)
                {
                    args.Skip();
                }
            });

            application.Bootstrap(foo.Object, bar.Object);

            foo.Verify((o) => o.Bootstrap(), Times.Never);
            bar.Verify((o) => o.Bootstrap(), Times.Once);
        }

        [TestMethod]
        public void TestRegisterSkip()
        {
            var foo = new Mock<IServiceProvider>();
            var bar = new Mock<IServiceProvider>();

            dispatcher.AddListener<RegisterProviderEventArgs>((args) =>
            {
                if (args.GetServiceProvider() == foo.Object)
                {
                    args.Skip();
                }
            });

            application.Register(foo.Object);
            application.Register(bar.Object);

            foo.Verify((o) => o.Register(), Times.Never);
            bar.Verify((o) => o.Register(), Times.Once);
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
        [ExpectedExceptionAndMessage(typeof(LogicException))]
        public void TestInitingRegister()
        {
            var foo = new Mock<IServiceProvider>();
            var bar = new Mock<IServiceProvider>();

            dispatcher.AddListener<InitProviderEventArgs>((args) =>
            {
                application.Register(foo.Object);
            });

            application.Register(bar.Object);
            application.Bootstrap();
            application.Init();
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(LogicException))]
        public void TestTerminateRegister()
        {
            var foo = new Mock<IServiceProvider>();

            dispatcher.AddListener<BeforeTerminateEventArgs>((args) =>
            {
                application.Register(foo.Object);
            });

            application.Bootstrap();
            application.Init();
            application.Terminate();
        }

        [TestMethod]
        public void TestTerminateSequenceOfEvents()
        {
            var count = 0;
            dispatcher.AddListener<BeforeTerminateEventArgs>((args) =>
            {
                Assert.AreEqual(0, count++);
            });

            dispatcher.AddListener<AfterTerminateEventArgs>((args) =>
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

            application.Bootstrap();
            application.Register(foo.Object);
            application.Init();
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
        public void TestBoostrapRepeat()
        {
            var foo = new Mock<IBootstrap>().Object;
            application.Bootstrap(foo, foo);
        }

        [TestMethod]
        public void TestBoostrapOrder()
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

            application.Bootstrap(foo.Object, bar.Object, baz.Object);
            Assert.AreEqual(3, count);
        }
    }
}
