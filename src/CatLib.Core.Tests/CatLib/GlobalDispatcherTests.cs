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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests
{
    [TestClass]
    public class GlobalDispatcherTests
    {
        [TestMethod]
        public void TestOn()
        {
            Application.New();

            var n = string.Empty;
            var a = new object[]{};
            var r = new object();
            App.Listen("Events.On", (name, args) =>
            {
                n = name;
                a = args;
                return r;
            });

            Assert.AreEqual(r, App.TriggerHalt("Events.On","helloworld", "catlib"));
            Assert.AreEqual("Events.On", n);
            Assert.AreEqual("helloworld", a[0]);
            Assert.AreEqual("catlib", a[1]);
        }

        [TestMethod]
        public void TestDispatcherGroup()
        {
            Application.New();
            var isCall = false;
            App.On("Events", () =>
            {
                isCall = true;
            }, "catlib");

            App.On("Events2", () =>
            {
                isCall = true;
            }, "catlib");

            App.Trigger("Events");
            Assert.AreEqual(true, isCall);
            isCall = false;
            App.Trigger("Events2");
            Assert.AreEqual(true, isCall);
            isCall = false;

            App.Off("catlib");
            App.Trigger("Events");
            App.Trigger("Events2");
            Assert.AreEqual(false, isCall);
        }

        [TestMethod]
        public void TestT1()
        {
            var dispatcher = new Dispatcher();
            Application.New().Bootstrap();
            var n = 0;
            dispatcher.On("Events.T1", (int num) =>
            {
                n = num;
            });

            App.On("Events.T1",(int num) =>
            {
                n = num + 1;
            });

            dispatcher.Listen("Events.T1.Listen", (int num) =>
            {
                n = num;
                return 100;
            });

            App.Listen("Events.T1.Listen", (int num) =>
            {
                n = num + 1;
                return 200;
            });

            var isCall = false;
            dispatcher.On("Events.T0", () =>
            {
                isCall = true;
            });

            dispatcher.Listen("Events.T0.Listen", () =>
            {
                return 300;
            });

            App.Listen("Events.T0.Listen", () =>
            {
                return 300;
            });

            dispatcher.Trigger("Events.T1",199478);
            Assert.AreEqual(199478, n);
            App.Trigger("Events.T1", 199478);
            Assert.AreEqual(199479, n);

            Assert.AreEqual(100, dispatcher.TriggerHalt("Events.T1.Listen", "abc", App.Handler, 199478));
            Assert.AreEqual(199478, n);
            Assert.AreEqual(200, App.TriggerHalt("Events.T1.Listen", "abcd", dispatcher, 199478));
            Assert.AreEqual(199479, n);

            dispatcher.TriggerHalt("Events.T0", "abcd", dispatcher, 199478);
            Assert.AreEqual(true, isCall);
            Assert.AreEqual(300, dispatcher.TriggerHalt("Events.T0.Listen", "abcd", dispatcher, 199478));
            Assert.AreEqual(300, App.TriggerHalt("Events.T0.Listen", "abcd", dispatcher, 199478));
        }

        [TestMethod]
        public void TestT1_2()
        {
            var dispatcher = new Dispatcher();
            Application.New().Bootstrap();
            var n = 0;
            var s = string.Empty;
            dispatcher.On("Events.T1_2", (int num, string hello) =>
            {
                n = num;
                s = hello;
            });

            App.On("Events.T1_2", (int num, string hello) =>
            {
                n = num + 1;
                s = hello;
            });

            dispatcher.Listen("Events.T1_2.Listen", (int num, string hello) =>
            {
                n = num;
                s = hello;
                return 100;
            });

            App.Listen("Events.T1_2.Listen", (int num, string hello) =>
            {
                n = num + 1;
                s = hello;
                return 200;
            });

            dispatcher.Trigger("Events.T1_2", "abc",199478);
            Assert.AreEqual(199478, n);
            Assert.AreEqual("abc", s);
            App.Trigger("Events.T1_2", "abcd", 199478);
            Assert.AreEqual(199479, n);
            Assert.AreEqual("abcd", s);

            Assert.AreEqual(100, dispatcher.TriggerHalt("Events.T1_2.Listen", "abc", App.Handler, 199478));
            Assert.AreEqual(199478, n);
            Assert.AreEqual("abc", s);
            Assert.AreEqual(200, App.TriggerHalt("Events.T1_2.Listen", "abcd", dispatcher, 199478));
            Assert.AreEqual(199479, n);
            Assert.AreEqual("abcd", s);
        }

        [TestMethod]
        public void Test1_3()
        {
            var dispatcher = new Dispatcher();
            Application.New().Bootstrap();
            var n = 0;
            var s = string.Empty;
            dispatcher.On("Events.T1_3", (int num, string hello, IDispatcher disp) =>
            {
                n = num;
                s = hello;
                Assert.AreEqual(App.Handler, disp);
            });

            App.On("Events.T1_3", (int num, string hello, IDispatcher disp) =>
            {
                n = num + 1;
                s = hello;
                Assert.AreEqual(dispatcher, disp);
            });

            dispatcher.Listen("Events.T1_3.Listen", (int num, string hello, IDispatcher disp) =>
            {
                n = num;
                s = hello;
                Assert.AreEqual(App.Handler, disp);
                return 100;
            });

            App.Listen("Events.T1_3.Listen", (int num, string hello, IDispatcher disp) =>
            {
                n = num + 1;
                s = hello;
                Assert.AreEqual(dispatcher, disp);
                return 200;
            });

            dispatcher.Trigger("Events.T1_3", "abc", App.Handler, 199478);
            Assert.AreEqual(199478, n);
            Assert.AreEqual("abc", s);
            App.Trigger("Events.T1_3", "abcd", dispatcher, 199478);
            Assert.AreEqual(199479, n);
            Assert.AreEqual("abcd", s);

            Assert.AreEqual(100, dispatcher.TriggerHalt("Events.T1_3.Listen", "abc", App.Handler, 199478));
            Assert.AreEqual(199478, n);
            Assert.AreEqual("abc", s);
            Assert.AreEqual(200, App.TriggerHalt("Events.T1_3.Listen", "abcd", dispatcher, 199478));
            Assert.AreEqual(199479, n);
            Assert.AreEqual("abcd", s);
        }

        [TestMethod]
        public void Test1_4()
        {
            var dispatcher = new Dispatcher();
            Application.New().Bootstrap();
            var n = 0;
            var s = string.Empty;
            dispatcher.On("Events.T1_4", (int num, string hello, IDispatcher disp, IApplication application) =>
            {
                n = num;
                s = hello;
                Assert.AreEqual(App.Handler, disp);
                Assert.AreEqual(App.Handler, application);
            });

            App.On("Events.T1_4", (int num, string hello, IDispatcher disp, IApplication application) =>
            {
                n = num + 1;
                s = hello;
                Assert.AreEqual(dispatcher, disp);
                Assert.AreEqual(App.Handler, application);
            });

            dispatcher.Listen("Events.T1_4.Listen", (int num, string hello, IDispatcher disp, IApplication application) =>
            {
                n = num;
                s = hello;
                Assert.AreEqual(App.Handler, disp);
                Assert.AreEqual(App.Handler, application);
                return 100;
            });

            App.Listen("Events.T1_4.Listen", (int num, string hello, IDispatcher disp, IApplication application) =>
            {
                n = num + 1;
                s = hello;
                Assert.AreEqual(dispatcher, disp);
                Assert.AreEqual(App.Handler, application);
                return 200;
            });

            dispatcher.Trigger("Events.T1_4", "abc", App.Handler, 199478);
            Assert.AreEqual(199478, n);
            Assert.AreEqual("abc", s);
            App.Trigger("Events.T1_4", "abcd", dispatcher, 199478);
            Assert.AreEqual(199479, n);
            Assert.AreEqual("abcd", s);

            Assert.AreEqual(100, dispatcher.TriggerHalt("Events.T1_4.Listen", "abc", App.Handler, 199478));
            Assert.AreEqual(199478, n);
            Assert.AreEqual("abc", s);
            Assert.AreEqual(200, App.TriggerHalt("Events.T1_4.Listen", "abcd", dispatcher, 199478));
            Assert.AreEqual(199479, n);
            Assert.AreEqual("abcd", s);
        }
    }
}
