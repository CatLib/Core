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
using CatLib.Exception;
using CatLib.Tests.Fixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CContainer = CatLib.Container.Container;

namespace CatLib.Tests.Container
{
    [TestClass]
    public class TestsExtendContainer
    {
        private IContainer container;

        [TestInitialize]
        public void Initialize()
        {
            container = new CContainer();
            container.Singleton<IContainer>(() => container);
            container.Bind<IFoo, Foo>()
                .Alias<Foo>()
                .Alias("foo-alias");
        }

        [TestMethod]
        public void TestMake()
        {
            Assert.AreSame(container, container.Make<IContainer>());
            Assert.AreNotEqual(null, container.Make<Foo>("foo-alias"));

            var bar = container.Make(typeof(Bar));
            Assert.AreNotEqual(null, bar);
        }

        [TestMethod]
        public void TestInstance()
        {
            container.Instance<Bar>(new Bar());

            var bar = container.Make<Bar>();
            Assert.AreNotEqual(null, bar);
        }

        [TestMethod]
        public void TestRelease()
        {
            container.Release<IContainer>();
            Assert.IsFalse(container.HasInstance<IContainer>());
        }

        [TestMethod]
        public void TestAlias()
        {
            container.Alias<CContainer, IContainer>();
            Assert.IsTrue(container.IsAlias<CContainer>());
        }

        [TestMethod]
        public void TestReleaseWithObject()
        {
            container.Instance<string>("foo");
            container.Instance<int>(10);
            object[] data = null;
            Assert.AreEqual(true, container.Release(ref data));

            data = Array.Empty<object>();
            Assert.AreEqual(true, container.Release(ref data));

            data = new object[] { "foo", 10 };
            Assert.AreEqual(true, container.Release(ref data));
            Assert.AreEqual(true, data.Length == 0);

            data = new object[] { "foo", 10, 100 };
            Assert.AreEqual(false, container.Release(ref data));
            Assert.AreEqual(true, data.Length == 3);

            container.Instance<int>(10);
            data = new object[] { "foo", 10, 100 };
            Assert.AreEqual(false, container.Release(ref data, false));
            Assert.AreEqual(true, data.Length == 2);
            CollectionAssert.AreEqual(new object[] { "foo", 100 }, data);

            container.Instance<string>("foo");
            data = new object[] { 10, "foo", 100 };
            Assert.AreEqual(false, container.Release(ref data));
            Assert.AreEqual(true, data.Length == 2);
            CollectionAssert.AreEqual(new object[] { 10, 100 }, data);
        }

        [TestMethod]
        public void TestBindIf()
        {
            Assert.IsFalse(container.BindIf("foo-alias", (container, args) => new Foo(), true, out _));
            Assert.IsFalse(container.BindIf<IFoo, Foo>(out _));
            Assert.IsTrue(container.BindIf<Bar>(out _));
        }

        [TestMethod]
        public void TestSingletonIf()
        {
            Assert.IsFalse(container.SingletonIf("foo-alias", (container, args) => new Foo(), out _));
            Assert.IsFalse(container.SingletonIf<IFoo, Foo>(out _));
            Assert.IsTrue(container.SingletonIf<Bar>(out _));
        }

        [TestMethod]
        public void TestGetBind()
        {
            Assert.AreNotEqual(null, container.GetBind<IFoo>());
        }

        [TestMethod]
        public void TestCanMake()
        {
            Assert.IsTrue(container.CanMake<IContainer>());
            Assert.IsFalse(container.CanMake<int>());
        }

        [TestMethod]
        public void TestIsStatic()
        {
            Assert.IsTrue(container.IsStatic<IContainer>());
            Assert.IsFalse(container.IsStatic<IFoo>());
        }

        [TestMethod]
        public void TestIsAlias()
        {
            Assert.IsFalse(container.IsAlias<IFoo>());
            Assert.IsTrue(container.IsAlias<Foo>());
            Assert.IsFalse(container.IsAlias<IContainer>());
        }

