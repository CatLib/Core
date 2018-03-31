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

        private int TestCall()
        {
            return 199478;
        }

        [TestMethod]
        public void TestProtectedFunction()
        {
            var app = MakeEnv();
            var dispatcher = app.Make<IDispatcher>();
            dispatcher.On("event.name", this, "TestCall");
            Assert.AreEqual(199478, dispatcher.TriggerHalt("event.name"));
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
        public void TestCancelHandlerString()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            var isCall = false;
            dispatcher.Listen("event.name", (object payload) =>
            {
                isCall = true;
                Assert.AreEqual(123, payload);
                return 1;
            }, "1");
            dispatcher.Listen("event.name", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 2;
            }, "1");
            dispatcher.Listen("event.*", (string eventName, object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 3;
            }, "1");

            App.Off("1");

            Assert.AreEqual(null, dispatcher.TriggerHalt("event.name", 123));
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

            dispatcher.Listen<object, object>("event.time", (payload) =>
            {
                Assert.AreEqual(123, payload);
                return null;
            });

            Assert.AreEqual(1, dispatcher.TriggerHalt("event.time", 123));
        }

        [TestMethod]
        public void TestBreakEvent()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            dispatcher.Listen("event.time", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 1;
            });

            dispatcher.Listen("event.time", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return false;
            });

            dispatcher.Listen("event.time", (object payload) =>
            {
                Assert.AreEqual(123, payload);
                return 2;
            });

            var result = dispatcher.Trigger("event.time", 123);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(1, result[0]);
        }

        [TestMethod]
        public void TestAllHaltNull()
        {
            var app = MakeEnv();

            var dispatcher = app.Make<IDispatcher>();
            dispatcher.Listen<object, object>("event.time", (payload) =>
            {
                return null;
            });

            dispatcher.Listen<object, object>("event.time", (payload) =>
            {
                return null;
            });

            Assert.AreEqual(null, dispatcher.TriggerHalt("event.time"));
        }

        [TestMethod]
        public void TestOffWithEventName()
        {
            var app = MakeEnv();
            var dispatcher = app.Make<IDispatcher>();
            var handler = dispatcher.Listen("event.time", (object payload) =>
            {
                return 1;
            });

            dispatcher.Listen("event.time", (object payload) =>
            {
                return 1;
            });

            var handler2 = dispatcher.Listen("event.*", (object payload) =>
            {
                return 1;
            });

            App.Off("event.time");
            App.Off("event.time"); // double remove
            App.Off("event.*");
            App.Off("event.*.Empty");
            App.Off(handler);
            App.Off(handler2);

            Assert.AreEqual(0, dispatcher.Trigger("event.time").Length);
        }

        public class TestEventClass
        {
            public object TestFuncHello()
            {
                return "hello";
            }

            public object TestFuncWorld(IContainer container)
            {
                Assert.AreNotEqual(null, container);
                return "world";
            }
        }

        [TestMethod]
        public void TestOffWithObject()
        {
            var app = MakeEnv();
            var dispatcher = app.Make<IDispatcher>();

            var cls = new TestEventClass();
            dispatcher.On("MyTestEventClass.TestFuncHello", cls);
            dispatcher.On("MyTestEventClass.*", cls, "TestFuncWorld");

            Assert.AreEqual("hello", dispatcher.TriggerHalt("MyTestEventClass.TestFuncHello"));
            Assert.AreEqual("world", dispatcher.TriggerHalt("MyTestEventClass.Jump"));

            App.Off(cls);
            App.Off(cls); // double off

            Assert.AreEqual(null, dispatcher.TriggerHalt("MyTestEventClass.TestFuncHello"));
            Assert.AreEqual(null, dispatcher.TriggerHalt("MyTestEventClass.Jump"));
        }

        [TestMethod]
        public void TestHasListeners()
        {
            var app = MakeEnv();
            var dispatcher = app.Make<IDispatcher>();

            var cls = new TestEventClass();
            dispatcher.On("MyTestEventClass.TestFuncHello", cls);
            dispatcher.On("MyTestEventClass.*", cls, "TestFuncWorld");
            var isCall = false;
            dispatcher.On("MyTestEventClass.*", (string name) =>
            {
                isCall = true;
                Assert.AreEqual("MyTestEventClass.Jump.Hack", name);
            });

            Assert.AreEqual(false, App.HasListeners("Null"));
            Assert.AreEqual(true, App.HasListeners("MyTestEventClass.TestFuncHello"));
            Assert.AreEqual(true, App.HasListeners("MyTestEventClass.Jump.Hack"));
            Assert.AreEqual(false, App.HasListeners("MyTestEventClass.Jump.Hack", true));
            App.Trigger("MyTestEventClass.Jump.Hack");
            Assert.AreEqual(true, isCall);
        }

        public static int StaticCall()
        {
            return 100;
        }

        [TestMethod]
        public void TestStatic()
        {
            var app = MakeEnv();
            var dispatcher = app.Make<IDispatcher>();

            App.Listen("ActionTest", StaticCall);
            Assert.AreEqual(100, App.TriggerHalt("ActionTest"));
        }

        [TestMethod]
        public void TestActionFuncCover()
        {
            var app = MakeEnv();
            var dispatcher = app.Make<IDispatcher>();

            App.On("ActionTest", () =>
            {
            });

            App.On("ActionTest", (IContainer container) =>
            {
                Assert.AreNotEqual(null, container);
            });

            App.On("ActionTest", (IContainer container, object data) =>
            {
                Assert.AreEqual(typeof(string), data.GetType());
                Assert.AreNotEqual(null, container);
            });

            App.On("ActionTest2", (int num, int num2 ,int num3) =>
            {
                Assert.AreEqual(1, num);
                Assert.AreEqual(2, num2);
                Assert.AreEqual(3, num3);
            });

            App.On("ActionTest2", (int num, int num2, int num3, int num4) =>
            {
                Assert.AreEqual(1, num);
                Assert.AreEqual(2, num2);
                Assert.AreEqual(3, num3);
                Assert.AreEqual(4, num4);
            });

            App.Trigger("ActionTest", "helloworld");
            App.Trigger("ActionTest2", 1, 2, 3, 4);

            App.Listen("FuncTest", () =>
            {
                return 0;
            });

            App.Listen("FuncTest", (IContainer container) =>
            {
                Assert.AreNotEqual(null, container);
                return 1;
            });

            App.Listen("FuncTest", (IContainer container, object data) =>
            {
                Assert.AreEqual(typeof(string), data.GetType());
                Assert.AreNotEqual(null, container);
                return 2;
            });

            App.Listen("FuncTest2", (int num, int num2, int num3) =>
            {
                Assert.AreEqual(1, num);
                Assert.AreEqual(2, num2);
                Assert.AreEqual(3, num3);
                return 3;
            });

            App.Listen("FuncTest2", (int num, int num2, int num3, int num4) =>
            {
                Assert.AreEqual(1, num);
                Assert.AreEqual(2, num2);
                Assert.AreEqual(3, num3);
                Assert.AreEqual(4, num4);
                return 4;
            });

            Assert.AreEqual(3, App.Trigger("FuncTest", "helloworld").Length);
            Assert.AreEqual(2, App.Trigger("FuncTest2", 1, 2, 3, 4).Length);
        }
    }
}
