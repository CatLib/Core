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

using CatLib.Tests.Fixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Dispatcher = CatLib.EventDispatcher.EventDispatcher;

namespace CatLib.EventDispatcher.Tests
{
    [TestClass]
    public class TestsEventDispatcher
    {
        private Dispatcher eventDispatcher;

        internal interface IResponse
        {
            void Foo(EventArgs eventArgs);
        }

        [TestInitialize]
        public void Initialize()
        {
            eventDispatcher = new Dispatcher();
        }

        [TestMethod]
        public void TestDispatch()
        {
            var expected = new TestEventArgs();
            var mock = new Mock<IResponse>();
            eventDispatcher.AddListener("foo", mock.Object.Foo);
            eventDispatcher.Dispatch("foo", expected);
            mock.Verify((o) => o.Foo(expected));
        }

        [TestMethod]
        public void TestAddListener()
        {
            var first = new Mock<IResponse>();
            var second = new Mock<IResponse>();

            eventDispatcher.AddListener("foo", first.Object.Foo);
            eventDispatcher.AddListener("foo", second.Object.Foo);

            CollectionAssert.AreEqual(
                new Action<EventArgs>[]
            {
                second.Object.Foo,
                first.Object.Foo,
            }, eventDispatcher.GetListeners("foo"));
        }

        [TestMethod]
        public void TestAddListenerBySort()
        {
            var first = new Mock<IResponse>();
            var second = new Mock<IResponse>();
            var third = new Mock<IResponse>();

            eventDispatcher.AddListener("foo", first.Object.Foo, 10);
            eventDispatcher.AddListener("foo", second.Object.Foo, -10);
            eventDispatcher.AddListener("foo", third.Object.Foo);

            CollectionAssert.AreEqual(
                new Action<EventArgs>[]
                {
                    second.Object.Foo, third.Object.Foo, first.Object.Foo,
                },
                eventDispatcher.GetListeners("foo"));
        }

        [TestMethod]
        public void TestHasListener()
        {
            var foo = new Mock<IResponse>();
            var bar = new Mock<IResponse>();

            eventDispatcher.AddListener("foo", foo.Object.Foo);
            eventDispatcher.AddListener("bar", bar.Object.Foo);

            Assert.IsTrue(eventDispatcher.HasListeners("foo"));
            Assert.IsTrue(eventDispatcher.HasListeners("bar"));
            Assert.IsFalse(eventDispatcher.HasListeners("baz"));
        }

        [TestMethod]
        public void TestRepateAddSameListeners()
        {
            var first = new Mock<IResponse>();

            Assert.IsTrue(eventDispatcher.AddListener("foo", first.Object.Foo));
            Assert.IsFalse(eventDispatcher.AddListener("foo", first.Object.Foo));
        }

        [TestMethod]
        public void TestRemoveListeners()
        {
            var foo = new Mock<IResponse>();
            var bar = new Mock<IResponse>();
            var expected = new TestEventArgs();

            eventDispatcher.AddListener("foo", foo.Object.Foo);
            eventDispatcher.AddListener("bar", bar.Object.Foo);

            eventDispatcher.Dispatch("foo", expected);
            eventDispatcher.Dispatch("bar", expected);

            foo.Verify((o) => o.Foo(expected));
            bar.Verify((o) => o.Foo(expected));

            eventDispatcher.RemoveListener("foo", foo.Object.Foo);

            eventDispatcher.Dispatch("foo", expected);
            eventDispatcher.Dispatch("bar", expected);

            foo.Verify((o) => o.Foo(expected), Times.Exactly(1));
            bar.Verify((o) => o.Foo(expected), Times.Exactly(2));
        }

        [TestMethod]
        public void TestRemoveAllListeners()
        {
            var foo = new Mock<IResponse>();
            var foobar = new Mock<IResponse>();
            var expected = new TestEventArgs();

            eventDispatcher.AddListener("foo", foo.Object.Foo);
            eventDispatcher.AddListener("foo", foobar.Object.Foo);

            eventDispatcher.Dispatch("foo", expected);

            foo.Verify((o) => o.Foo(expected));
            foobar.Verify((o) => o.Foo(expected));

            eventDispatcher.RemoveListener("foo");

            eventDispatcher.Dispatch("foo", expected);

            foo.Verify((o) => o.Foo(expected), Times.Exactly(1));
            foobar.Verify((o) => o.Foo(expected), Times.Exactly(1));
        }

        [TestMethod]
        public void TestRemoveNotExistsListener()
        {
            var foo = new Mock<IResponse>();
            Assert.IsFalse(eventDispatcher.RemoveListener("foo", foo.Object.Foo));
            Assert.IsFalse(eventDispatcher.RemoveListener("bar"));
        }

        [TestMethod]
        public void TestStoppableEvent()
        {
            var first = new Mock<IResponse>();
            var second = new Mock<IResponse>();
            var expected = new TestEventArgs();

            // Same priority, post-listener priority.
            second.Setup((o) => o.Foo(expected)).Callback(() =>
            {
                expected.Stop();
            });

            eventDispatcher.AddListener("foo", first.Object.Foo);
            eventDispatcher.AddListener("foo", second.Object.Foo);

            eventDispatcher.Dispatch("foo", expected);

            first.Verify((o) => o.Foo(expected), Times.Never);
            second.Verify((o) => o.Foo(expected));
        }
    }
}
