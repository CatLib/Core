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

        [TestInitialize]
        public void Initialize()
        {
            eventDispatcher = new Dispatcher();
        }

        [TestMethod]
        public void TestDispatch()
        {
            var expected = new TestEventArgs();
            var handler = new Mock<EventHandler>();
            eventDispatcher.AddListener("foo", handler.Object);
            eventDispatcher.Dispatch("foo", this, expected);
            handler.Verify((o) => o.Invoke(this, expected));
        }

        [TestMethod]
        public void TestAddListener()
        {
            var foo1 = new Mock<EventHandler>();
            var foo2 = new Mock<EventHandler>();

            eventDispatcher.AddListener("foo", foo1.Object);
            eventDispatcher.AddListener("foo", foo2.Object);

            CollectionAssert.AreEqual(
                new[]
                {
                    foo1.Object,
                    foo2.Object,
                }, eventDispatcher.GetListeners("foo"));
        }

        [TestMethod]
        public void TestHasListener()
        {
            var foo = new Mock<EventHandler>();
            var bar = new Mock<EventHandler>();

            eventDispatcher.AddListener("foo", foo.Object);
            eventDispatcher.AddListener("bar", bar.Object);

            Assert.IsTrue(eventDispatcher.HasListener("foo"));
            Assert.IsTrue(eventDispatcher.HasListener("bar"));
            Assert.IsFalse(eventDispatcher.HasListener("baz"));
        }

        [TestMethod]
        public void TestRepateAddSameListeners()
        {
            var foo = new Mock<EventHandler>();

            Assert.IsTrue(eventDispatcher.AddListener("foo", foo.Object));
            Assert.IsFalse(eventDispatcher.AddListener("foo", foo.Object));
        }

        [TestMethod]
        public void TestRemoveListeners()
        {
            var foo = new Mock<EventHandler>();
            var bar = new Mock<EventHandler>();
            var expected = new TestEventArgs();

            eventDispatcher.AddListener("foo", foo.Object);
            eventDispatcher.AddListener("bar", bar.Object);

            eventDispatcher.Dispatch("foo", this, expected);
            eventDispatcher.Dispatch("bar", this, expected);

            foo.Verify((o) => o.Invoke(this, expected));
            bar.Verify((o) => o.Invoke(this, expected));

            eventDispatcher.RemoveListener("foo", foo.Object);

            eventDispatcher.Dispatch("foo", this, expected);
            eventDispatcher.Dispatch("bar", this, expected);

            foo.Verify((o) => o.Invoke(this, expected), Times.Exactly(1));
            bar.Verify((o) => o.Invoke(this, expected), Times.Exactly(2));
        }

        [TestMethod]
        public void TestRemoveAllListeners()
        {
            var foo1 = new Mock<EventHandler>();
            var foo2 = new Mock<EventHandler>();
            var expected = new TestEventArgs();

            eventDispatcher.AddListener("foo", foo1.Object);
            eventDispatcher.AddListener("foo", foo2.Object);

            eventDispatcher.Dispatch("foo", this, expected);

            foo1.Verify((o) => o.Invoke(this, expected));
            foo2.Verify((o) => o.Invoke(this, expected));

            eventDispatcher.RemoveListener("foo");

            eventDispatcher.Dispatch("foo", this, expected);

            foo1.Verify((o) => o.Invoke(this, expected), Times.Exactly(1));
            foo2.Verify((o) => o.Invoke(this, expected), Times.Exactly(1));
        }

        [TestMethod]
        public void TestRemoveNotExistsListener()
        {
            var foo = new Mock<EventHandler>();
            Assert.IsFalse(eventDispatcher.RemoveListener("foo", foo.Object));
            Assert.IsFalse(eventDispatcher.RemoveListener("bar"));
        }

        [TestMethod]
        public void TestStoppableEvent()
        {
            var foo1 = new Mock<EventHandler>();
            var foo2 = new Mock<EventHandler>();
            var expected = new TestEventArgs();

            foo1.Setup((o) => o.Invoke(this, expected)).Callback(() =>
            {
                expected.StopPropagation();
            });

            eventDispatcher.AddListener("foo", foo1.Object);
            eventDispatcher.AddListener("foo", foo2.Object);

            eventDispatcher.Dispatch("foo", this, expected);

            foo1.Verify((o) => o.Invoke(this, expected));
            foo2.Verify((o) => o.Invoke(this, expected), Times.Never);
        }
    }
}
