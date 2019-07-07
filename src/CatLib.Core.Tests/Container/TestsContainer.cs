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

using CatLib.Support;
using CatLib.Tests;
using CatLib.Tests.Fixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#pragma warning disable CA1034
#pragma warning disable CA1031
#pragma warning disable CA1051

namespace CatLib.Container.Tests
{
    [TestClass]
    public class TestsContainer
    {
        private IContainer container;

        [TestInitialize]
        public void Initialize()
        {
            container = CreateContainer();

            container.OnFindType((service) =>
            {
                return Type.GetType(service);
            });
        }

        [TestMethod]
        public void TestTag()
        {
            var foo = new Foo();
            container["baz"] = 100;
            container["bar"] = 200;

            var service = container.Type2Service(typeof(Foo));
            container.Bind(service, (container, args) => foo, false);

            container.Tag("tag", service);
            container.Tag("tag", "baz", "bar");

            CollectionAssert.AreEqual(new object [] { foo, 100, 200 },
                container.Tagged("tag"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestTagIllegal()
        {
            container.Tag(string.Empty);
            container.Tag(null);
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(LogicException),
            "Tag \"foo\" is not exist.")]
        public void TestTagNotExists()
        {
            container.Tagged("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestUnbind()
        {
            container.Bind("foo", (app, args) => "foo", false);
            container.Unbind("foo");
            container.Make("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestUnbindWithAlias()
        {
            container.Bind("bar", (app, args) => "bar", false).Alias("baz");
            container.Unbind("baz");
            container.Make("baz");
        }

        [TestMethod]
        [DataRow(typeof(string[]))]
        [DataRow(typeof(AbstractClass))]
        [DataRow(typeof(IFoo))]
        [ExpectedExceptionAndMessage(typeof(LogicException), "can not bind.")]
        public void TestUnableType(Type type)
        {
            container.Bind("foo", type, false);
        }

        [TestMethod]
        public void TestBindIf()
        {
            var foo = container.BindIf("foo", (container, args) => "foo", true, out IBindData bindFoo);
            var bar = container.BindIf("foo", (container, args) => "bar", true, out IBindData bindBar);

            Assert.AreSame(bindFoo, bindBar);
            Assert.AreEqual(true, foo);
            Assert.AreEqual(false, bar);
        }

        [TestMethod]
        public void TestbBindIfWithType()
        {
            var foo = container.BindIf("foo", typeof(Foo), true, out IBindData bindFoo);
            var bar = container.BindIf("foo", typeof(Bar), true, out IBindData bindBar);

            Assert.AreSame(bindFoo, bindBar);
            Assert.AreEqual(true, foo);
            Assert.AreEqual(false, bar);
        }

        [TestMethod]
        public void TestBindStatic()
        {
            container.Bind("foo", (container, args) => new Foo(), true);

            var fooBind = container.GetBind("foo");
            var foo = container.Make("foo");

            Assert.AreNotEqual(null, fooBind);
            Assert.AreEqual(true, fooBind.IsStatic);
            Assert.AreSame(foo, container.Make("foo"));
        }

        [TestMethod]
        public void TestBindNonStatic()
        {
            container.Bind("foo", (container, args) => new Foo(), false);
            var older = container.Make("foo");
            var newer = container.Make("foo");

            Assert.AreNotSame(older, newer);
        }

        [TestMethod]
        public void TestGetBind()
        {
            var fooBind = container.Bind("foo", (container, args) => "foo", false);
            var getBind = container.GetBind("foo");
            Assert.AreSame(fooBind, getBind);

            getBind = container.GetBind("non-service");
            Assert.AreEqual(null, getBind);

            container.Alias("foo-alias", "foo");
            getBind = container.GetBind("foo-alias");
            Assert.AreEqual(fooBind, getBind);

            getBind = container.GetBind(null);
            Assert.AreEqual(null, getBind);
        }

        [TestMethod]
        public void TestBindIllegal()
        {
            container.Bind("foo", (container, args) => "foo", true);
            ExceptionAssert.Throws<LogicException>(() =>
            {
                container.Bind("foo", (container, args) => "repeat bind", false);
            });

            container.Instance("bar", "bar");
            ExceptionAssert.Throws<LogicException>(() =>
            {
                container.Bind("bar", (container, args) => "instance repeat bind", false);
            });

            container.Alias("foo-alias", "foo");
            ExceptionAssert.Throws<LogicException>(() =>
            {
                container.Bind("foo-alias", (container, args) => "alias repeat bind", false);
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Bind(null, (container, args) => "invalid service name", false);
            });

            ExceptionAssert.Throws<CodeStandardException>(() =>
            {
                container.Bind("$foo", (container, args) => "Illegal placeholder", false);
            });
           
        }

        [TestMethod]
        public void TestHasBind()
        {
            container.Bind("foo", (container, args) => "foo", false)
                     .Alias("foo-alias");

            Assert.IsTrue(container.HasBind("foo"));
            Assert.IsTrue(container.HasBind("foo-alias"));
        }

        [TestMethod]
        public void TestIsStatic()
        {
            container.Bind("foo", (container, args) => "foo", true)
                     .Alias("foo-alias");

            container.Bind("bar", (container, args) => "bar", false)
                     .Alias("bar-alias");

            Assert.IsTrue(container.IsStatic("foo"));
            Assert.IsTrue(container.IsStatic("foo-alias"));
            Assert.IsFalse(container.IsStatic("bar"));
            Assert.IsFalse(container.IsStatic("bar-alias"));
        }

        [TestMethod]
        public void TestAlias()
        {
            container.Bind("foo", (container, args) => "foo", false)
                     .Alias("foo-alias");

            container.Instance("bar", "bar");
            container.Alias("bar-alias", "bar");

            Assert.AreEqual("foo", container["foo-alias"]);
            Assert.AreEqual("bar", container["bar-alias"]);
        }

        [TestMethod]
        [DataRow("bar-alias", "bar", typeof(CodeStandardException), "Set an alias for a service that does not exist.")]
        [DataRow(null, "bar", typeof(ArgumentNullException), "Alias name is null.")]
        [DataRow("baz", null, typeof(ArgumentNullException), "Service name is null.")]
        [DataRow("foo", "foo", typeof(LogicException), "Alias is same as service")]
        public void TestAliasIllegal(string alias, string service, Type expected, string reason)
        {
            container.Bind("foo", (container, args) => "foo", false)
                     .Alias("foo-alias");

            try
            {
                container.Alias(alias, service);
                Assert.Fail(reason);
            }
            catch (Exception ex) when(ex.GetType() != expected)
            {
                Assert.Fail($"Expected throw an exception: {expected}, actual throw: {ex}");
            }
            catch (Exception)
            {
                // test passed.
            }
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(LogicException), "Circular dependency detected while for")]
        public void TestCircularDependencyInject()
        {
            container.Bind("foo", typeof(CircularDependency), false);
            container.Make("foo");
        }

        [TestMethod]
        public void TestIsAlias()
        {
            container.Bind("foo", (container, args) => "foo", false)
                     .Alias("foo-alias");


            Assert.IsTrue(container.IsAlias("foo-alias"));
            Assert.IsFalse(container.IsAlias("foo"));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(new[] { "unless-parameter" })]
        public void TestCall(string[] userParams)
        {
            var serviceFoo = container.Type2Service(typeof(Foo));
            var serviceBar = container.Type2Service(typeof(Bar));

            container.Bind(serviceFoo, typeof(Foo), true);
            container.Bind(serviceBar, typeof(Bar), true);

            var fooBar = FooBar.New();
            var methodInfo = fooBar.GetType().GetMethod(nameof(fooBar.GetName));
            Assert.AreEqual("foobar", container.Call(fooBar, methodInfo, userParams));
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(LogicException), "Circular dependency detected while for")]
        public void TestCallCircularDependencyInject()
        {
            var service = container.Type2Service(typeof(CircularDependency));
            container.Bind(service, typeof(CircularDependency), false);

            var foo = new CircularDependency(null);
            container.Call(foo, foo.GetType().GetMethod(nameof(foo.Foo)));
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestCallGivenIncorrectParament()
        {
            container.Bind("num", (container, args) => "foo", true)
                     .Alias("$num");

            Action<int> action = (int num) =>
            {
                Assert.Fail("Should throw UnresolvableException exception.");
            };

            container.Call(action.Target, action.Method);
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(LogicException),
            "Too many parameters , must be less or equal than 255")]
        public void TestMakearameterOverflow()
        {
            container.Bind("foobar", typeof(FooBar), false);
            container.Make("foobar", new object[256]);
        }

        [TestMethod]
        public void TestTightInject()
        {
            Action<object[], Foo, Bar> action = (objs, foo, bar) =>
            {
                Assert.AreEqual(100, objs[0]);
                Assert.AreEqual("foo", objs[1]);
                Assert.AreNotEqual(null, foo);
                Assert.AreNotEqual(null, bar);
            };

            container.Call(action.Target, action.Method, 100, "foo");
        }

        [TestMethod]
        public void TestContextInjectionWithProperty()
        {
            var service = container.Type2Service(typeof(Baz));
            container.Bind(service, typeof(Baz), false)
                     .Needs("$Name").Given(() => "bazbaz")
                     .Needs("$Qux").Given(() => 100);

            var baz = (Baz)container.Make(service);
            Assert.AreEqual("bazbaz", baz.Name);
        }

        [TestMethod]
        public void TestContextInjection()
        {
            container.Bind("baz", typeof(Baz), false)
                     .Needs("$boo").Given(() => 500)
                     .Needs("$Qux").Given(() => 100);

            var baz = (Baz)container.Make("baz");
            Assert.AreEqual(500, baz.Boo);

            container.Bind("baz-cast-type", typeof(Baz), false)
                     .Needs("$boo").Given(() => "300")
                     .Needs("$Qux").Given(() => 100);
            baz = (Baz)container.Make("baz-cast-type");
            Assert.AreEqual(300, baz.Boo);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestContextInjectionErrorType()
        {
            container = CreateContainer();
            container.Bind("foobar", typeof(FooBar), false)
                     .Needs("$foo").Given(() => new Bar());
            container.Make("foobar");
        }

        [TestMethod]
        public void TestMake()
        {
            var service = container.Type2Service(typeof(FooBar));
            container.Bind(service, typeof(FooBar), true);
            var foobar = container.Make(service);
            Assert.AreNotEqual(null, foobar);
        }

        [TestMethod]
        public void TestMakeWithParams()
        {
            container.Bind("baz", typeof(Baz), false)
                     .Needs("$Qux").Given(() => 100);
            var baz = (Baz)container.Make("baz", 500);
            Assert.AreEqual(500, baz.Boo);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestMakeEmptyService()
        {
            container.Make(string.Empty);
        }

        [TestMethod]
        public void TestMakeWithAlias()
        {
            container.Bind("foo", typeof(Foo), true).Alias("foo-alias");
            container.Bind("bar", typeof(Bar), true).Alias("bar-alias");

            var foo = container.Make("foo");
            var bar = container.Make("bar");

            Assert.AreSame(foo, container.Make("foo-alias"));
            Assert.AreSame(bar, container.Make("bar-alias"));
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException),
            "Unresolvable dependency , resolving [Name] in class")]
        public void TestMakeAttributeInjectFaild()
        {
            container = CreateContainer();
            container.Bind("baz", typeof(Baz), true)
                     .Needs("$Qux").Given(() => 100);
            container.Make("baz", new Foo());
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(UnresolvableException),
            "Unresolvable dependency , resolving [Qux] in class")]
        public void TestMakeAttributeInjectFaildWithPrimitiveAttr()
        {
            container.Bind("baz", typeof(Baz), true);
            container.Make("baz", new Foo());
        }

        [TestMethod]
        public void TestMakeWithDefaultValuePrimitive()
        {
            container.Bind("baz", typeof(Baz), true)
                     .Needs("$Qux").Given(() => 500);
            var baz = (Baz)container.Make("baz");
            Assert.AreEqual(100, baz.Boo);
        }

        [TestMethod]
        public void TestMakeWithDefaultValue()
        {
            var container = CreateContainer();
            container.Bind("fubar", typeof(Fubar), true);

            var fubar = (Fubar)container.Make("fubar");
            Assert.AreEqual(null, fubar.Bar);
        }

        [TestMethod]
        public void TestMakeInjectWithStruct()
        {
            var service = container.Type2Service(typeof(Position));
            container.Bind("fubar", typeof(Fubar), true);
            container.Bind(service, (container, args) => new Position()
            {
                X = 1,
                Y = 2
            });

            var fubar = (Fubar)container.Make("fubar");

            Assert.AreEqual(1, fubar.Position.X);
            Assert.AreEqual(2, fubar.Position.Y);
        }

        [TestMethod]
        public void TestMakeInjectWithGeneric()
        {
            container = CreateContainer();
            container.Bind("fubar", typeof(Fubar), true);
            var service = container.Type2Service(typeof(IList<string>));
            container.Bind(service, (container, args) => new List<string>()
            {
                "iron man",
                "black window"
            });
            service = container.Type2Service(typeof(IList<int>));
            container.Bind(service, (container, args) => new List<int>()
            {
                25,
                23
            });

            var fubar = (Fubar)container.Make("fubar");

            CollectionAssert.AreEqual(
                new[] { "iron man", "black window" },
                fubar.Heros.ToArray());

            CollectionAssert.AreEqual(
                new[] { 25, 23 },
                fubar.Ages.ToArray());

            Assert.AreEqual(null, fubar.Bar);
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(LogicException),
            "can not bind.")]
        public void TestMakeAbstractClass()
        {
            container.Bind("foo", typeof(AbstractClass), true);
            container.Make("foo");
        }

        [TestMethod]
        public void TestMakeInjectWithInheritance()
        {
            var service = container.Type2Service(typeof(IList<string>));
            container.Bind("fubar", typeof(Fubar), true);
            container.Bind(service, (container, args) => new List<string>()
            {
                "iron man",
                "black window"
            });

            var fubar = (Fubar)container.Make("fubar");

            CollectionAssert.AreEqual(
                new[] { "iron man", "black window" },
                fubar.Heros.ToArray());

            Assert.AreNotEqual(null, fubar.Bar);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestMakeMissConstructor()
        {
            container.Bind("quux", typeof(Quux), true);
            container.Make("quux");
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(TestException), "QuuxFoo")]
        public void TestMakeConstructorThrowException()
        {
            container.Bind("foo", typeof(QuuxFoo), false);
            container.Make("foo");
        }

        [TestMethod]
        public void TestMakeWithIParams()
        {
            container = CreateContainer();
            container.Bind("foobar", typeof(FooBar), false);

            var foo = new Foo();
            var bar = new Bar();
            var foobar = (FooBar)container.Make("foobar",
                new ParamsCollection
            {
                {"foo", foo},
            }, bar);

            Assert.AreEqual("foobar", foobar.ToString());
            Assert.AreSame(foo, foobar.Foo);
            Assert.AreSame(bar, foobar.Bar);
        }

        [TestMethod]
        public void TestMakeWithMultIParamsOrder()
        {
            container = CreateContainer();
            container.Bind("foobar", typeof(FooBar), false);

            var foo1 = new Foo();
            var bar1 = new Bar();
            var foo2 = new Foo();
            var bar2 = new Bar();
            var foobar = (FooBar)container.Make("foobar",
                new ParamsCollection
            {
                { "foo", foo1 },
                { "bar", bar1 }
            }, new ParamsCollection
            {
                { "foo", foo2 },
            }, bar2);

            Assert.AreEqual("foobar", foobar.ToString());
            Assert.AreSame(foo1, foobar.Foo);
            Assert.AreSame(bar1, foobar.Bar);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestMakeWithIParamsGivenErrorType()
        {
            container = CreateContainer();
            container.Bind("foobar", typeof(FooBar), false);

            container.Make("foobar", new ParamsCollection
                {
                    {"foo", new Bar()},
                    {"bar", new Foo()},
                });
        }

        [TestMethod]
        public void TestMakeNullable()
        {
            container.Bind("bar", typeof(Bar), false)
                     .Needs("$Age").Given(() => 24);
            var bar = (Bar)container.Make("bar");

            Assert.IsFalse(bar.Num.HasValue);
            Assert.AreEqual(24, bar.Age.Value);
        }

        [TestMethod]
        public void TestCanMake()
        {
            Assert.IsFalse(container.CanMake("foo"));
            Assert.IsTrue(container.CanMake<Foo>());

            container = CreateContainer();
            Assert.IsFalse(container.CanMake<Foo>());
        }

        [TestMethod]
        public void TestInstance()
        {
            var bar = new object();
            container.Instance("foo", "foo");
            container.Instance("bar", bar);

            Assert.AreEqual("foo", container.Make("foo"));
            Assert.AreSame(bar, container.Make("bar"));

            // double checked
            Assert.AreSame(bar, container.Make("bar"));
        }

        [TestMethod]
        [ExpectedException(typeof(CodeStandardException))]
        [DataRow("foo:bar")]
        [DataRow("$foo")]
        [DataRow("foo@bar")]
        public void TestInstanceIllegalChars(string service)
        {
            container.Instance(service, "illegal chars");
        }

        [TestMethod]
        [ExpectedException(typeof(CodeStandardException))]
        public void TestInstanceNotAllowedSameObject()
        {
            var foo = new object();
            container.Instance("foo", foo);
            container.Instance("bar", foo);
        }

        [TestMethod]
        public void TestInstanceAllowedDifferenceObject()
        {
            container.Instance("foo", "foo");
            container.Instance("bar", "bar");
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException)
            , "is not Singleton(Static) Bind.")]
        public void TestInstanceIllegal()
        {
            container.Bind("foo", typeof(Foo), false);
            container.Instance("foo", "foo");
        }

        [TestMethod]
        public void TestReleaseAutoCallDispose()
        {
            container.Bind("bar", typeof(Bar), true);
            var bar = (Bar)container.Make("bar");

            Assert.IsFalse(bar.Disposed);
            container.Release("bar");
            Assert.IsTrue(bar.Disposed);
        }

        [TestMethod]
        public void TestOnRelease()
        {
            var count = 0;
            container.Bind("foo", typeof(Foo), true)
                     .OnRelease((binder, instance)=>
                     {
                         Assert.AreEqual(typeof(Foo), instance.GetType());
                         count++;
                     });

            container.Make("foo");
            container.Release("foo");

            container = CreateContainer();
            container.Instance("foo", "foo");
            container.OnRelease((binder, instance) =>
            {
                Assert.AreEqual("foo", instance);
                count++;
            });
            container.Release("foo");

            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public void TestFlush()
        {
            container["foo"] = "foo";
            container["bar"] = "bar";
            container["baz"] = "baz";

            container.Flush();

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make("foo");
            });

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make("bar");
            });

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make("baz");
            });
        }

        [TestMethod]
        public void TestVariant()
        {
            container.Bind("foo", typeof(Variant), false);

            var foo = (Variant)container.Make("foo", 1);
            Assert.AreEqual("iron man", foo.Model.Name);

            foo = (Variant)container.Make("foo", 2);
            Assert.AreEqual("black window", foo.Model.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestVariantThrowException()
        {
            container.Bind("foo", typeof(Variant), false);
            container.Make("foo", -1);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestVariantForceGivenNull()
        {
            container.Bind("foo", typeof(Variant), false);
            container.Make("foo", new ParamsCollection
                {
                    { "model", null }
                });
        }

        [TestMethod]
        [ExpectedException(typeof(CodeStandardException))]
        public void TestReboundNotExistsService()
        {
            var container = new Container();
            container.OnRebound("foo", (instance) =>
            {
            });
        }

        [TestMethod]
        public void TestUnbindNotExistsService()
        {
            container.Unbind("service not exists");
        }

        protected virtual IContainer CreateContainer()
        {
            return new Container();
        }

        // todo: rebuild .....

        [TestMethod]
        public void TestOnRebound()
        {
            var container = new Container();
            var callRebound = false;

            var temp = container.Bind("TestService", (c, p) => 100);
            container.OnRebound("TestService", (instance) =>
            {
                Assert.AreEqual(300, instance);
                callRebound = true;
            });
            temp.Unbind();
            var bind = container.Bind("TestService", (c, p) => 200);
            container.Make("TestService");
            bind.Unbind();
            container.Bind("TestService", (c, p) => 300);

            Assert.AreEqual(true, callRebound);
        }

        [TestMethod]
        public void TestOnReboundWithInstance()
        {
            var container = new Container();
            var callRebound = false;

            container.Instance("TestService", 100);
            container.OnRebound("TestService", (instance) =>
            {
                Assert.AreEqual(300, instance);
                callRebound = true;
            });
            container.Instance("TestService", 300);

            Assert.AreEqual(true, callRebound);
        }

        public class TestWatchCLass
        {
            public int value;

            public IContainer container;

            public void OnChange(int instance, IContainer container)
            {
                value = instance;
                this.container = container;
            }
        }

        [TestMethod]
        public void TestInstanceAndDecorator()
        {
            var container = new Container();
            var oldObject = new object();
            object newObject = null;
            container.Extend("Hello", (instance, c) =>
            {
                Console.WriteLine("new");
                return newObject = new object();
            });

            container.Bind("Hello", (_, __) => oldObject);
            var ins = container["Hello"];
            Assert.AreSame(newObject, ins);
        }

        [TestMethod]
        public void TestHasInstance()
        {
            var container = new Application();
            container.Instance<Application>(container);

            Assert.AreEqual(true, container.HasInstance<Application>());
            Assert.AreEqual(false, container.HasInstance<IBindData>());
        }

        [TestMethod]
        public void TestIsResolved()
        {
            var container = new Application();
            container.Instance<Application>(container);

            Assert.AreEqual(true, container.IsResolved<Application>());
            Assert.AreEqual(false, container.IsResolved<IBindData>());
        }

        [TestMethod]
        public void TestFlushAndInstance()
        {
            var container = new Application();
            container.Instance<Application>(container);

            container.OnRelease((__) =>
            {
                container.Instance<Application>(container);
            });

            var isError = false;
            try
            {
                container.Flush();
            }
            catch (RuntimeException)
            {
                isError = true;
            }

            Assert.AreEqual(true, isError);
        }

        public class TestFlushOrderDependencyClass
        {
        }

        public class TestFlushOrderClass
        {
            public TestFlushOrderClass(TestFlushOrderDependencyClass cls)
            {
            }
        }

        [TestMethod]
        public void TestFlushOrder()
        {
            var container = new Container();
            container.Instance<Container>(container);

            var list = new List<object>();

            container.OnRelease((_) =>
            {
                list.Add(_);
            });

            container.Singleton<TestFlushOrderDependencyClass>();
            container.Singleton<TestFlushOrderClass>();

            container.Make<TestFlushOrderClass>();

            container.Flush();

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(typeof(TestFlushOrderClass), list[0].GetType());
            Assert.AreEqual(typeof(TestFlushOrderDependencyClass), list[1].GetType());
            Assert.AreEqual(typeof(Container), list[2].GetType());
        }

        [TestMethod]
        public void TestRebuildAndFlush()
        {
            var container = new Application();
            container.Bootstrap();
            container.Make<IApplication>();
            var list = new List<object>();

            container.Singleton<TestFlushOrderDependencyClass>();
            container.Singleton<TestFlushOrderClass>();

            container.Make<TestFlushOrderClass>();
            var temp = Facade<TestFlushOrderDependencyClass>.Instance;
            container.Release<TestFlushOrderDependencyClass>();

            container.OnRelease((_) =>
            {
                if(typeof(TestFlushOrderDependencyClass) ==  _.GetType()
                   || typeof(Application) == _.GetType()
                   || typeof(TestFlushOrderClass) == _.GetType())
                list.Add(_);
            });
            container.Instance<TestFlushOrderDependencyClass>(new TestFlushOrderDependencyClass());

            container.Flush();

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(typeof(TestFlushOrderClass), list[0].GetType());
            Assert.AreEqual(typeof(TestFlushOrderDependencyClass), list[1].GetType());
            Assert.AreEqual(typeof(Application), list[2].GetType());
        }

        [TestMethod]
        public void TestRebuildAndFlushNotWatch()
        {
            var container = new Application();
            container.Bootstrap();
            container.Make<IApplication>();
            var list = new List<object>();

            container.Singleton<TestFlushOrderDependencyClass>();
            container.Singleton<TestFlushOrderClass>();

            container.Make<TestFlushOrderClass>();
            container.Release<TestFlushOrderDependencyClass>();

            container.OnRelease((_) =>
            {
                if (typeof(TestFlushOrderDependencyClass) == _.GetType()
                    || typeof(Application) == _.GetType()
                    || typeof(TestFlushOrderClass) == _.GetType())
                    list.Add(_);
            });
            container.Instance<TestFlushOrderDependencyClass>(new TestFlushOrderDependencyClass());

            container.Flush();

            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(typeof(TestFlushOrderDependencyClass), list[0].GetType());
            Assert.AreEqual(typeof(TestFlushOrderClass), list[1].GetType());
            Assert.AreEqual(typeof(Application), list[2].GetType());
        }

        [TestMethod]
        public void TestCannotWatch()
        {
        }

        [TestMethod]
        public void TestExtend()
        {
            var container = new Container();
            container.Bind("hello", (_, __) => "hello");
            container.Extend("hello", (instance, c) => instance + " world");

            Assert.AreEqual("hello world", container["hello"]);
        }

        [TestMethod]
        public void TestMultExtend()
        {
            var container = new Container();
            container.Bind("hello", (_, __) => "hello");
            container.Extend("hello", (instance, c) => instance + " world");
            container.Extend("hello", (instance, c) => instance + " miaomiao");

            Assert.AreEqual("hello world miaomiao", container["hello"]);
        }

        [TestMethod]
        public void TestClearExtend()
        {
            var container = new Container();
            container.Bind("hello", (_, __) => "hello");
            container.Extend("hello", (instance, c) => instance + " world");
            container.Extend("hello", (instance, c) => instance + " miaomiao");

            container.Bind("world", (_, __) => "hello");
            container.Extend("world", (instance, c) => instance + " world");
            container.Extend("world", (instance, c) => instance + " miaomiao");

            Assert.AreEqual("hello world miaomiao", container["hello"]);
            Assert.AreEqual("hello world miaomiao", container["world"]);

            container.ClearExtenders("hello");

            Assert.AreEqual("hello", container["hello"]);
            Assert.AreEqual("hello world miaomiao", container["world"]);
        }

        [TestMethod]
        public void TestClearExtendNotUse()
        {
            var container = new Container();
            container.Bind("hello", (_, __) => "hello");
            container.Extend("hello", (instance, c) => instance + " world");
            container.Extend("hello", (instance, c) => instance + " miaomiao");
            container.ClearExtenders("hello");
            Assert.AreEqual("hello", container["hello"]);
        }

        [TestMethod]
        public void TestExtendSingle()
        {
            var container = new Container();
            container.Singleton("hello", (_, __) => "hello");
            container.Extend("hello", (instance, c) => instance + " world");
            Assert.AreEqual("hello world", container["hello"]);
            container.Extend("hello", (instance, c) => instance + " miaomiao");
            Assert.AreEqual("hello world miaomiao", container["hello"]);

            container.Release("hello");
            Assert.AreEqual("hello world", container["hello"]);
        }

        [TestMethod]
        public void TestIsResolvedExtend()
        {
            var container = new Container();
            container.Singleton("hello", (_, __) => "hello");
            container.Make("hello");
            container.Release("hello");

            var data = "";
            container.OnRebound("hello", (instance) =>
            {
                data = instance.ToString();
            });
            container.Extend("hello", (instance, c) => instance + " world");

            Assert.AreEqual("hello world", data);
        }

        public class TestExtendGivenMismatchedTypeClass
        {
            public TestExtendGivenMismatchedTypeClass(IContainer container)
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestExtendGivenMismatchedType()
        {
            var container = new Container();
            container.Singleton<IContainer>(() => container);
            container.Singleton<TestExtendGivenMismatchedTypeClass>();
            container.Extend<IContainer>((instance) => "123");
            container.Make<TestExtendGivenMismatchedTypeClass>();
        }

        [TestMethod]
        public void TestThisSet()
        {
            var container = new Container();
            container["hello"] = "world";
            Assert.AreEqual("world", container.Make("hello"));
        }

        [TestMethod]
        public void TestMultThisSet()
        {
            var container = new Container();
            container["hello"] = "world";
            container["world"] = "hello";
            Assert.AreEqual("world", container.Make("hello"));
            Assert.AreEqual("hello", container.Make("world"));
        }

        [TestMethod]
        public void TestExistsThisSet()
        {
            var container = new Container();
            container["hello"] = "world";
            Assert.AreEqual("world", container.Make("hello"));
            container["hello"] = 123;
            Assert.AreEqual(123, container.Make("hello"));
        }

        [TestMethod]
        public void TestOnAfterResolving()
        {
            var container = new Container();
            var val = 0;
            container.OnAfterResolving((_) =>
            {
                Assert.AreEqual(10, val);
                val = 20;
            });
            container.OnAfterResolving((_,__) =>
            {
                Assert.AreEqual(20, val);
                val = 30;
            });
            container.OnResolving((_, __) =>
            {
                Assert.AreEqual(0, val);
                val = 10;
            });

            container["hello"] = "hello";
            Assert.AreEqual("hello", container["hello"]);
            Assert.AreEqual(30, val);
        }

        [TestMethod]
        public void TestOnAfterResolvingLocal()
        {
            var container = new Container();
            var val = 0;
            container.Bind("hello", (_, __) => "world").OnAfterResolving(() =>
            {
                Assert.AreEqual(10, val);
                val = 20;
            }).OnAfterResolving((_) =>
            {
                Assert.AreEqual(20, val);
                val = 30;
            }).OnResolving(() =>
            {
                Assert.AreEqual(0, val);
                val = 10;
            });

            Assert.AreEqual("world", container["hello"]);
            Assert.AreEqual(30, val);
        }

        public class TestNeedGivenWithParamNameClass
        {
            public int MyParam { get; set; }

            public TestNeedGivenWithParamNameClass(int myParam)
            {
                MyParam = myParam;
            }
        }

        [TestMethod]
        public void TestNeedGivenWithParamName()
        {
            var container = new Container();
            container.Bind<TestNeedGivenWithParamNameClass>()
                .Needs("$myParam").Given(() => 100);

            Assert.AreEqual(100, container.Make<TestNeedGivenWithParamNameClass>().MyParam);

            container = new Container();
            container.Bind<TestNeedGivenWithParamNameClass>()
                .Needs("$myParam").Given<int>();
            container.Bind<int>(() => 200);

            Assert.AreEqual(200, container.Make<TestNeedGivenWithParamNameClass>().MyParam);
        }

        [TestMethod]
        public void TestNullRelease()
        {
            var container = new Container();
            Assert.AreEqual(false, container.Release(null));
        }

        public class TestGivenInvalidTypeClass
        {
            public TestGivenInvalidTypeClass(Container container)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestGivenInvalidType()
        {
            var container = new Container();
            container.Bind<TestGivenInvalidTypeClass>()
                .Needs("$container").Given(() => 123);
            container.Make<TestGivenInvalidTypeClass>();
        }

        public class TestGivenInvalidTypeAttrClass
        {
            [Inject]
            public Container container { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestGivenInvalidTypeAttr()
        {
            var container = new Container();
            container.Bind<TestGivenInvalidTypeAttrClass>()
                .Needs("$container").Given(() => 123);
            container.Make<TestGivenInvalidTypeAttrClass>();
        }

        public class NotSupportNullInject : Container
        {
            protected override bool CanInject(Type type, object instance)
            {
                return instance != null;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestGivenInvalidTypeAttrNotSupportNullInject()
        {
            var container = new NotSupportNullInject();
            container.Bind<TestGivenInvalidTypeAttrClass>()
                .Needs("$container").Given(() => null);
            container.Make<TestGivenInvalidTypeAttrClass>();
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestGivenInvalidTypeNotSupportNullInject()
        {
            var container = new NotSupportNullInject();
            container.Bind<TestGivenInvalidTypeClass>()
                .Needs("$container").Given(() => null);
            container.Make<TestGivenInvalidTypeClass>();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestSetAliasIsService()
        {
            var container = new Container();
            container.Bind("abc", (c, p) => 1, false);
            container.Bind("ccc", (c, p) => 1, false);
            container.Alias("abc", "ccc");
        }

        /// <summary>
        /// 生成容器.
        /// </summary>
        /// <returns>容器.</returns>
        private Container MakeContainer()
        {
            var container = new Container();
            return container;
        }
    }
}
