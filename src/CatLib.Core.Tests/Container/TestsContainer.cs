﻿/*
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
using System.Collections.Generic;
using System.Linq;
using CContainer = CatLib.Container.Container;
using SException = System.Exception;

namespace CatLib.Tests.Container
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

            CollectionAssert.AreEqual(
                new object[] { foo, 100, 200 },
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
        [ExpectedExceptionAndMessage(
            typeof(LogicException),
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

            Assert.ThrowsException<LogicException>(() =>
            {
                container.Bind("foo", (container, args) => "repeat bind", false);
            });

            container.Instance("bar", "bar");
            Assert.ThrowsException<LogicException>(() =>
            {
                container.Bind("bar", (container, args) => "instance repeat bind", false);
            });

            container.Alias("foo-alias", "foo");
            Assert.ThrowsException<LogicException>(() =>
            {
                container.Bind("foo-alias", (container, args) => "alias repeat bind", false);
            });

            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                container.Bind(null, (container, args) => "invalid service name", false);
            });

            Assert.ThrowsException<LogicException>(() =>
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
        [DataRow("bar-alias", "bar", typeof(LogicException), "Set an alias for a service that does not exist.")]
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
            catch (SException ex) when (ex.GetType() != expected)
            {
                Assert.Fail($"Expected throw an exception: {expected}, actual throw: {ex}");
            }
#pragma warning disable CA1031
            catch (SException)
#pragma warning restore CA1031
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
        [ExpectedExceptionAndMessage(
            typeof(LogicException),
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
        [ExpectedException(
            typeof(UnresolvableException),
            "Unresolvable dependency , resolving [Name] in class")]
        public void TestMakeAttributeInjectFaild()
        {
            container = CreateContainer();
            container.Bind("baz", typeof(Baz), true)
                     .Needs("$Qux").Given(() => 100);
            container.Make("baz", new Foo());
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(
            typeof(UnresolvableException),
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
            container = CreateContainer();
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
                Y = 2,
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
                "black window",
            });
            service = container.Type2Service(typeof(IList<int>));
            container.Bind(service, (container, args) => new List<int>()
            {
                25,
                23,
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
        [ExpectedExceptionAndMessage(
            typeof(LogicException),
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
                "black window",
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
        [ExpectedExceptionAndMessage(typeof(AssertException), "marked inject attribute, but the property is not public.")]
        public void TestPropertyInjectWithProtectedAccessFaild()
        {
            var service = container.Type2Service(typeof(Foo));
            container.Bind(service, typeof(Foo), true);
            container.Bind("protected", typeof(ProtectedProperty), true);
            container.Make("protected");
        }

        [TestMethod]
        [ExpectedExceptionAndMessage(typeof(UnresolvableException), "QuuxFoo")]
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
            var foobar = (FooBar)container.Make(
                "foobar",
                new ParamsCollection
                {
                    { "foo", foo },
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
            var foobar = (FooBar)container.Make(
                "foobar",
                new ParamsCollection
            {
                { "foo", foo1 },
                { "bar", bar1 },
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
                    { "foo", new Bar() },
                    { "bar", new Foo() },
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
        [ExpectedException(typeof(LogicException))]
        [DataRow("foo:bar")]
        [DataRow("$foo")]
        [DataRow("foo@bar")]
        public void TestInstanceIllegalChars(string service)
        {
            container.Instance(service, "illegal chars");
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
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
        [ExpectedException(
            typeof(LogicException),
            "is not Singleton(Static) Bind.")]
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
                     .OnRelease((binder, instance) =>
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

            Assert.ThrowsException<UnresolvableException>(() =>
            {
                container.Make("foo");
            });

            Assert.ThrowsException<UnresolvableException>(() =>
            {
                container.Make("bar");
            });

            Assert.ThrowsException<UnresolvableException>(() =>
            {
                container.Make("baz");
            });
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestFlushInstanceService()
        {
            container.Bind("foo", typeof(Foo), true)
                     .OnRelease((binder, instance) =>
                     {
                         container.Instance("bar", "bar");
                     });

            container.Make("foo");
            container.Flush();
        }

        [TestMethod]
        public void TestFlushOrder()
        {
            var foo = container.Type2Service(typeof(Foo));
            var bar = container.Type2Service(typeof(Bar));
            container.Bind(foo, typeof(Foo), true);
            container.Bind(bar, typeof(Bar), true);
            container.Bind("foobar", typeof(FooBar), true);

            var actual = new List<Type>();
            container.OnRelease((binder, instance) =>
            {
                actual.Add(instance.GetType());
            });

            container.Make("foobar");
            container.Flush();

            CollectionAssert.AreEqual(
                new[]
                {
                    typeof(FooBar),
                    typeof(Bar),
                    typeof(Foo),
                }, actual);
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
                    { "model", null },
                });
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestReboundNotExistsService()
        {
            container.OnRebound("foobar", (instance) =>
            {
            });
        }

        [TestMethod]
        public void TestRebound()
        {
            var count = 0;
            var binder = container.Bind("foo", (container, args) => "foo");
            container.OnRebound("foo", (instance) =>
            {
                Assert.AreEqual("bar", instance);
                count++;
            });

            binder.Unbind();

            // first built don't trigger OnRebound.
            binder = container.Bind("foo", (c, p) => "boo");
            container.Make("foo");
            binder.Unbind();

            container.Bind("foo", (c, p) => "bar");

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void TestReboundWithInstance()
        {
            var count = 0;
            container.Instance("foo", "foo");
            container.OnRebound("foo", (instance) =>
            {
                Assert.AreEqual("bar", instance);
                count++;
            });
            container.Instance("foo", "bar");
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void TestUnbindNotExistsService()
        {
            container.Unbind("service not exists");
        }

        [TestMethod]
        public void TestHasInstance()
        {
            container.Instance("foo", "foo");

            Assert.IsTrue(container.HasInstance("foo"));
            Assert.IsFalse(container.HasInstance("bar"));
        }

        [TestMethod]
        public void TestIsResolved()
        {
            container.Instance("foo", "foo");

            Assert.IsTrue(container.IsResolved("foo"));
            Assert.IsFalse(container.IsResolved("bar"));

            container.Bind("bar", typeof(Bar), true);
            Assert.IsFalse(container.IsResolved("bar"));

            container.Make("bar");
            Assert.IsTrue(container.IsResolved("bar"));
        }

        [TestMethod]
        public void TestExtend()
        {
            container.Bind("foo", (container, args) => "foo", false);
            container.Extend("foo", (instance, container) => instance + "bar");

            Assert.AreEqual("foobar", container["foo"]);
        }

        [TestMethod]
        public void TestExtendMult()
        {
            container.Bind("foo", (container, args) => "foo", false);
            container.Extend("foo", (instance, container) => instance + "bar");
            container.Extend("foo", (instance, container) => instance + "baz");

            Assert.AreEqual("foobarbaz", container["foo"]);
        }

        [TestMethod]
        public void TestExtendSingle()
        {
            container.Bind("foo", (container, args) => "foo", true);
            container.Extend("foo", (instance, container) => instance + "bar");

            Assert.AreEqual("foobar", container["foo"]);

            // Existing instances are only valid if they are extended.
            container.Extend("foo", (instance, container) => instance + "baz");
            Assert.AreEqual("foobarbaz", container["foo"]);

            container.Release("foo");
            Assert.AreEqual("foobar", container["foo"]);
        }

        [TestMethod]
        public void TestExtendAndRebound()
        {
            container.Bind("foo", (container, args) => "foo", false);

            // marked resolved.
            container.Make("foo");
            container.Release("foo");

            var actual = string.Empty;
            container.OnRebound("foo", (instance) =>
            {
                actual = instance.ToString();
            });

            container.Extend("foo", (instance, container) => instance + "bar");

            Assert.AreEqual("foobar", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestExtendGivenMismatchedType()
        {
            var service = container.Type2Service(typeof(Foo));
            container.Bind(service, typeof(Foo), true);
            container.Bind("foobar", typeof(FooBar), true);

            container.Extend(service, (instance, container) => "mismatched type");
            container.Make("foobar");
        }

        [TestMethod]
        public void TestClearExtend()
        {
            if (!(container is CContainer catlibContainer))
            {
                return;
            }

            container.Bind("foo", (container, args) => "foo", false);
            container.Extend("foo", (instance, container) => instance + "bar");
            container.Extend("foo", (instance, container) => instance + "baz");

            container.Bind("bar", (container, args) => "bar", false);
            container.Extend("bar", (instance, container) => instance + "foo");
            container.Extend("bar", (instance, container) => instance + "baz");

            Assert.AreEqual("foobarbaz", container["foo"]);
            Assert.AreEqual("barfoobaz", container["bar"]);

            catlibContainer.ClearExtenders("foo");

            Assert.AreEqual("foo", container["foo"]);
            Assert.AreEqual("barfoobaz", container["bar"]);
        }

        [TestMethod]
        public void TestIndexer()
        {
            container["foo"] = "bar";
            Assert.AreEqual("bar", container["foo"]);
        }

        [TestMethod]
        public void TestIndexerOverride()
        {
            container["foo"] = "bar";
            Assert.AreEqual("bar", container["foo"]);

            container["foo"] = "baz";
            Assert.AreEqual("baz", container["foo"]);
        }

        [TestMethod]
        public void TestOnAfterResolving()
        {
            var step = 0;
            container.OnAfterResolving((binder, instance) =>
            {
                Assert.AreEqual(1, step);
                step = 2;
            });
            container.OnAfterResolving((binder, instance) =>
            {
                Assert.AreEqual(2, step);
                step = 3;
            });
            container.OnResolving((binder, instance) =>
            {
                Assert.AreEqual(0, step);
                step = 1;
            });

            container["foo"] = "bar";
            Assert.AreEqual("bar", container["foo"]);
            Assert.AreEqual(3, step);
        }

        [TestMethod]
        public void TestOnAfterResolvingLocal()
        {
            var step = 0;
            container.Bind("foo", (binder, instance) => "bar")
                .OnAfterResolving(() =>
            {
                Assert.AreEqual(1, step);
                step = 2;
            }).OnAfterResolving((_) =>
            {
                Assert.AreEqual(2, step);
                step = 3;
            }).OnResolving(() =>
            {
                Assert.AreEqual(0, step);
                step = 1;
            });

            Assert.AreEqual("bar", container["foo"]);
            Assert.AreEqual(3, step);
        }

        [TestMethod]
        public void TestNullRelease()
        {
            Assert.AreEqual(false, container.Release(null));
        }

        protected virtual IContainer CreateContainer()
        {
            return new CContainer();
        }
    }
}
