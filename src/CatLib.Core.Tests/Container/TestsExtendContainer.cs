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
using CatLib.Support;
using CatLib.Tests.Fixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        }

        [TestMethod]
        public void TestWrap()
        {
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
        [ExpectedExceptionAndMessage(typeof(LogicException),
            "Function \"method-is-not-found\" not found.")]
        public void TestCallIllegal()
        {
            container.Bind<Foo>();
            container.Bind<Bar>();

            var foobar = FooBar.New();

            container.Call(foobar, "method-is-not-found");
        }

        [TestMethod]
        public void TestFactory()
        {
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
            container.Bind("foo", (container, args) => "foo")
                     .Alias<IFoo>();
            Assert.IsTrue(container.HasBind<IFoo>());
        }

        [TestMethod]
        public void TestIsAlias()
        {
            container.Bind("foo", (container, args) => "foo", false)
                     .Alias<IFoo>();

            Assert.IsTrue(container.IsAlias<IFoo>());
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
            var binder = container.Singleton<string>(() => "foo");
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

            ExceptionAssert.Throws<LogicException>(() =>
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

            ExceptionAssert.Throws<LogicException>(() =>
            {
                container.Invoke("Foo1.EchoInt", 100);
            });

            ExceptionAssert.Throws<LogicException>(() =>
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
            App.BindMethod("Foo.EchoInt", foo).Needs("$input").Given(() => 200);
            Assert.AreEqual(100, App.Invoke("Foo.EchoInt", 100));
            Assert.AreEqual(200, App.Invoke("Foo.EchoInt"));
        }
    }
}
