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

namespace CatLib.Tests
{
    [TestClass]
    public class ContainerHelperTests
    {
        /// <summary>
        /// 生成服务和转为目标
        /// </summary>
        [TestMethod]
        public void MakeTConvert()
        {
            var container = MakeContainer();
            var obj = container.Make<ContainerHelperTests>("ContainerHelperTests");
            Assert.AreSame(this, obj);
        }

        /// <summary>
        /// 生成服务和转为目标
        /// </summary>
        [TestMethod]
        public void MakeTService()
        {
            var container = MakeContainer();
            var obj = container.Make<ContainerHelperTests>();
            Assert.AreSame(this, obj);
        }

        [TestMethod]
        public void MakeTypeService()
        {
            var container = MakeContainer();
            var obj = container.Make(typeof(ContainerHelperTests));
            Assert.AreSame(this, obj);
        }

        /// <summary>
        /// 以单例形式绑定
        /// </summary>
        [TestMethod]
        public void BindSingleton()
        {
            var container = MakeContainer();
            container.Singleton("BindSingleton", (c, param) =>
            {
                return new object();
            });
            var obj = container.Make("BindSingleton");
            Assert.AreSame(obj, container.Make("BindSingleton"));
        }

        public interface IContainerHelperTestClass
        {
            
        }

        public class ContainerHelperTestClass : IContainerHelperTestClass
        {

        }

        public class TestClassService
        {

        }

        /// <summary>
        /// 以单列形式绑定
        /// </summary>
        [TestMethod]
        public void BindSingletonTServiceTConcrete()
        {
            var container = MakeContainer();
            container.Singleton<IContainerHelperTestClass, ContainerHelperTestClass>();
            var obj = container.Make(container.Type2Service(typeof(IContainerHelperTestClass)));
            Assert.AreNotEqual(null, obj);
        }

        /// <summary>
        /// 以单列形式绑定
        /// </summary>
        [TestMethod]
        public void SingletonTService()
        {
            var container = MakeContainer();
            container.Singleton<TestClassService>((c, p) =>
            {
                return new object();
            });
            var obj = container.Make(container.Type2Service(typeof(TestClassService)));
            var obj2 = container.Make(container.Type2Service(typeof(TestClassService)));

            Assert.AreSame(obj, obj2);
        }

        [TestMethod]
        public void TestInstance()
        {
            var container = MakeContainer();
            var obj = new TestClassService();
            container.Instance<TestClassService>(obj);

            Assert.AreSame(obj, container.Make<TestClassService>());
        }

        [TestMethod]
        public void TestRelease()
        {
            var container = MakeContainer();
            var obj = new TestClassService();
            container.Instance<TestClassService>(obj);
            container.OnFindType((str) =>
            {
                return Type.GetType(str);
            });

            Assert.AreSame(obj, container.Make<TestClassService>());
            container.Release<TestClassService>();
            // 因为被释放后容器会容器会自动推测出所需类的实例
            Assert.AreSame(obj.GetType(), container.Make<TestClassService>().GetType());
        }

        [TestMethod]
        public void TestBindIf()
        {
            var app = new Application();
            IBindData bindData;
            Assert.AreEqual(true, App.BindIf("TestBind", (c, p) => 1, out bindData));
            Assert.AreEqual(false, App.BindIf("TestBind", (c, p) => 2, out bindData));
            Assert.AreEqual(1, app["TestBind"]);

            Assert.AreEqual(true, App.BindIf<object>(out bindData));
            Assert.AreEqual(typeof(object), app.Make<object>().GetType());

            Assert.AreEqual(true, App.BindIf<long>((c,p) => 100, out bindData));
            Assert.AreEqual(true, App.BindIf<int>(() => 100, out bindData));
            Assert.AreEqual(100, App.Make<int>());
            Assert.AreEqual(100, App.Make<int>()); // double get check
            Assert.AreEqual(true, App.BindIf<float, float>(out bindData));
            Assert.AreEqual(false, App.BindIf<float, float>(out bindData));

            Assert.AreEqual(typeof(float), App.Make<float>(App.Type2Service(typeof(float))).GetType());
        }

