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

namespace CatLib.Tests.Events
{
    [TestClass]
    public class DispatcherTests
    {
        /// <summary>
        /// 生成测试环境
        /// </summary>
        /// <returns></returns>
        private IContainer MakeEnv()
        {
            var app = new Application();

            app.Bootstrap();
            app.Init();

            return app;
        }

        [TestMethod]
        public void TestSimpleOnEvents()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            var isCall = false;
            dispatcher.On("event.name", (object payload) =>
            {
                isCall = true;
                Assert.AreEqual(123, payload);
            });

            Assert.AreEqual(null, (dispatcher.Trigger("event.name", 123) as object[])[0]);
            Assert.AreEqual(true, isCall);
        }

        public void SimpleCallFunctionVoid(object payload)
        {
            
        }

        [TestMethod]
        public void TestTriggerReturnResult()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            var isCall = false;
            dispatcher.Listen("event.name", (object payload) =>
            {
                isCall = true;
                Assert.AreEqual(123, payload);
                return 1;
            });
            dispatcher.Listen("event.name", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 2;
            });

            var result = dispatcher.Trigger("event.name", 123) as object[];

            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
            Assert.AreEqual(true, isCall);
        }

        [TestMethod]
        public void TestAsteriskWildcard()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            var isCall = false;
            dispatcher.Listen("event.name", (object payload) =>
            {
                isCall = true;
                Assert.AreEqual(123, payload);
                return 1;
            });
            dispatcher.Listen("event.name", ( object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 2;
            });
            dispatcher.Listen("event.age", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 2;
            });
            dispatcher.Listen("event.*", (string eventName, object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 3;
            });

            var result = dispatcher.Trigger("event.name", 123) as object[];
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
            Assert.AreEqual(3, result[2]);
            Assert.AreEqual(true, isCall);

            result = dispatcher.Trigger("event.age", 123) as object[];
            Assert.AreEqual(2, result[0]);
            Assert.AreEqual(3, result[1]);
        }

        [TestMethod]
        public void TestHalfTrigger()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            var isCall = false;
            dispatcher.Listen("event.name", (object payload) =>
            {
                isCall = true;
                Assert.AreEqual(123, payload);
                return 1;
            });
            dispatcher.Listen("event.name", (object payload) =>
            {
                isCall = true;
                Assert.AreEqual(123, payload);
                return 2;
            });
            dispatcher.Listen("event.*", (string eventName, object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 3;
            });

            Assert.AreEqual(1, dispatcher.TriggerHalt("event.name", 123));
            Assert.AreEqual(true, isCall);
        }

        [TestMethod]
        public void TestCancelHandler()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            var isCall = false;
            var handler = dispatcher.Listen("event.name", (object payload) =>
            {
                isCall = true;
                Assert.AreEqual(123, payload);
                return 1;
            });
            dispatcher.Listen("event.name", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 2;
            });
            dispatcher.Listen("event.*", (string eventName, object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 3;
            });

            App.Off(handler);

            Assert.AreEqual(2, dispatcher.TriggerHalt("event.name", 123));
            Assert.AreEqual(false, isCall);
        }

        [TestMethod]
        public void TestStopBubbling()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            dispatcher.Listen("event.*", (string eventName, object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 1;
            });
            dispatcher.Listen("event.time", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 2;
            });
            dispatcher.Listen("event.time", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 3;
            });

            var results = dispatcher.Trigger("event.time", 123);

            Assert.AreEqual(3, results.Length);
            Assert.AreEqual(2, results[0]);
            Assert.AreEqual(3, results[1]);
            Assert.AreEqual(1, results[2]);
        }

        [TestMethod]
        public void TestHaltNull()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            dispatcher.Listen("event.*", (string eventName, object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 1;
            });

            dispatcher.Listen("event.time", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return null;
            });

            Assert.AreEqual(1, dispatcher.TriggerHalt("event.time", 123));
        }
    }
}
