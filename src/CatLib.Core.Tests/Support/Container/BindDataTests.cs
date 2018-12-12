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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Stl
{
    /// <summary>
    /// 绑定数据测试用例
    /// </summary>
    [TestClass]
    public class BindDataTest
    {
        #region Needs
        /// <summary>
        /// 需要什么样的数据不为空
        /// </summary>
        [TestMethod]
        public void CheckNeedsIsNotNull()
        {
            var container = new Container();
            var bindData = new BindData(container, "NeedsIsNotNull", (app, param) => "hello world", false);

            var needs = bindData.Needs("TestService");
            var needsWithType = bindData.Needs<BindDataTest>();

            Assert.AreNotEqual(null, needs);
            Assert.AreNotEqual(null, needsWithType);
        }

        /// <summary>
        /// 检测当需求什么方法时传入无效参数
        /// </summary>
        [TestMethod]
        public void CheckNeedsIllegalValue()
        {
            var container = new Container();
            var bindData = new BindData(container, "CheckNeedsIllegalValue", (app, param) => "hello world", false);

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                bindData.Needs(null);
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                bindData.Needs(string.Empty);
            });
        }

        /// <summary>
        /// 是否可以取得关系上下文
        /// </summary>
        [TestMethod]
        public void CanGetContextual()
        {
            var container = new Container();
            var bindData = new BindData(container, "NeedsIsNotNull", (app, param) => "hello world", false);

            bindData.Needs("need1").Given("abc");
            bindData.Needs("need2").Given<BindDataTest>();

            Assert.AreEqual("abc", bindData.GetContextual("need1"));
            Assert.AreEqual(container.Type2Service(typeof(BindDataTest)), bindData.GetContextual("need2"));
            Assert.AreEqual("empty", bindData.GetContextual("empty"));
        }
        #endregion

        #region Alias
        /// <summary>
        /// 是否能够增加别名
        /// </summary>
        [TestMethod]
        public void CanAddAlias()
        {
            var container = new Container();
            var bindData = container.Bind("CanAddAlias", (app, param) => "hello world", false);

            bindData.Alias("Alias");
            bindData.Alias<BindDataTest>();

            var textAliasGet = container.GetBind("Alias");
            Assert.AreSame(textAliasGet, bindData);

            var classAliasGet = container.GetBind(container.Type2Service(typeof(BindDataTest)));
            Assert.AreSame(bindData, textAliasGet);
            Assert.AreSame(bindData, classAliasGet);
        }

        /// <summary>
        /// 检测无效的别名
        /// </summary>
        [TestMethod]
        public void CheckIllegalAlias()
        {
            var container = new Container();
            var bindData = new BindData(container, "CheckIllegalAlias", (app, param) => "hello world", false);

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                bindData.Alias(null);
            });
            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                bindData.Alias(string.Empty);
            });
        }
        #endregion

        #region Tag
        /// <summary>
        /// 是否能增加标签
        /// </summary>
        [TestMethod]
        public void CanAddTag()
        {
            var container = new Container();
            var bindData = container.Bind("data", (app, param) => "hello world", false);

            bindData.Tag("tag1").Tag("tag2");

            var data = container.Tagged("tag1");
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual("hello world", data[0]);

            data = container.Tagged("tag2");
            Assert.AreEqual(1, data.Length);
            Assert.AreEqual("hello world", data[0]);
        }
        #endregion

        #region OnRelease
        /// <summary>
        /// 是否能追加到释放事件
        /// </summary>
        [TestMethod]
        public void CanOnRelease()
        {
            var container = new Container();
            var bindData = container.Bind("CanAddOnRelease", (app, param) => "hello world", true);

            bindData.OnRelease((bind, obj) =>
            {
                Assert.AreEqual("Test", obj);
                Assert.AreSame(bindData, bind);
            });

            // double check
            bindData.OnRelease((obj) =>
            {
                Assert.AreEqual("Test", obj);
            });

            var isCall = false;
            bindData.OnRelease(()=>
            {
                isCall = true;
            });

            container.Instance("CanAddOnRelease", "Test");
            container.Release("CanAddOnRelease");
            Assert.AreEqual(true, isCall);
        }
        /// <summary>
        /// 检查无效的解决事件传入参数
        /// </summary>
        [TestMethod]
        public void CheckIllegalRelease()
        {
            var container = new Container();
            var bindData = container.Bind("CheckIllegalRelease", (app, param) => "hello world", true);

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                bindData.OnRelease(null);
            });

            bindData.Unbind();
            bindData = container.Bind("CheckIllegalRelease", (app, param) => "hello world", false);
            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                bindData.OnRelease((obj) =>
                {
                    Assert.Fail();
                });
                container.Instance("CheckIllegalRelease", "Test");
                container.Release("CheckIllegalRelease");
            });
        }

        #endregion

        #region OnResolving

        [TestMethod]
        public void TestAddOnResolvingWithExtend()
        {
            var container = new Container();
            var bindData = new BindData(container, "CanAddOnResolving", (app, param) => "hello world", false);
            bindData.OnResolving((obj) => Assert.AreEqual("hello world", obj));
            var data = bindData.TriggerResolving("hello world");
            Assert.AreEqual("hello world", data);
        }

        [TestMethod]
        public void TestAddOnResolvingWithExtendNoneInstance()
        {
            var container = new Container();
            var bindData = new BindData(container, "CanAddOnResolving", (app, param) => "hello world", false);
            var call = false;
            bindData.OnResolving(() =>
            {
                call = true;
            });
            var data = bindData.TriggerResolving("hello world");
            Assert.AreEqual("hello world", data);
            Assert.AreEqual(true, call);
        }

        /// <summary>
        /// 检查无效的解决事件传入参数
        /// </summary>
        [TestMethod]
        public void CheckIllegalResolving()
        {
            var container = new Container();
            var bindData = new BindData(container, "CanAddOnResolving", (app, param) => "hello world", false);

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                bindData.OnResolving(null);
            });
        }

        [TestMethod]
        public void TestTypeMatchOnResolving()
        {
            var container = new Container();
            var count = 0;
            container.Bind("hello", (_, __) => new ContainerHelperTests.TestTypeMatchOnResolvingClass())
                .OnResolving<ContainerHelperTests.ITypeMatchInterface>((instance) =>
                {
                    count++;
                }).OnResolving<ContainerHelperTests.ITypeMatchInterface>((bindData, instance) =>
                {
                    Assert.AreNotEqual(null, bindData);
                    count++;
                }).OnAfterResolving<ContainerHelperTests.ITypeMatchInterface>((instance) =>
                {
                    count++;
                }).OnAfterResolving<ContainerHelperTests.ITypeMatchInterface>((bindData, instance) =>
                {
                    Assert.AreNotEqual(null, bindData);
                    count++;
                }).OnAfterResolving<Container>((instance) =>
                {
                    count++;
                });

            container.Make("hello");

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void TestTypeMatchOnRelease()
        {
            var container = new Container();
            var count = 0;
            container.Singleton("hello", (_, __) => new ContainerHelperTests.TestTypeMatchOnResolvingClass())
                .OnRelease<ContainerHelperTests.ITypeMatchInterface>((instance) =>
                {
                    count++;
                }).OnRelease<Container>((instance) =>
                {
                    count++;
                }).OnRelease<ContainerHelperTests.ITypeMatchInterface>((bindData,instance) =>
                {
                    Assert.AreNotEqual(null, bindData);
                    count++;
                }).OnRelease<Container>((bindData, instance) =>
                {
                    Assert.AreNotEqual(null, bindData);
                    count++;
                }).OnRelease<string>((bindData, instance) =>
                {
                    Assert.AreNotEqual(null, bindData);
                    count++;
                });
            container.Singleton("world", (_, __) => "hello");

            container.Make("hello");

            container.Release("hello");

            Assert.AreEqual(2, count);
        }
        #endregion

        #region Unbind
        /// <summary>
        /// 能够正常解除绑定
        /// </summary>
        [TestMethod]
        public void CanUnBind()
        {
            var container = new Container();
            var bindData = container.Bind("CanUnBind", (app, param) => "hello world", false);

            Assert.AreEqual("hello world", container.Make("CanUnBind").ToString());
            bindData.Unbind();

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make("CanUnBind");
            });
        }

        /// <summary>
        /// 能够正常解除绑定
        /// </summary>
        [TestMethod]
        public void CheckIllegalUnBindInput()
        {
            var container = new Container();
            var bindData = container.Bind("CanUnBind", (app, param) => "hello world", false);
            bindData.Unbind();

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                bindData.Alias("hello");
            });
        }
        #endregion

        #region AddContextual

        /// <summary>
        /// 重复写入上下文
        /// </summary>
        [TestMethod]
        public void AddContextualRepeat()
        {
            var container = new Container();
            var bindData = new BindData(container, "AddContextualRepeat", (app, param) => "hello world", false);

            bindData.AddContextual("service", "service given");
            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                bindData.AddContextual("service", "service given");
            });
        }

        #endregion
    }
}