        [TestMethod]
        public void TestSingletonIf()
        {
            var testObject = new object();
            var testObject2 = new object();
            var app = new Application();
            IBindData bindData;
            Assert.AreEqual(true, App.SingletonIf("TestBind", (c, p) => new object(), out bindData));

            var makeObject = app["TestBind"];
            Assert.AreEqual(false, App.SingletonIf("TestBind", (c, p) => testObject2, out bindData));
            Assert.AreSame(testObject.GetType(), makeObject.GetType());
            Assert.AreSame(makeObject, app["TestBind"]);

            Assert.AreEqual(true, App.SingletonIf<object>(out bindData));
            Assert.AreEqual(typeof(object), app.Make<object>().GetType());

            Assert.AreEqual(true, App.SingletonIf<long>((c, p) => 100, out bindData));
            Assert.AreEqual(true, App.SingletonIf<int>(() => 100, out bindData));
            Assert.AreEqual(100, App.Make<int>());
            Assert.AreEqual(true, App.SingletonIf<float, float>(out bindData));
            Assert.AreEqual(false, App.SingletonIf<float, float>(out bindData));

            Assert.AreEqual(typeof(float), App.Make<float>(App.Type2Service(typeof(float))).GetType());
        }

        [TestMethod]
        public void TestGetBind()
        {
            var container = new Container();
            var bind = container.Bind<IApplication>(() => "helloworld");

            Assert.AreEqual("helloworld", container.Make(container.Type2Service<IApplication>()));
            Assert.AreEqual(true, container.HasBind<IApplication>());
            Assert.AreSame(bind, container.GetBind<IApplication>());
        }

        [TestMethod]
        public void TestCanMake()
        {
            var container = new Container();

            Assert.AreEqual(false, container.CanMake<IApplication>());
            container.Bind<IApplication>(() => "helloworld");
            Assert.AreEqual(true, container.CanMake<IApplication>());
        }

        [TestMethod]
        public void TestIsStatic()
        {
            var container = new Container();
            container.Bind<IApplication>(() => "helloworld");
            Assert.AreEqual(false, container.IsStatic<IApplication>());
            container.Unbind<IApplication>();
            container.Singleton<IApplication>(() => "helloworld");
            Assert.AreEqual("helloworld", container.Make(container.Type2Service<IApplication>()));
            Assert.AreEqual(true, container.IsStatic<IApplication>());
        }

        [TestMethod]
        public void TestIsAlias()
        {
            var container = new Container();
            container.Bind<IApplication>(() => "helloworld").Alias<IContainer>();
            Assert.AreEqual(false, container.IsAlias<IApplication>());
            Assert.AreEqual(true, container.IsAlias<IContainer>());
        }

