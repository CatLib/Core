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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using Dispatcher = CatLib.EventDispatcher.EventDispatcher;

namespace CatLib.EventDispatcher.Tests
{
    [TestClass]
    public class TestsEventDispatcher
    {
        internal interface IResponse<T>
        {
            void Foo(T eventArgs);
        }

        [TestMethod]
        public void TestDispatch()
        {
            var eventDispatcher = new Dispatcher();
            var expected = new TestEventArgs();
            var mock = new Mock<IResponse<TestEventArgs>>();
            eventDispatcher.AddListener<TestEventArgs>(mock.Object.Foo);
            eventDispatcher.Dispatch(expected);
            mock.Verify((o) => o.Foo(expected));
        }

        [TestMethod]
        public void TestAddListener()
        {
            var eventDispatcher = new Dispatcher();
            var first = new Mock<IResponse<TestEventArgs>>();
            var second = new Mock<IResponse<TestEventArgs>>();

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
            eventDispatcher.AddListener<TestEventArgs>(second.Object.Foo);

            Assert.AreEqual(2, eventDispatcher.GetListeners<TestEventArgs>().Count());
        }

        [TestMethod]
        public void TestAddListenerBySort()
        {
            var eventDispatcher = new Dispatcher();
            var first = new Mock<IResponse<TestEventArgs>>();
            var second = new Mock<IResponse<TestEventArgs>>();
            var third = new Mock<IResponse<TestEventArgs>>();

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo, 10);
            eventDispatcher.AddListener<TestEventArgs>(second.Object.Foo, -10);
            eventDispatcher.AddListener<TestEventArgs>(third.Object.Foo);

            CollectionAssert.AreEqual(
                new Action<TestEventArgs>[]
                {
                    second.Object.Foo, third.Object.Foo, first.Object.Foo,
                },
                eventDispatcher.GetListeners<TestEventArgs>().ToList());
        }

        [TestMethod]
        public void TestHasListener()
        {
            var eventDispatcher = new Dispatcher();
            var first = new Mock<IResponse<TestEventArgs>>();
            var second = new Mock<IResponse<BaseTestEventArgs>>();

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
            eventDispatcher.AddListener<BaseTestEventArgs>(second.Object.Foo);

            Assert.IsTrue(eventDispatcher.HasListeners<TestEventArgs>());
            Assert.IsTrue(eventDispatcher.HasListeners<BaseTestEventArgs>());
        }

        [TestMethod]
        public void TestDispatchForInheritanceChain()
        {
            var eventDispatcher = new Dispatcher();
            var first = new Mock<IResponse<TestEventArgs>>();
            var second = new Mock<IResponse<BaseTestEventArgs>>();
            var third = new Mock<IResponse<EventArgs>>();
            var expected = new TestEventArgs();

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
            eventDispatcher.AddListener<BaseTestEventArgs>(second.Object.Foo);
            eventDispatcher.AddListener<EventArgs>(third.Object.Foo);

            eventDispatcher.Dispatch(expected);

            first.Verify((o) => o.Foo(expected));
            second.Verify((o) => o.Foo(expected));
            third.Verify((o) => o.Foo(expected));

            var expectedBase = new BaseTestEventArgs();

            eventDispatcher.Dispatch(expectedBase);
            second.Verify((o) => o.Foo(expectedBase));
            third.Verify((o) => o.Foo(expectedBase));
        }

        [TestMethod]
        public void TestDispatchForInheritanceChainDisabled()
        {
            var eventDispatcher = new Dispatcher(false);
            var first = new Mock<IResponse<TestEventArgs>>();
            var second = new Mock<IResponse<BaseTestEventArgs>>();
            var third = new Mock<IResponse<EventArgs>>();
            var expected = new TestEventArgs();

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
            eventDispatcher.AddListener<BaseTestEventArgs>(second.Object.Foo);
            eventDispatcher.AddListener<EventArgs>(third.Object.Foo);

            eventDispatcher.Dispatch(expected);

            first.Verify((o) => o.Foo(expected));
            second.Verify((o) => o.Foo(expected), Times.Never);
            third.Verify((o) => o.Foo(expected), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeException))]
        public void TestRepateAddSameListeners()
        {
            var eventDispatcher = new Dispatcher(false);
            var first = new Mock<IResponse<TestEventArgs>>();

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
        }

        [TestMethod]
        public void TestRemoveListeners()
        {
            var eventDispatcher = new Dispatcher();
            var first = new Mock<IResponse<TestEventArgs>>();
            var second = new Mock<IResponse<EventArgs>>();
            var expected = new TestEventArgs();

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
            eventDispatcher.AddListener<EventArgs>(second.Object.Foo);

            eventDispatcher.Dispatch(expected);

            first.Verify((o) => o.Foo(expected));
            second.Verify((o) => o.Foo(expected));

            eventDispatcher.RemoveListener<TestEventArgs>(first.Object.Foo);

            eventDispatcher.Dispatch(expected);

            first.Verify((o) => o.Foo(expected), Times.Exactly(1));
            second.Verify((o) => o.Foo(expected), Times.Exactly(2));
        }

        [TestMethod]
        public void TestRemoveAllListeners()
        {
            var eventDispatcher = new Dispatcher();
            var first = new Mock<IResponse<TestEventArgs>>();
            var second = new Mock<IResponse<TestEventArgs>>();
            var expected = new TestEventArgs();

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
            eventDispatcher.AddListener<TestEventArgs>(second.Object.Foo);

            eventDispatcher.Dispatch(expected);

            first.Verify((o) => o.Foo(expected));
            second.Verify((o) => o.Foo(expected));

            eventDispatcher.RemoveListener<TestEventArgs>();

            eventDispatcher.Dispatch(expected);

            first.Verify((o) => o.Foo(expected), Times.Exactly(1));
            second.Verify((o) => o.Foo(expected), Times.Exactly(1));
        }

        [TestMethod]
        public void TestRemoveNotExistsListener()
        {
            var eventDispatcher = new Dispatcher();
            var first = new Mock<IResponse<TestEventArgs>>();
            eventDispatcher.RemoveListener<TestEventArgs>(first.Object.Foo);
        }

        [TestMethod]
        public void TestStoppableEvent()
        {
            var eventDispatcher = new Dispatcher();
            var first = new Mock<IResponse<TestEventArgs>>();
            var second = new Mock<IResponse<TestEventArgs>>();
            var expected = new TestEventArgs();

            second.Setup((o) => o.Foo(expected)).Callback(() =>
            {
                expected.Stop();
            });

            eventDispatcher.AddListener<TestEventArgs>(first.Object.Foo);
            eventDispatcher.AddListener<TestEventArgs>(second.Object.Foo);

            eventDispatcher.Dispatch(expected);

            first.Verify((o) => o.Foo(expected), Times.Never);
            second.Verify((o) => o.Foo(expected));
        }

        internal class BaseTestEventArgs : EventArgs
        {
            public string Name { get; set; } = "foo";
        }

        internal class TestEventArgs : BaseTestEventArgs, IStoppableEvent
        {
            private bool isPropagationStopped;

            public object Payload { get; set; }

            public bool IsPropagationStopped()
            {
                return isPropagationStopped;
            }

            public void Stop()
            {
                isPropagationStopped = true;
            }
        }
    }
}
