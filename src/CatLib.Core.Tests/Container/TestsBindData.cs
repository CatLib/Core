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
using System;
using CContainer = CatLib.Container.Container;

namespace CatLib.Tests.Container
{
    [TestClass]
    public class TestsBindData
    {
        private CContainer container;
        private BindData bindData;

        [TestInitialize]
        public void Init()
        {
            container = new CContainer();
            bindData = (BindData)container.Bind("foo", (container, args) => "foo", true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckNeedsIllegal()
        {
            bindData.Needs(null);
        }

        [TestMethod]
        public void TestGetContextual()
        {
            bindData.Needs("foo").Given("bar");
            Assert.AreEqual("bar", bindData.GetContextual("foo"));

            bindData.Needs("bar").Given<Foo>();
            Assert.AreEqual(container.Type2Service(typeof(Foo)), bindData.GetContextual("bar"));
        }

        [TestMethod]
        public void TestGetContextualClosure()
        {
            bindData.Needs("foo").Given(() => "foo");

            Assert.AreEqual("foo", bindData.GetContextualClosure("foo")());
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestNeedsDuplicate()
        {
            bindData.Needs("foo").Given("bar");
            bindData.Needs("foo").Given("baz");
        }

        [TestMethod]
        public void TestAlias()
        {
            bindData.Alias("foo-alias");
            bindData.Alias<IFoo>();

            Assert.AreSame(bindData, container.GetBind("foo-alias"));
            Assert.AreSame(bindData, container.GetBind<IFoo>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestAliasIllegal()
        {
            bindData.Alias(null);
        }

        [TestMethod]
        public void TestTag()
        {
            bindData.Tag("tag");
            CollectionAssert.AreEqual(new[] { "foo" }, container.Tagged("tag"));
        }

        [TestMethod]
        public void TestOnRelease()
        {
            var count = 0;
            bindData.OnRelease((binder, instance) =>
            {
                Assert.AreEqual("foo", instance);
                Assert.AreSame(bindData, binder);
                count++;
            });

            bindData.OnRelease(() =>
            {
                count++;
            });

            container.Make("foo");
            container.Release("foo");
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestOnReleaseIllegal()
        {
            bindData.OnRelease(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestOnResolvingIllegal()
        {
            bindData.OnResolving(null);
        }

        [TestMethod]
        [ExpectedException(typeof(UnresolvableException))]
        public void TestUnbind()
        {
            Assert.AreEqual("foo", container.Make("foo"));
            bindData.Unbind();
            container.Make("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestUnbindAfterChanged()
        {
            bindData.Unbind();
            bindData.Alias("foo-alias");
        }
    }
}