        [TestMethod]
        public void TestWrap()
        {
            container = new CContainer();
            container.Singleton<IFoo, Foo>().Alias<Foo>();
            var expected = container.Make<IFoo>();
            var count = 0;

            var wrapped = container.Wrap((IFoo foo) =>
            {
                Assert.AreSame(expected, foo);
                count++;
            });
            wrapped();

            wrapped = container.Wrap((IFoo foo1, Foo foo2) =>
            {
                Assert.AreSame(expected, foo1);
                Assert.AreSame(foo1, foo2);
                count++;
            });
            wrapped();

            wrapped = container.Wrap((IFoo foo1, Foo foo2, IFoo foo3) =>
            {
                Assert.AreSame(expected, foo1);
                Assert.AreSame(foo1, foo2);
                Assert.AreSame(foo2, foo3);
                count++;
            });
            wrapped();

            wrapped = container.Wrap((IFoo foo1, IFoo foo2, IFoo foo3, Foo foo4) =>
            {
                Assert.AreSame(expected, foo1);
                Assert.AreSame(foo1, foo2);
                Assert.AreSame(foo2, foo3);
                Assert.AreSame(foo3, foo4);
                count++;
            });
            wrapped();

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void TestCall()
        {
            container = new CContainer();
            container.Singleton<IFoo, Foo>().Alias<Foo>();
            var expected = container.Make<IFoo>();
            var count = 0;

            container.Call((IFoo foo) =>
            {
                Assert.AreSame(expected, foo);
                count++;
            });

            container.Call((IFoo foo1, Foo foo2) =>
            {
                Assert.AreSame(expected, foo1);
                Assert.AreSame(foo1, foo2);
                count++;
            });

            container.Call((IFoo foo1, Foo foo2, IFoo foo3) =>
            {
                Assert.AreSame(expected, foo1);
                Assert.AreSame(foo1, foo2);
                Assert.AreSame(foo2, foo3);
                count++;
            });

            container.Call((IFoo foo1, IFoo foo2, IFoo foo3, Foo foo4) =>
            {
                Assert.AreSame(expected, foo1);
                Assert.AreSame(foo1, foo2);
                Assert.AreSame(foo2, foo3);
                Assert.AreSame(foo3, foo4);
                count++;
            });

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void TestOnResolving()
        {
            var count = 0;
            container.OnResolving<IFoo>((foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.OnResolving<Foo>((binder, foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.OnResolving<IContainer>((binder, foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.Make<IFoo>();

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void TestOnAfterResolving()
        {
            var count = 0;
            container.OnAfterResolving<IFoo>((foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.OnAfterResolving<Foo>((binder, foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.OnAfterResolving<IContainer>((binder, foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.Make<IFoo>();

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void TestOnRelease()
        {
            container.Unbind<IFoo>();
            container.Singleton<IFoo, Foo>();

            var count = 0;
            container.OnRelease<IFoo>((foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.OnRelease<Foo>((binder, foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.OnRelease<IContainer>((binder, foo) =>
            {
                Assert.AreNotEqual(null, foo);
                count++;
            });

            container.Make<IFoo>();
            Assert.AreEqual(0, count);

            container.Release<IFoo>();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(
            typeof(LogicException),
            "Function \"method-is-not-found\" not found.")]
        public void TestCallIllegal()
        {
            var foobar = FooBar.New();
            container.Call(foobar, "method-is-not-found");
        }

        [TestMethod]
        public void TestFactory()
        {
            container.Flush();
            container.Instance<IFoo>(new Foo());
            container.Instance("bar", "bar");

            var factory1 = container.Factory<IFoo>(100);
            var expected = container.Make<IFoo>();
            Assert.AreEqual(expected, factory1());

            var factory2 = container.Factory("bar", 200);
            Assert.AreEqual("bar", factory2());
        }

        [TestMethod]
        public void TestHasBind()
        {
            Assert.IsTrue(container.HasBind<IFoo>());
        }

        [TestMethod]
        public void TestWatch()
        {
            var binder = container.Singleton<string>(() => "foo");
            container.Make<string>();
            binder.Unbind();

            var actual = string.Empty;
            container.Watch<string>((instance) =>
            {
                actual = instance;
            });

            container.Singleton<string>(() => "bar");

            Assert.AreEqual("bar", actual);
        }

        [TestMethod]
        public void TestWatchInstanceOverride()
        {
            container.Singleton<string>(() => "foo");
            container.Make<string>();

            var actual1 = string.Empty;
            container.Watch<string>((instance) =>
            {
                actual1 = instance;
            });

            var actual2 = false;
            container.Watch<string>(() =>
            {
                actual2 = true;
            });

            container.Instance(
                container.Type2Service(typeof(string)),
                "bar");

            Assert.AreEqual("bar", actual1);
            Assert.IsTrue(actual2);
        }

        [TestMethod]
        public void TestReleaseWithTag()
        {
            container.Bind("foo", typeof(Foo), true)
                    .Tag("foobar");
            container.Bind("bar", typeof(Bar), true)
                    .Tag("foobar");

            var instances = container.Tagged("foobar");
            Assert.AreEqual(true, container.Release(ref instances));
        }

        [TestMethod]
        public void TestExtend()
        {
            container.Bind<string>(() => "foo");
            container.Bind<object>(() => "baz");
            container.Extend<string>((instance) => instance + "bar");

            Assert.AreEqual("foobar", container.Make<string>());
            Assert.AreEqual("bazbar", container.Make<object>());
        }

        [TestMethod]
        public void TestBindMethod()
        {
            container.BindMethod("foo", () => "foo");
            Assert.AreEqual("foo", container.Invoke("foo"));

            container.BindMethod("bar", (IContainer container)
                => container != null);
            Assert.IsTrue((bool)container.Invoke("bar"));

            container.BindMethod("echo", (string input) =>
            {
                return input;
            });
            Assert.AreEqual("foobar", container.Invoke("echo", "foobar"));
        }

        [TestMethod]
        public void TestUnbindMethodWithMethodName()
        {
            container.BindMethod("foo", () => "foo");
            container.BindMethod("bar", () => "bar");

            container.UnbindMethod("foo");

            Assert.ThrowsException<LogicException>(() =>
            {
                container.Invoke("foo");
            });

            Assert.AreEqual("bar", container.Invoke("bar"));
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestBindMethodExists()
        {
            container.BindMethod("foo", () => "foo");
            container.BindMethod("foo", () => "bar");
        }

        [TestMethod]
        public void TestBindMethodStatic()
        {
            container.BindMethod<string>("echo", Foo.Echo);
            Assert.AreEqual("foo", container.Invoke("echo", "foo"));
        }

        [TestMethod]
        public void TestUnbindWithObject()
        {
            var foo1 = new Foo();
            container.BindMethod("Foo1.EchoInt", foo1);
            container.BindMethod("Foo1.EchoFloat", foo1);

            var foo2 = new Foo();
            container.BindMethod("Foo2.EchoInt", foo2);
            container.BindMethod("Foo2.EchoFloat", foo2);

            container.UnbindMethod(foo1);
            container.UnbindMethod("unknow-method");

            Assert.ThrowsException<LogicException>(() =>
            {
                container.Invoke("Foo1.EchoInt", 100);
            });

            Assert.ThrowsException<LogicException>(() =>
            {
                container.Invoke("Foo1.EchoFloat", 0.5f);
            });

            Assert.AreEqual(100, container.Invoke("Foo2.EchoInt", 100));
            Assert.AreEqual(0.5f, container.Invoke("Foo2.EchoFloat", 0.5f));
        }

        [TestMethod]
        public void TestContainerMethodContextual()
        {
            var foo = new Foo();
            container.BindMethod("Foo.EchoInt", foo).Needs("$input").Given(() => 200);
            Assert.AreEqual(100, container.Invoke("Foo.EchoInt", 100));
            Assert.AreEqual(200, container.Invoke("Foo.EchoInt"));
        }
    }
}
