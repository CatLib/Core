/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Stl
{
    /// <summary>
    /// 给与数据测试用例
    /// </summary>
    [TestClass]
    public class GivenDataTest
    {
        /// <summary>
        /// 可以给与数据
        /// </summary>
        [TestMethod]
        public void CanGiven()
        {
            var container = new Container();
            var bindData = new BindData(container, "CanGiven", (app, param) => "hello world", false);
            var givenData = new GivenData<IBindData>(container, bindData);
            givenData.Needs("needs1");
            givenData.Given("hello");
            Assert.AreEqual("hello", bindData.GetContextual("needs1"));

            givenData = new GivenData<IBindData>(container, bindData);
            givenData.Needs("needs2");
            givenData.Given<GivenDataTest>();
            Assert.AreEqual(container.Type2Service(typeof(GivenDataTest)), bindData.GetContextual("needs2"));
        }

        /// <summary>
        /// 检查给与的无效值
        /// </summary>
        [TestMethod]
        public void CheckGivenIllegalValue()
        {
            var container = new Container();
            var bindData = new BindData(container, "CanGiven", (app, param) => "hello world", false);
            var givenData = new GivenData<IBindData>(container, bindData);
            givenData.Needs("needs");

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                givenData.Given(string.Empty);
            });
        }


        private class TestGivenClosureClass
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public TestGivenClosureClass(string name, int value = 0)
            {
                Name = name;
                Value = value;
            }
        }

        [TestMethod]
        public void TestGivenClosure()
        {
            var container = new Container();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given(() => "hello world")
                .Needs<int>().Given(()=> 10);

            Assert.AreEqual("hello world", container.Make<TestGivenClosureClass>().Name);
            Assert.AreEqual(10, container.Make<TestGivenClosureClass>().Value);
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestGivenDuplicateValue()
        {
            var container = new Container();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given(() => "hello world");
            container.Bind<TestGivenClosureClass>().Needs<string>().Given(() => "ddd");
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestGivenDuplicateValue2()
        {
            var container = new Container();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given(() => "hello world");
            container.Bind<TestGivenClosureClass>().Needs<string>().Given<int>();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestGivenDuplicateValue3()
        {
            var container = new Container();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given<int>();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given(() => "hello world");
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestGivenDuplicateValue4()
        {
            var container = new Container();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given<int>();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given<long>();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestGivenDuplicateValue5()
        {
            var container = new Container();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given(() => "hello world");
            container.Bind<TestGivenClosureClass>().Needs<long>().Given<long>();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given<long>();
        }

        [TestMethod]
        [ExpectedException(typeof(LogicException))]
        public void TestGivenDuplicateValue6()
        {
            var container = new Container();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given<int>();
            container.Bind<TestGivenClosureClass>().Needs<long>().Given(() => "hello world");
            container.Bind<TestGivenClosureClass>().Needs<string>().Given(() => "hello world");
        }

        [TestMethod]
        [ExpectedException(typeof(AssertException))]
        public void TestGivenClosureException()
        {
            var container = new Container();
            container.Bind<TestGivenClosureClass>().Needs<string>().Given(() =>
            {
                throw new AssertException("hello world");
            });

            container.Make<TestGivenClosureClass>();
        }

        private class TestGivenClosureAttrClass
        {
            [Inject]
            public string Name { get; set; }

            [Inject]
            public int Value { get; set; }
        }

        [TestMethod]
        public void TestGivenAttrClosure()
        {
            var container = new Container();
            container.Bind<TestGivenClosureAttrClass>().Needs<string>().Given(() => "hello world")
                .Needs<int>().Given(() => 10);

            Assert.AreEqual("hello world", container.Make<TestGivenClosureAttrClass>().Name);
            Assert.AreEqual(10, container.Make<TestGivenClosureAttrClass>().Value);
        }
    }
}