        [TestMethod]
        public void TestGetService()
        {
            var container = new Container();
            Assert.AreEqual(container.Type2Service(typeof(string)), container.Type2Service<string>());
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

        public interface IWatchTest
        {
            int getValue();
        }

        public class TestData : IWatchTest
        {
            private int val;
            public TestData(int val)
            {
                this.val = val;
            }
            public int getValue()
            {
                return val;
            }
        }

        [TestMethod]
        public void TestWatchLambda()
        {
            var container = new Container();
            container.Instance<IContainer>(container);

            var isCall = false;
            container.Instance<IWatchTest>(new TestData(100));
            container.Watch<IWatchTest>((val) =>
            {
                isCall = true;
                Assert.AreEqual(200, val.getValue());
            });
            container.Instance<IWatchTest>(new TestData(200));

            Assert.AreEqual(true, isCall);
            Assert.AreEqual(true, isCall);
        }

        [TestMethod]
        public void TestWatchLambdaNoParam()
        {
            var container = new Container();
            container.Instance<IContainer>(container);

            var isCall = false;
            container.Instance<IWatchTest>(100);
            container.Watch<IWatchTest>(() =>
            {
                isCall = true;
            });
            container.Instance<IWatchTest>(200);

            Assert.AreEqual(true, isCall);
        }

        [TestMethod]
        public void TestReleaseWithObject()
        {
            var container = new Container();
            container.Instance<string>("abc");
            container.Instance<int>(10);
            object[] data = null;
            Assert.AreEqual(true, container.Release(ref data));

            data = new object[0];
            Assert.AreEqual(true, container.Release(ref data));

            data = new object[] {"abc", 10};
            Assert.AreEqual(true, container.Release(ref data));
            Assert.AreEqual(true, data.Length == 0);

            data = new object[] { "abc", 10, 998 };
            Assert.AreEqual(false, container.Release(ref data));
            Assert.AreEqual(true, data.Length == 3);

            container.Instance<int>(10);
            data = new object[] { "abc", 10, 998 };
            Assert.AreEqual(false, container.Release(ref data, false));
            Assert.AreEqual(true, data.Length == 2);
            Assert.AreEqual("abc", data[0]);
            Assert.AreEqual(998, data[1]);

            container.Instance<string>("abc");
            data = new object[] { 10 , "abc", 998 };
            Assert.AreEqual(false, container.Release(ref data));
            Assert.AreEqual(true, data.Length == 2);
            Assert.AreEqual(10, data[0]);
            Assert.AreEqual(998, data[1]);
        }

        [TestMethod]
        public void TestSetAlias()
        {
            var container = new Container();
            container.Instance<object>("abc");
            container.Alias<string, object>();

            Assert.AreEqual("abc", container.Make<string>());
        }

        public class TestOnResolvingClass
        {
            public string Name;
            public TestOnResolvingClass()
            {
                
            }
        }

        [TestMethod]
        public void TestOnResolving()
        {
            var container = new Container();
            container.Singleton<TestOnResolvingClass>().OnResolving((instance) =>
            {
                var cls = (instance) as TestOnResolvingClass;
                Assert.AreEqual(null, cls.Name);
                cls.Name = "123";
            });

            container.OnResolving((instance) =>
            {
                var cls = (instance) as TestOnResolvingClass;
                Assert.AreEqual("123", cls.Name);
                cls.Name = "222";
            });

            Assert.AreEqual("222", container.Make<TestOnResolvingClass>().Name);
        }

        [TestMethod]
        public void TestExtend()
        {
            var container = new Container();
            container.Extend<string>((instance) => instance + " world");
            container.Bind<string>(() => "hello");
            Assert.AreEqual("hello world", container.Make<string>());
        }

        [TestMethod]
        public void TestExtendContainer()
        {
            var container = new Container();
            container.Extend<string>((instance, _) =>
            {
                Assert.AreSame(container, _);
                return instance + " world";
            });
            container.Bind<string>(() => "hello");
            Assert.AreEqual("hello world", container.Make<string>());
        }

        [TestMethod]
        public void TestExtendContainerWithService()
        {
            var container = new Container();
            container.Extend("abc", (instance, _) =>
            {
                Assert.AreSame(container, _);
                return instance + " world";
            });

            container.Bind("abc", (b, p) => "hello", false);
            Assert.AreEqual("hello world", container.Make("abc"));
        }

        [TestMethod]
        public void TestExtendContainerWithService2()
        {
            var container = new Container();
            container.Extend("abc", (instance) => instance + " world");

            container.Bind("abc", (b, p) => "hello", false);
            Assert.AreEqual("hello world", container.Make("abc"));
        }

        [TestMethod]
        public void TestExtendWithServiceName()
        {
            var container = new Container();
            container.Extend<ITypeMatchInterface, TestTypeMatchOnResolvingClass>((instance) => null);
            container.Bind<ITypeMatchInterface, TestTypeMatchOnResolvingClass>();

            Assert.AreEqual(null, container.Make<ITypeMatchInterface>());
        }

        [TestMethod]
        public void TestExtendWithServiceName2()
        {
            var container = new Container();
            container.Extend<ITypeMatchInterface, TestTypeMatchOnResolvingClass>((instance, c) =>
            {
                Assert.AreEqual(container, c);
                return null;
            });
            container.Bind<ITypeMatchInterface, TestTypeMatchOnResolvingClass>();
            Assert.AreEqual(null, container.Make<ITypeMatchInterface>());
        }

        public interface ITypeMatchInterface
        {
            
        }

        public class TestTypeMatchOnResolvingClass : ITypeMatchInterface
        {
            
        }

        [TestMethod]
        public void TestTypeMatchOnResolving()
        {
            var container = new Container();
            container.Bind("hello", (_, __) => new TestTypeMatchOnResolvingClass());
            container["world"] = "hello";
            var count = 0;
            container.OnResolving<ITypeMatchInterface>((instance) =>
            {
                count++;
            });

            container.OnResolving<ITypeMatchInterface>((bindData, instance) =>
            {
                Assert.AreNotEqual(null, bindData);
                count++;
            });

            container.OnAfterResolving<ITypeMatchInterface>((instance) =>
            {
                count++;
            });

            container.OnAfterResolving<ITypeMatchInterface>((bindData,instance) =>
            {
                Assert.AreNotEqual(null, bindData);
                count++;
            });

            container.Make("hello");
            container.Make("world");

            Assert.AreEqual(4, count);
        }

        [TestMethod]
        public void TestTypeMatchOnRelease()
        {
            var container = new Container();
            container.Singleton("hello", (_, __) => new TestTypeMatchOnResolvingClass());
            container.Singleton("world", (_, __) => "hello");
            var count = 0;
            var stringCount = 0;
            container.OnRelease<ITypeMatchInterface>((instance) =>
            {
                count++;
            });

            container.OnRelease<ITypeMatchInterface>((bindData, instance) =>
            {
                Assert.AreNotEqual(null, bindData);
                count++;
            });

            container.OnRelease<string>((instance) =>
            {
                stringCount++;
            });

            container.OnRelease<string>((bindData, instance) =>
            {
                Assert.AreNotEqual(null, bindData);
                stringCount++;
            });

            container.Make("hello");
            container.Make("world");

            container.Release("hello");
            container.Release("world");

            Assert.AreEqual(2, count);
            Assert.AreEqual(2, stringCount);
        }

        [TestMethod]
        public void TestBindFunc2()
        {
            var container = new Container();
            var obj = new object();
            container.Bind<IAwait>((p) => (bool) p[0] ? obj : new object()).Alias("created");
            Assert.AreSame(obj, container.Make("created", true));
            Assert.AreNotSame(obj, container.Make("created", false));
        }

        [TestMethod]
        public void TestSingletonFunc2()
        {
            var container = new Container();
            var obj = new object();
            container.Singleton<IAwait>((p) => (bool)p[0] ? obj : new object()).Alias("created");
            Assert.AreSame(obj, container.Make("created", true));
            Assert.AreSame(obj, container.Make("created", false));
        }

        [TestMethod]
        public void TestBindIfFunc2()
        {
            var container = new Container();
            var obj = new object();
            Assert.AreEqual(true, container.BindIf<IAwait>((p) => (bool)p[0] ? obj : new object(), out IBindData bindData));
            Assert.AreEqual(false, container.BindIf<IAwait>((p) => null, out bindData));
            bindData.Alias("created");
            Assert.AreSame(obj, container.Make("created", true));
            Assert.AreNotSame(obj, container.Make("created", false));
        }

        [TestMethod]
        public void TestSingletonIfFunc2()
        {
            var container = new Container();
            var obj = new object();
            Assert.AreEqual(true, container.SingletonIf<IAwait>((p) => (bool)p[0] ? obj : new object(), out IBindData bindData));
            Assert.AreEqual(false, container.SingletonIf<IAwait>((p) => null, out bindData));
            bindData.Alias("created");
            Assert.AreSame(obj, container.Make("created", true));
            Assert.AreSame(obj, container.Make("created", false));
        }

        /// <summary>
        /// 生成容器
        /// </summary>
        /// <returns>容器</returns>
        private Container MakeContainer()
        {
            var container = new Container();
            container.Instance("ContainerHelperTests", this);
            container.Alias(container.Type2Service(typeof(ContainerHelperTests)), "ContainerHelperTests");
            return container;
        }
    }
}
