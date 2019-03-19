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

using System;
using CatLib.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Core.Tests.Support.Container
{
    [TestClass]
    public class MethodContainerTests
    {
        [TestMethod]
        public void TestBindMethod()
        {
            (new Application()).Bootstrap();
            App.BindMethod("TestMethod", () => 10);
            App.BindMethod("TestMethodContainer", (IContainer container) => container != null);
            App.BindMethod("TestMethodContainer2", (IContainer container, IContainer container2) =>
            {
                Assert.AreSame(container, container2);
                return container != null;
            });
            App.BindMethod("TestMethodInputNum", (IApplication application, IContainer container, int num) => num);
            App.BindMethod("TestMethodInputNum2", (IApplication application, IContainer container, int num, float num2) => num2);

            Assert.AreEqual(10, App.Invoke("TestMethod"));
            Assert.AreEqual(true, App.Invoke("TestMethodContainer"));
            Assert.AreEqual(true, App.Invoke("TestMethodContainer2"));
            Assert.AreEqual(1000, App.Invoke("TestMethodInputNum", 1000, 2000));
            Assert.AreEqual((float) 2000, App.Invoke("TestMethodInputNum2", 1000, 2000, 3000));
        }

        [TestMethod]
        public void UnBindMethodWithMethodName()
        {
            new Application().Bootstrap();
            App.BindMethod("TestMethod10", () => 10);
            App.BindMethod("TestMethod20", () => 20);

            App.UnbindMethod("TestMethod10");

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                App.Invoke("TestMethod10");
            });

            Assert.AreEqual(20, App.Invoke("TestMethod20"));
        }

        public class TestContainerClass
        {
            public int Func1(IContainer container, int input)
            {
                return input;
            }

            public float Func2(IContainer container, int input, float input2)
            {
                return input2;
            }
        }

        [TestMethod]
        public void TestBindExcistsMethod()
        {
            new Application();
            var cls = new TestContainerClass();
            App.BindMethod("Helloworld.Func1", cls);
            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                App.BindMethod("Helloworld.Func1", cls);
            });
        }

        public static object TestStaticMethodAction(int num)
        {
            return num;
        }

        [TestMethod]
        public void TestStaticMethod()
        {
            new Application().Bootstrap();
            App.BindMethod<int>("echo", TestStaticMethodAction);
            Assert.AreEqual(200, App.Invoke("echo", 200));
        }

        [TestMethod]
        public void TestUnbindStaticMethod()
        {
            new Application().Bootstrap();
            var bind = App.BindMethod<int>("echo", TestStaticMethodAction);
            Assert.AreEqual(100, App.Invoke("echo", 100));
            App.UnbindMethod(bind);
            ExceptionAssert.Throws<Exception>(() =>
            {
                App.Invoke("echo", 200);
            });
        }

        [TestMethod]
        public void TestUnbindWithObject()
        {
            new Application().Bootstrap();
            var cls = new TestContainerClass(); 
            App.BindMethod("Helloworld.Func1", cls);
            App.BindMethod("Helloworld.Func2", cls);

            var cls2 = new TestContainerClass();
            App.BindMethod("Helloworld2.Func1", cls2);
            App.BindMethod("Helloworld2.Func2", cls2);

            App.UnbindMethod(cls);
            App.UnbindMethod(cls); // double unbind test
            App.UnbindMethod("UnknowMethod");

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                App.Invoke("Helloworld.Func1", 1000, 2000);
            });

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                App.Invoke("Helloworld.Func2", 1000, 2000);
            });

            Assert.AreEqual(1000, App.Invoke("Helloworld2.Func1", 1000, 2000));
            Assert.AreEqual((float)2000, App.Invoke("Helloworld2.Func2", 1000, 2000));
        }

        [TestMethod]
        public void TestContainerMethodContextual()
        {
            new Application().Bootstrap();

            App.Instance("input", 1000);
            App.Alias("$input", "input");
            App.Instance("input2", 2000);
            App.Alias("$input2", "input2");

            var cls = new TestContainerClass();
            App.BindMethod("Helloworld.Func1", cls).Needs("$input").Given("$input2");
            App.BindMethod("Helloworld.Func2", cls)
                .Needs("$input").Given("$input2")
                .Needs("$input2").Given("$input"); ;

            Assert.AreEqual(2000, App.Invoke("Helloworld.Func1"));
            Assert.AreEqual((float)1000, App.Invoke("Helloworld.Func2"));
        }

        [TestMethod]
        public void TestFlush()
        {
            new Application();
            var cls = new TestContainerClass();
            var bind1 = App.BindMethod("Helloworld.Func1", cls);
            var bind2 = App.BindMethod("Helloworld.Func2", cls);

            App.Handler.Flush();

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                App.Invoke("Helloworld.Func1", 1000, 2000);
            });

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                App.Invoke("Helloworld.Func2", 1000, 2000);
            });

            App.UnbindMethod(cls);
            App.UnbindMethod(cls); // double unbind test
            App.UnbindMethod(bind1);
            App.UnbindMethod(bind2);
        }
    }
}
