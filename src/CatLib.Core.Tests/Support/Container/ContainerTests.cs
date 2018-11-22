/*
 * This file is part of the CatLib package.
 *
 * (c) Yu Bin <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: http://catlib.io/
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Stl
{
    /// <summary>
    /// 容器测试用例
    /// </summary>
    [TestClass]
    public class ContainerTest
    {
        public class InnerExceptionClass
        {
            public InnerExceptionClass()
            {
                throw new ArgumentException("Exception in InnerExceptionClass");
            }
        }

        public class TestExceptionClass
        {
            public TestExceptionClass()
            {
                new InnerExceptionClass();
            }
        }

        [TestMethod]
        public void TestInnerException()
        {
            var container = MakeContainer();
            container.Bind("TestExceptionClass", typeof(TestExceptionClass), false);

            var isThrow = false;
            try
            {
                container.Make("TestExceptionClass");
            }
            catch (Exception ex)
            {
                isThrow = true;
                Console.WriteLine(ex.Message);
            }

            Assert.AreEqual(true, isThrow);
        }

        #region Tag
        /// <summary>
        /// 是否可以标记服务
        /// </summary>
        [TestMethod]
        public void CanTagService()
        {
            var container = MakeContainer();
            ExceptionAssert.DoesNotThrow(() =>
            {
                container.Tag("TestTag", "service1", "service2");
                container.Tag<ContainerTest>("TestTag");
            });
        }

        /// <summary>
        /// 检测无效的Tag输入
        /// </summary>
        [TestMethod]
        public void CheckIllegalTagInput()
        {
            var container = MakeContainer();
            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Tag("TestTag");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Tag("TestTag", null);
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Tag(null, "service1", "service2");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Tag(string.Empty, "service1", "service2");
            });
        }

        /// <summary>
        /// 是否可以根据标签生成服务
        /// </summary>
        [TestMethod]
        public void CanMakeWithTaged()
        {
            var container = MakeContainer();
            container.Bind("TestService1", (app, param) => "hello");
            container.Bind("TestService2", (app, param) => "world");

            container.Tag("TestTag", "TestService1", "TestService2");

            ExceptionAssert.DoesNotThrow(() =>
            {
                var obj = container.Tagged("TestTag");
                Assert.AreEqual(2, obj.Length);
                Assert.AreEqual("hello", obj[0]);
                Assert.AreEqual("world", obj[1]);
            });
        }

        [TestMethod]
        public void TestUnbind()
        {
            var container = MakeContainer();
            container.Bind("TestService1", (app, param) => "hello");
            container.Bind("TestService2", (app, param) => "world").Alias<IBindData>();

            container.Unbind("TestService1");
            container.Unbind<IBindData>();

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make("TestService1");
            });

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make("TestService2");
            });

            container.Bind("TestService2", (app, param) => "hello");
            Assert.AreEqual("hello", container["TestService2"]);
        }

        /// <summary>
        /// 测试不存在的Tag
        /// </summary>
        [TestMethod]
        public void CheckNotExistTaged()
        {
            var container = MakeContainer();
            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Tagged("TestTag");
            });
        }

        /// <summary>
        /// 合并标记
        /// </summary>
        [TestMethod]
        public void MergeTag()
        {
            var container = MakeContainer();
            container.Tag("hello", "world");
            container.Tag("hello", "world2");

            container.Bind("world", (c, p) => "hello");
            container.Bind("world2", (c, p) => "world");

            Assert.AreEqual(2, container.Tagged("hello").Length);
            Assert.AreEqual("hello", container.Tagged("hello")[0]);
            Assert.AreEqual("world", container.Tagged("hello")[1]);
        }

        /// <summary>
        /// 空服务测试
        /// </summary>
        [TestMethod]
        public void NullTagService()
        {
            var container = MakeContainer();
            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Tag("hello", "world", null);
            });
        }

        #endregion

        #region Bind
        /// <summary>
        /// 测试无法被绑定的类型
        /// </summary>
        [TestMethod]
        public void TestBindUnableBuilt()
        {
            var container = MakeContainer();

            IBindData binder;
            Assert.AreEqual(false, container.BindIf<IContainer>(out binder));

            var isError = false;
            try
            {
                container.Bind<string[]>();
            }
            catch (RuntimeException)
            {
                isError = true;
            }
            Assert.AreEqual(true, isError);
        }

        /// <summary>
        /// 是否能够进行如果不存在则绑定的操作
        /// </summary>
        [TestMethod]
        public void CanBindIf()
        {
            var container = MakeContainer();
            IBindData bind1, bind2;
            var result1 = container.BindIf("CanBindIf", (cont, param) => "Hello", true, out bind1);
            var result2 = container.BindIf("CanBindIf", (cont, param) => "World", false, out bind2);

            Assert.AreSame(bind1, bind2);
            Assert.AreEqual(true, result1);
            Assert.AreEqual(false, result2);
        }

        /// <summary>
        /// 是否能够进行如果不存在则绑定的操作
        /// </summary>
        [TestMethod]
        public void CanBindIfByType()
        {
            var container = MakeContainer();
            IBindData bind1, bind2;
            var result1 = container.BindIf("CanBindIf", typeof(ContainerTest), true, out bind1);
            var result2 = container.BindIf("CanBindIf", typeof(ContainerTest), false, out bind2);

            Assert.AreSame(bind1, bind2);
            Assert.AreEqual(true, result1);
            Assert.AreEqual(false, result2);
        }

        /// <summary>
        /// 检测无效的绑定
        /// </summary>
        [TestMethod]
        public void CheckIllegalBind()
        {
            var container = MakeContainer();
            container.Bind("CheckIllegalBind", (cont, param) => "HelloWorld", true);

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Bind("CheckIllegalBind", (cont, param) => "Repeat Bind");
            });

            container.Instance("InstanceBind", "hello world");

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Bind("InstanceBind", (cont, param) => "Instance Repeat Bind");
            });

            container.Alias("Hello", "CheckIllegalBind");

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Bind("Hello", (cont, param) => "Alias Repeat Bind");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Bind(string.Empty, (cont, param) => "HelloWorld");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Bind(null, (cont, param) => "HelloWorld");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Bind("NoConcrete", null);
            });
        }

        /// <summary>
        /// 静态绑定方法
        /// </summary>
        [TestMethod]
        public void CanBindFuncStatic()
        {
            var container = MakeContainer();
            container.Bind("CanBindFuncStatic", (cont, param) => "HelloWorld", true);

            var bind = container.GetBind("CanBindFuncStatic");
            var hasBind = container.HasBind("CanBindFuncStatic");
            var obj = container.Make("CanBindFuncStatic");

            Assert.AreNotEqual(null, bind);
            Assert.AreEqual(true, hasBind);
            Assert.AreEqual(true, bind.IsStatic);
            Assert.AreSame("HelloWorld", obj);
        }

        /// <summary>
        /// 非静态绑定
        /// </summary>
        [TestMethod]
        public void CanBindFunc()
        {
            var container = MakeContainer();
            container.Bind("CanBindFunc", (cont, param) => new List<string>());

            var bind = container.Make("CanBindFunc");
            var bind2 = container.Make("CanBindFunc");

            Assert.AreNotEqual(null, bind);
            Assert.AreNotSame(bind, bind2);
        }

        /// <summary>
        /// 检测获取绑定
        /// </summary>
        [TestMethod]
        public void CanGetBind()
        {
            var container = MakeContainer();
            var bind = container.Bind("CanGetBind", (cont, param) => "hello world");
            var getBind = container.GetBind("CanGetBind");
            Assert.AreSame(bind, getBind);

            var getBindNull = container.GetBind("CanGetBindNull");
            Assert.AreEqual(null, getBindNull);

            bind.Alias("AliasName");
            var aliasBind = container.GetBind("AliasName");
            Assert.AreSame(bind, aliasBind);
        }

        /// <summary>
        /// 检测非法的获取绑定
        /// </summary>
        [TestMethod]
        public void CheckIllegalGetBind()
        {
            var container = MakeContainer();
            container.Bind("CheckIllegalGetBind", (cont, param) => "hello world");

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.GetBind(string.Empty);
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.GetBind(null);
            });
        }

        /// <summary>
        /// 检测是否拥有绑定
        /// </summary>
        [TestMethod]
        public void CanHasBind()
        {
            var container = MakeContainer();
            var bind = container.Bind("CanHasBind", (cont, param) => "hello world");
            bind.Alias("AliasName");
            Assert.IsTrue(container.HasBind("CanHasBind"));
            Assert.IsTrue(container.HasBind("AliasName"));
            Assert.IsFalse(container.HasBind(container.Type2Service(typeof(ContainerTest))));
            bind.Alias<ContainerTest>();
            Assert.IsTrue(container.HasBind(container.Type2Service(typeof(ContainerTest))));
        }

        /// <summary>
        /// 检查是否是静态的函数
        /// </summary>
        [TestMethod]
        public void CanIsStatic()
        {
            var container = MakeContainer();
            var bind = container.Bind("CanIsStatic", (cont, param) => "hello world", true);
            container.Bind("CanIsStaticNotStatic", (cont, param) => "hello world not static");

            bind.Alias("AliasName");
            Assert.IsTrue(container.IsStatic("CanIsStatic"));
            Assert.IsTrue(container.IsStatic("AliasName"));
            Assert.IsFalse(container.IsStatic("NoAliasName"));
            Assert.IsFalse(container.IsStatic("CanIsStaticNotStatic"));
            Assert.IsTrue(container.HasBind("CanIsStaticNotStatic"));
        }
        #endregion

        #region Alias
        /// <summary>
        /// 正常的设定别名
        /// </summary>
        [TestMethod]
        public void CheckNormalAlias()
        {
            var container = MakeContainer();
            container.Bind("CheckNormalAlias", (cont, param) => "hello world");

            container.Instance("StaticService", "hello");
            ExceptionAssert.DoesNotThrow(() =>
            {
                container.Alias("AliasName1", "CheckNormalAlias");
            });
            ExceptionAssert.DoesNotThrow(() =>
            {
                container.Alias("AliasName2", "StaticService");
            });
        }

        /// <summary>
        /// 检测非法的别名输入
        /// </summary>
        [TestMethod]
        public void CheckIllegalAlias()
        {
            var container = MakeContainer();
            container.Bind("CheckIllegalAlias", (cont, param) => "hello world");
            container.Alias("AliasName", "CheckIllegalAlias");

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Alias("AliasName", "CheckIllegalAlias");
            });

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Alias("AliasNameOther", "CheckNormalAliasNotExist");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Alias(string.Empty, "CheckIllegalAlias");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Alias(null, "CheckIllegalAlias");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Alias("AliasNameOther2", string.Empty);
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Alias("AliasNameOther3", null);
            });
        }
        #endregion

        #region Call
        /// <summary>
        /// 被注入的测试类
        /// </summary>
        public class CallTestClassInject
        {
            public object GetNumber()
            {
                return 2;
            }
        }
        /// <summary>
        /// 调用测试类
        /// </summary>
        public class CallTestClass
        {
            public object GetNumber(CallTestClassInject cls)
            {
                return cls != null ? cls.GetNumber() : 1;
            }

            public object GetNumberNoParam()
            {
                return 1;
            }
        }

        /// <summary>
        /// 调用测试类
        /// </summary>
        public class CallTestClassLoopDependency
        {
            public object GetNumber(LoopDependencyClass cls)
            {
                return 1;
            }
        }

        public class LoopDependencyClass
        {
            public LoopDependencyClass(LoopDependencyClass2 cls)
            {

            }
        }

        public class LoopDependencyClass2
        {
            public LoopDependencyClass2(LoopDependencyClass cls)
            {

            }
        }

        /// <summary>
        /// 循环依赖测试
        /// </summary>
        [TestMethod]
        public void CheckLoopDependency()
        {
            var container = MakeContainer();
            container.Bind<LoopDependencyClass>();
            container.Bind<LoopDependencyClass2>();

            var cls = new CallTestClassLoopDependency();

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Call(cls, "GetNumber");
            });
        }

        /// <summary>
        /// 调用方法注入测试
        /// </summary>
        [TestMethod]
        public void CheckDelegateCall()
        {
            var container = MakeContainer();
            container.Instance<Container>(container);

            container.Call((Container cls) =>
            {
                Assert.AreNotEqual(null, cls);
            });

            container.Call((Container cls1, Container cls2) =>
            {
                Assert.AreNotEqual(null, cls1);
                Assert.AreNotEqual(null, cls2);
            });

            container.Call((Container cls1, Container cls2, Container cls3) =>
            {
                Assert.AreNotEqual(null, cls1);
                Assert.AreNotEqual(null, cls2);
                Assert.AreNotEqual(null, cls3);
            });

            container.Call((Container cls1, Container cls2, Container cls3, Container cls4) =>
            {
                Assert.AreNotEqual(null, cls1);
                Assert.AreNotEqual(null, cls2);
                Assert.AreNotEqual(null, cls3);
                Assert.AreNotEqual(null, cls4);

                Assert.AreSame(cls1, cls4);
            });
        }

        [TestMethod]
        public void CheckWrapCall()
        {
            var container = MakeContainer();
            container.Instance<Container>(container);

            var callCount = 0;
            var wrap = container.Wrap((Container cls) =>
            {
                Assert.AreNotEqual(null, cls);
                callCount++;
            });
            wrap.Invoke();

            wrap = container.Wrap((Container cls1, Container cls2) =>
            {
                Assert.AreNotEqual(null, cls1);
                Assert.AreNotEqual(null, cls2);
                callCount++;
            });
            wrap.Invoke();

            wrap = container.Wrap((Container cls1, Container cls2, Container cls3) =>
            {
                Assert.AreNotEqual(null, cls1);
                Assert.AreNotEqual(null, cls2);
                Assert.AreNotEqual(null, cls3);
                callCount++;
            });
            wrap.Invoke();

            wrap = container.Wrap((Container cls1, Container cls2, Container cls3, Container cls4) =>
            {
                Assert.AreNotEqual(null, cls1);
                Assert.AreNotEqual(null, cls2);
                Assert.AreNotEqual(null, cls3);
                Assert.AreNotEqual(null, cls4);

                Assert.AreSame(cls1, cls4);
                callCount++;
            });
            wrap.Invoke();

            Assert.AreEqual(4, callCount);
        }

        [TestMethod]
        public void TestFactory()
        {
            var container = MakeContainer();
            container.Instance<Container>(container);
            container.Instance("hello", 123);

            var fac = container.Factory<Container>(123);
            Assert.AreEqual(container.Make<Container>(), fac.Invoke());
            var fac2 = container.Factory("hello", 333);
            Assert.AreEqual(123, fac2.Invoke());
        }

        [TestMethod]
        public void TestIsAlias()
        {
            var container = MakeContainer();
            container.Instance<Container>(container);
            container.Alias("123", container.Type2Service<Container>());

            Assert.AreEqual(true, container.IsAlias("123"));
            Assert.AreEqual(false, container.IsAlias(container.Type2Service(typeof(Container))));
        }

        /// <summary>
        /// 可以调用方法
        /// </summary>
        [TestMethod]
        public void CanCallMethod()
        {
            var container = MakeContainer();
            container.Bind<CallTestClassInject>();
            var cls = new CallTestClass();

            var result = container.Call(cls, "GetNumber");
            Assert.AreEqual(2, result);

            var results = container.Call(cls, "GetNumberNoParam");
            Assert.AreEqual(1, results);
        }

        /// <summary>
        /// 无参数调用函数
        /// </summary>
        [TestMethod]
        public void CanCallMethodNoParam()
        {
            var container = MakeContainer() as IContainer;
            container.Bind<CallTestClassInject>();
            var cls = new CallTestClass();

            var result = container.Call(cls, "GetNumber", null);
            Assert.AreEqual(2, result);
        }

        /// <summary>
        /// 测试无效的调用方法
        /// </summary>
        [TestMethod]
        public void CheckIllegalCallMethod()
        {
            var container = MakeContainer();
            container.Bind<CallTestClassInject>();
            var cls = new CallTestClass();

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Call(null, "GetNumber");
            });

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Call(cls, "GetNumberIllegal");
            });
        }

        [TestMethod]
        public void TestContainerCallWithNullParams()
        {
            var container = MakeContainer();
            container.Instance("@num", 777);
            var result = container.Call(this, "TestContainerCall", null);
            Assert.AreEqual(777, result);
        }

        [TestMethod]
        public void TestContainerCallWithErrorParams()
        {
            var container = MakeContainer();
            container.Instance("@num", "helloworld");
            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Call(this, "TestContainerCall", null);
            });
        }

        /// <summary>
        /// 测试无效的传入参数
        /// </summary>
        [TestMethod]
        public void CheckIllegalCallMethodParam()
        {
            var container = MakeContainer();
            container.Bind<CallTestClassInject>();
            var cls = new CallTestClass();

            Assert.AreEqual(2, container.Call(cls, "GetNumber", "illegal param"));
            var result = container.Call(cls, "GetNumber", null);
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void TestOverflowParamNum()
        {
            var container = MakeContainer();
            var cls = new CallTestClass();

            var isThrow = false;
            try
            {
                container.Call(cls, "GetNumber", new object[256]);
            }
            catch (Exception)
            {
                isThrow = true;
            }

            Assert.AreEqual(true, isThrow);
        }

        class SimpleTestClass1 { }
        class SimpleTestClass2 { }

        [TestMethod]
        public void TestLooseParameters()
        {
            var container = MakeContainer();
            container.Bind<SimpleTestClass1>();
            container.Bind<SimpleTestClass2>();

            var objOut = new object();
            var call = container.Wrap((object[] obj, SimpleTestClass1 cls1, SimpleTestClass2 cls2) =>
            {
                Assert.AreSame(objOut, obj[0]);
                Assert.AreNotEqual(null, cls1);
                Assert.AreNotEqual(null, cls2);
                Assert.AreEqual((long)100, obj[1]);
            }, objOut, (long)100);

            call.Invoke();
        }
        #endregion

        #region Make
        public class MakeTestClass
        {
            private readonly MakeTestClassDependency dependency;

            [Inject]
            public MakeTestClassDependency Dependency { get; set; }

            [Inject]
            public MakeTestClassDependency DependencyRequired { get; set; }

            [Inject("AliasName")]
            public MakeTestClassDependency2 DependencyAlias { get; set; }

            [Inject("AliasNameRequired")]
            public MakeTestClassDependency DependencyAliasRequired { get; set; }

            public MakeTestClass(MakeTestClassDependency dependency)
            {
                this.dependency = dependency;
            }

            public string GetMsg()
            {
                return dependency.GetMsg();
            }
        }

        public class MakeTestNoParamClass
        {
            public int I { get; set; }

            public MakeTestClassDependency Dependency { get; set; }

            public MakeTestNoParamClass(int i, MakeTestClassDependency dependency)
            {
                I = i;
                Dependency = dependency;
            }
        }

        public interface IMsg
        {
            string GetMsg();
        }

        public class MakeTestClassDependency : IMsg
        {
            public string GetMsg()
            {
                return "hello";
            }
        }

        public class MakeTestClassDependency2 : IMsg
        {
            public string GetMsg()
            {
                return "world";
            }
        }

        public class NoClassAttrInject
        {
            [Inject]
            public int Time { get; set; }
        }

        /// <summary>
        /// 非类的属性注入
        /// </summary>
        [TestMethod]
        public void MakeNoClassAttrInject()
        {
            var container = MakeContainer();
            container.Bind<NoClassAttrInject>();
            container.Bind("@Time", (c, p) => 100, false);

            var result = container.Make<NoClassAttrInject>();
            Assert.AreEqual(100, result.Time);
        }

        /// <summary>
        /// 跨域生成没有绑定的服务
        /// </summary>
        [TestMethod]
        public void MakeNoBindType()
        {
            var container = MakeContainer();

            //container.OnFindType(Type.GetType); 不要使用这种写法否则域将不是这个程序集
            container.OnFindType((str) =>
            {
                return Type.GetType(str);
            });

            container.Bind<MakeTestClassDependency>().Alias("AliasNameRequired");
            container.Bind<MakeTestClassDependency2>().Alias("AliasName");
            var result = container.Make<MakeTestClass>();

            Assert.AreNotEqual(null, result);
        }

        [TestMethod]
        public void MakeWithNoParam()
        {
            var container = MakeContainer();
            container.Bind<MakeTestClassDependency>();
            var result = container.Make(container.Type2Service(typeof(MakeTestClassDependency)));
            Assert.AreNotEqual(null, result);
        }

        /// <summary>
        /// 无参构造函数的类进行make
        /// </summary>
        [TestMethod]
        public void MakeNoParamConstructor()
        {
            var container = MakeContainer();
            container.Bind<MakeTestClassDependency2>();
            var result = container.Make<MakeTestClassDependency2>();
            Assert.AreNotEqual(null, result);
        }

        /// <summary>
        /// 注入非类类型参数的构造函数
        /// </summary>
        [TestMethod]
        public void MakeNotClassConstructor()
        {
            var container = MakeContainer();
            container.Bind<MakeTestNoParamClass>();
            container.Bind<MakeTestClassDependency>();
            container.Instance("@i", 77);
            var result = container.Make<MakeTestNoParamClass>();
            Assert.AreEqual(77, result.I);
            Assert.AreNotEqual(null, result.Dependency);

            var result2 = container.Make<MakeTestNoParamClass>(100);
            Assert.AreEqual(100, result2.I);
            Assert.AreNotEqual(null, result2.Dependency);
        }

        /// <summary>
        /// 是否能正常生成服务
        /// </summary>
        [TestMethod]
        public void CanMake()
        {
            var container = MakeContainer();
            container.Bind<MakeTestClass>();
            container.Bind<MakeTestClassDependency>().Alias("AliasNameRequired");
            container.Bind<MakeTestClassDependency2>().Alias("AliasName");

            var result = container.Make<MakeTestClass>();
            Assert.AreEqual(typeof(MakeTestClass), result.GetType());

            var dep = new MakeTestClassDependency();
            var result2 = container.Make<MakeTestClass>(dep);
            Assert.AreEqual(typeof(MakeTestClass), result2.GetType());

            var result3 = container[container.Type2Service(typeof(MakeTestClass))] as MakeTestClass;
            Assert.AreEqual(typeof(MakeTestClass), result3.GetType());
        }

        /// <summary>
        /// 引发一个类型不一致的异常
        /// </summary>
        [TestMethod]
        public void CheckIllegalMakeTypeIsNotSame()
        {
            var container = MakeContainer();
            container.Singleton<MakeTestClass>();
            container.Singleton<MakeTestClassDependency2>().Alias("AliasNameRequired");
            container.Singleton<MakeTestClassDependency>().Alias("AliasName");

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Make<MakeTestClass>();
            });
        }

        /// <summary>
        /// 可以生成静态的对象
        /// </summary>
        [TestMethod]
        public void CanMakeStaticAlias()
        {
            var container = MakeContainer();
            container.Singleton<MakeTestClass>();
            container.Singleton<MakeTestClassDependency2>().Alias("AliasName");
            container.Singleton<MakeTestClassDependency>().Alias("AliasNameRequired");

            var result1 = container.Make<MakeTestClass>();
            var result2 = container.Make<MakeTestClass>();

            Assert.AreSame(result1, result2);
            Assert.AreSame(result1.DependencyAliasRequired, result2.DependencyAliasRequired);
            Assert.AreNotSame(result1.DependencyAlias, result2.DependencyAliasRequired);
        }

        /// <summary>
        /// 可以根据别名来生成对应不同的实例
        /// </summary>
        [TestMethod]
        public void CanMakeWithAlias()
        {
            var container = MakeContainer();
            container.Bind<MakeTestClass>();
            container.Bind<MakeTestClassDependency2>().Alias("AliasName");
            container.Bind<MakeTestClassDependency>().Alias("AliasNameRequired");

            var result = container.Make<MakeTestClass>();

            Assert.AreEqual("world", result.DependencyAlias.GetMsg());
            Assert.AreEqual("hello", result.DependencyAliasRequired.GetMsg());
        }

        /// <summary>
        /// 能够生成常规绑定
        /// </summary>
        [TestMethod]
        public void CanMakeNormalBind()
        {
            var container = MakeContainer();
            container.Bind<MakeTestClass>();
            container.Bind<MakeTestClassDependency>().Alias("AliasNameRequired");
            container.Bind<MakeTestClassDependency2>().Alias("AliasName");

            var result1 = container.Make<MakeTestClass>();
            var result2 = container.Make<MakeTestClass>();

            Assert.AreNotSame(result1, result2);
            Assert.AreNotSame(result1.Dependency, result1.DependencyRequired);
            Assert.AreNotSame(null, result1.DependencyRequired);
            Assert.AreNotSame(null, result1.DependencyAliasRequired);
            Assert.AreNotEqual(null, result1.DependencyAlias);
        }

        /// <summary>
        /// 必须参数约束
        /// </summary>
        [TestMethod]
        public void CheckMakeRequired()
        {
            var container = MakeContainer();
            container.Bind<MakeTestClass>();

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Make<MakeTestClass>();
            });
        }

        /// <summary>
        /// 无效的生成服务
        /// </summary>
        [TestMethod]
        public void CheckIllegalMake()
        {
            var container = MakeContainer();
            container.Bind<MakeTestClass>();

            ExceptionAssert.Throws<ArgumentNullException>(() =>
            {
                container.Make(string.Empty);
            });
        }

        /// <summary>
        /// 解决器是否有效
        /// </summary>
        [TestMethod]
        public void CanMakeWithResolve()
        {
            var container = MakeContainer();
            var bind = container.Bind<MakeTestClassDependency>();

            bind.OnResolving((bindData, obj) => "local resolve");
            container.OnResolving((bindData, obj) => obj + " global resolve");
            var isTrigger = false;
            container.OnResolving((obj) => isTrigger = true);

            var result = container.Make(container.Type2Service(typeof(MakeTestClassDependency)));

            Assert.AreEqual(true, isTrigger);
            Assert.AreEqual("local resolve global resolve", result);
        }

        /// <summary>
        /// 给与了错误的解决器,导致不正确的返回值
        /// </summary>
        [TestMethod]
        public void CheckMakeWithErrorResolve()
        {
            var container = MakeContainer();
            var bind = container.Bind<MakeTestClass>();
            container.Bind<MakeTestClassDependency2>().Alias("AliasName");
            var bind2 = container.Bind<MakeTestClassDependency>().Alias("AliasNameRequired");

            bind.OnResolving((bindData, obj) => "local resolve");
            container.OnResolving((bindData, obj) => obj + " global resolve");
            bind2.OnResolving((bindData, obj) => "bind2");

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Make(container.Type2Service(typeof(MakeTestClass)));
            });
        }

        /// <summary>
        /// 参数注入标记测试类
        /// </summary>
        public class TestMakeParamInjectAttrClass
        {
            private IMsg msg;
            public TestMakeParamInjectAttrClass(
                [Inject("AliasName")]IMsg msg)
            {
                this.msg = msg;
            }

            public string GetMsg()
            {
                return msg.GetMsg();
            }
        }

        /// <summary>
        /// 参数可以使用注入标记
        /// </summary>
        [TestMethod]
        public void CanParamUseInjectAttr()
        {
            var container = MakeContainer();
            var bind = container.Bind<TestMakeParamInjectAttrClass>();
            container.Bind<MakeTestClassDependency>();
            var subBind = container.Bind<MakeTestClassDependency2>().Alias("AliasName");

            bind.Needs<IMsg>().Given<MakeTestClassDependency>();
            var cls = container.Make<TestMakeParamInjectAttrClass>();
            Assert.AreEqual("world", cls.GetMsg());

            bind.Needs("AliasName").Given<MakeTestClassDependency>();
            cls = container.Make<TestMakeParamInjectAttrClass>();
            Assert.AreEqual("hello", cls.GetMsg());

            subBind.Unbind();
            cls = container.Make<TestMakeParamInjectAttrClass>();
            Assert.AreEqual("hello", cls.GetMsg());
        }

        public class TestMakeBasePrimitive
        {
            [Inject]
            public int Value { get; set; }
        }

        [TestMethod]
        public void TestUnresolvablePrimitiveAttr()
        {
            var container = MakeContainer();
            container.Bind<TestMakeBasePrimitive>();

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make<TestMakeBasePrimitive>();
            });
        }

        public class TestMakeBasePrimitiveConstructor
        {
            public TestMakeBasePrimitiveConstructor(int value)
            {

            }
        }

        [TestMethod]
        public void TestUnresolvablePrimitiveConstructor()
        {
            var container = MakeContainer();
            container.Bind<TestMakeBasePrimitiveConstructor>();
            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make<TestMakeBasePrimitiveConstructor>();
            });
        }

        public class TestOptionalPrimitiveClass
        {
            public TestOptionalPrimitiveClass(int value = 100)
            {
                Assert.AreEqual(100, value);
            }
        }

        public class SupportNullContainer : Container
        {
            public string[] GetStack()
            {
                return BuildStack.ToArray();
            }

            public object[][] GetUserParams()
            {
                return UserParamsStack.ToArray();
            }

            protected override void GuardResolveInstance(object instance, string makeService)
            {

            }
        }

        [TestMethod]
        public void TestSupportNullValueContainer()
        {
            var container = new SupportNullContainer() as IContainer;
            container.Bind("null", (c, p) => null);

            Assert.AreEqual(null, container.Make("null"));
        }

        [TestMethod]
        public void TestGetStack()
        {
            var container = new SupportNullContainer();
            container.Bind("null", (c, p) =>
            {
                Assert.AreEqual(1, container.GetStack().Length);
                Assert.AreEqual(1, container.GetUserParams().Length);
                Assert.AreEqual(3, container.GetUserParams()[0].Length);
                return null;
            });

            Assert.AreEqual(null, container.Make("null", "123", "hello", 12333));

            Assert.AreEqual(0, container.GetStack().Length);
            Assert.AreEqual(0, container.GetUserParams().Length);
        }

        public class TestInjectNullClass
        {
            public TestInjectNullClass(TestMakeBasePrimitiveConstructor cls)
            {
                Assert.AreEqual(null, cls);
            }
        }

        [TestMethod]
        public void TestInjectNull()
        {
            var container = new SupportNullContainer() as IContainer;
            container.Bind<TestInjectNullClass>();

            container.Make<TestInjectNullClass>();
        }

        public class TestDefaultValueClass
        {
            public TestDefaultValueClass(SupportNullContainer container = null)
            {
                Assert.AreEqual(null, container);
            }
        }

        [TestMethod]
        public void TestDefaultValue()
        {
            var container = new Container();
            container.Bind<TestDefaultValueClass>();
            container.Make<TestDefaultValueClass>();
        }

        [TestMethod]
        public void TestAllFalseFindType()
        {
            var container = new Container();

            container.OnFindType((str) => null);

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make<TestDefaultValueClass>();
            });
        }

        [TestMethod]
        public void TestOptionalPrimitive()
        {
            var container = MakeContainer();
            container.Bind<TestOptionalPrimitiveClass>();
            container.Make<TestOptionalPrimitiveClass>();
        }

        /// <summary>
        /// 参数注入是必须的
        /// </summary>
        public class TestMakeParamInjectAttrRequiredClass
        {
            private IMsg msg;
            public TestMakeParamInjectAttrRequiredClass(
                IMsg msg)
            {
                this.msg = msg;
            }

            public string GetMsg()
            {
                return msg.GetMsg();
            }
        }

        /// <summary>
        /// 参数可以使用注入标记
        /// </summary>
        [TestMethod]
        public void CanParamUseInjectAttrRequired()
        {
            var container = MakeContainer();
            container.Bind<TestMakeParamInjectAttrRequiredClass>();

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Make<TestMakeParamInjectAttrRequiredClass>();
            });

            container.Bind<MakeTestClassDependency, IMsg>();
            var result = container.Make<TestMakeParamInjectAttrRequiredClass>();
            Assert.AreEqual("hello", result.GetMsg());
        }

        public struct TestStruct
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// 测试结构体注入
        /// </summary>
        class TestMakeStructInject
        {
            [Inject]
            public TestStruct Struct { get; set; }

            [Inject]
            public MakeTestClassDependency Dependency { get; set; }
        }

        /// <summary>
        /// 可以进行结构体注入
        /// </summary>
        [TestMethod]
        public void CanMakeStructInject()
        {
            var container = MakeContainer();
            container.OnFindType((str) =>
            {
                return Type.GetType(str);
            });

            var result = container.Make<TestMakeStructInject>();
            Assert.AreNotEqual(null, result.Struct);
            Assert.AreNotEqual(null, result.Dependency);
        }

        class GenericClass<T>
        {
            public string GetMsg()
            {
                return typeof(T).ToString();
            }
        }

        /// <summary>
        /// 测试泛型注入
        /// </summary>
        class TestMakeGenericInject
        {
            [Inject]
            public GenericClass<string> Cls { get; set; }
        }

        /// <summary>
        /// 可以进行泛型注入
        /// </summary>
        [TestMethod]
        public void CanMakeGenericInject()
        {
            var container = MakeContainer();
            container.OnFindType((str) =>
            {
                return Type.GetType(str);
            });

            var result = container.Make<TestMakeGenericInject>();
            Assert.AreNotEqual(null, result.Cls);
            Assert.AreEqual(typeof(string).ToString(), result.Cls.GetMsg());

            container.Bind<GenericClass<string>>((app, param) => new GenericClass<string>());
            result = container.Make<TestMakeGenericInject>();
            Assert.AreNotEqual(null, result.Cls);

        }

        class TestOptionalInject
        {
            [Inject]
            public GenericClass<string> Cls { get; set; }

            public GenericClass<string> ClsNull { get; set; }
        }

        /// <summary>
        /// 可以进行泛型注入
        /// </summary>
        [TestMethod]
        public void OptionalInject()
        {
            var container = MakeContainer();
            container.OnFindType((str) =>
            {
                return Type.GetType(str);
            });

            var result = container.Make<TestOptionalInject>();
            Assert.AreNotEqual(null, result.Cls);
            Assert.AreEqual(null, result.ClsNull);
        }

        abstract class TestInjectBase
        {
            [Inject]
            public virtual GenericClass<string> Base { get; set; }

            public virtual GenericClass<string> Base2 { get; set; }

            [Inject]
            public virtual GenericClass<string> Base3 { get; set; }

            [Inject]
            public virtual GenericClass<string> Base4 { get; set; }

            [Inject]
            public abstract GenericClass<string> Base5 { get; set; }
        }

        class TestInject : TestInjectBase
        {
            [Inject]
            public override GenericClass<string> Base { get; set; }

            [Inject]
            public override GenericClass<string> Base2 { get; set; }

            public override GenericClass<string> Base3 { get; set; }

            public override GenericClass<string> Base5 { get; set; }
        }

        /// <summary>
        /// 实例一个无效的类
        /// </summary>
        [TestMethod]
        public void InvalidClassNew()
        {
            var container = MakeContainer();
            container.OnFindType((str) =>
            {
                return Type.GetType(str);
            });

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make<TestInjectBase>();
            });
        }

        /// <summary>
        /// 继承注入
        /// </summary>
        [TestMethod]
        public void InheritanceInject()
        {
            var container = MakeContainer();
            container.OnFindType((str) =>
            {
                return Type.GetType(str);
            });

            var result = container.Make<TestInject>();

            Assert.AreNotEqual(null, result.Base);
            Assert.AreNotEqual(null, result.Base2);
            Assert.AreEqual(null, result.Base3);
            Assert.AreNotEqual(null, result.Base4);
            Assert.AreEqual(null, result.Base5);
        }

        interface IComplexInterface
        {
            string Message();
        }

        class ComplexClass
        {
            [Inject]
            public IComplexInterface Msg { get; set; }
        }

        class ComplexInjectClass1 : IComplexInterface
        {
            public string Message()
            {
                return "ComplexInjectClass1";
            }
        }

        class ComplexInjectClass2 : IComplexInterface
        {
            public string Message()
            {
                return "ComplexInjectClass2";
            }
        }

        /// <summary>
        /// 复杂的上下文关系测试
        /// </summary>
        [TestMethod]
        public void ComplexContextualRelationshipTest1()
        {
            var container = MakeContainer();
            container.Bind<ComplexClass>().Needs<IComplexInterface>().Given("IComplexInterface.alias");
            container.Bind<ComplexInjectClass1>().Alias<IComplexInterface>();
            container.Bind<ComplexInjectClass2>().Alias("IComplexInterface.alias");

            var cls = container.Make<ComplexClass>();
            Assert.AreEqual("ComplexInjectClass2", cls.Msg.Message());
        }

        /// <summary>
        /// 复杂的上下文关系测试
        /// </summary>
        [TestMethod]
        public void ComplexContextualRelationshipTest2()
        {
            var container = MakeContainer();
            container.Bind<ComplexClass>();
            container.Bind<ComplexInjectClass1>().Alias<IComplexInterface>();
            container.Bind<ComplexInjectClass2>().Alias("IComplexInterface.alias");

            var cls = container.Make<ComplexClass>();
            Assert.AreEqual("ComplexInjectClass1", cls.Msg.Message());
        }

        class ComplexClassAlias
        {
            [Inject("IComplexInterface.alias")]
            public IComplexInterface Msg { get; set; }
        }

        /// <summary>
        /// 复杂的上下文关系测试
        /// </summary>
        [TestMethod]
        public void ComplexContextualRelationshipTest3()
        {
            var container = MakeContainer();
            container.Bind<ComplexClassAlias>();
            container.Bind<ComplexInjectClass1>().Alias<IComplexInterface>();
            container.Bind<ComplexInjectClass2>().Alias("IComplexInterface.alias");

            var cls = container.Make<ComplexClassAlias>();
            Assert.AreEqual("ComplexInjectClass2", cls.Msg.Message());
        }

        /// <summary>
        /// 复杂的上下文关系测试
        /// </summary>
        [TestMethod]
        public void ComplexContextualRelationshipTest4()
        {
            var container = MakeContainer();
            container.Bind<ComplexClassAlias>().Needs("IComplexInterface.alias").Given<IComplexInterface>();
            container.Bind<ComplexInjectClass1>().Alias<IComplexInterface>();
            container.Bind<ComplexInjectClass2>().Alias("IComplexInterface.alias");

            var cls = container.Make<ComplexClassAlias>();
            Assert.AreEqual("ComplexInjectClass1", cls.Msg.Message());
        }
        #endregion

        #region Instance
        /// <summary>
        /// 可以正确的给定静态实例
        /// </summary>
        [TestMethod]
        public void CanInstance()
        {
            var container = MakeContainer();
            var data = new List<string> { "hello world" };
            var data2 = new List<string> { "hello world" };
            container.Instance("TestInstance", data);
            var result = container.Make("TestInstance");

            Assert.AreSame(data, result);
            Assert.AreNotSame(data2, result);
        }

        /// <summary>
        /// 测试无效的实例
        /// </summary>
        [TestMethod]
        public void CheckIllegalInstance()
        {
            var container = MakeContainer();
            container.Bind("TestInstance", (app, param) => "hello world", false);
            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Instance("TestInstance", "online");
            });
        }

        /// <summary>
        /// 能够通过release
        /// </summary>
        [TestMethod]
        public void CanInstanceWithRelease()
        {
            var container = MakeContainer();
            var bindData = container.Bind("TestInstance", (app, param) => string.Empty, true);

            bool isComplete = false, isComplete2 = false;
            bindData.OnRelease((bind, obj) =>
            {
                Assert.AreEqual("hello world", obj);
                Assert.AreSame(bindData, bind);
                isComplete = true;
            });

            container.OnRelease((bind, obj) =>
            {
                Assert.AreEqual("hello world", obj);
                Assert.AreSame(bindData, bind);
                isComplete2 = true;
            });
            container.Instance("TestInstance", "hello world");
            container.Release("TestInstance");

            if (isComplete && isComplete2)
            {
                return;
            }
            Assert.Fail();
        }

        public class TestDisposableClass : IDisposable
        {
            public bool isDispose;
            public void Dispose()
            {
                isDispose = true;
            }
        }

        [TestMethod]
        public void TestDisposableRelease()
        {
            var container = MakeContainer();
            container.Singleton<TestDisposableClass>();
            var cls = container.Make<TestDisposableClass>();
            container.Release<TestDisposableClass>();
            Assert.AreEqual(true, cls.isDispose);
        }
        #endregion

        [TestMethod]
        public void TestOneWatch()
        {
            var container = MakeContainer();

            var b = container.Bind<object>((c, p) => 123);
            var obj = container.Make<object>();
            Assert.AreEqual(123, obj);

            b.Unbind();

            object ins1 = null;
            container.Watch<object>((instance) =>
            {
                ins1 = instance;
            });
            container.Bind<object>((c, p) => new object());

            Assert.AreNotEqual(null, ins1);
        }

        [TestMethod]
        public void TestNullBindWatch()
        {
            var container = MakeContainer();
            container.Instance<object>(123);

            object ins1 = null, ins2 = null;
            container.Watch<object>((instance) =>
            {
                ins1 = instance;
            });
            container.Watch<object>((instance) =>
            {
                ins2 = instance;
            });
            var obj = new object();
            container.Instance<object>(obj);

            Assert.AreSame(obj, ins1);
            Assert.AreEqual(obj, ins1);
            Assert.AreSame(obj, ins2);
            Assert.AreEqual(obj, ins2);
        }

        [TestMethod]
        public void TestBindWatch()
        {
            var container = MakeContainer();

            var b = container.Bind<object>((c, p) => 123);
            var obj = container.Make<object>();
            Assert.AreEqual(123, obj);

            b.Unbind();

            object ins1 = null,ins2 = null;
            container.Watch<object>((instance) =>
            {
                ins1 = instance;
            });
            container.Watch<object>((instance) =>
            {
                ins2 = instance;
            });
            container.Bind<object>((c, p) => new object());

            Assert.AreNotSame(ins1, ins2);
        }

        [TestMethod]
        public void TestSingletonWatch()
        {
            var container = MakeContainer();

            var b = container.Singleton<object>((c, p) => 123);
            var obj = container.Make<object>();
            Assert.AreEqual(123, obj);

            b.Unbind();

            object ins1 = null, ins2 = null;
            container.Watch<object>((instance) =>
            {
                ins1 = instance;
            });
            container.Watch<object>((instance) =>
            {
                ins2 = instance;
            });
            container.Singleton<object>((c, p) => new object());

            Assert.AreSame(ins1, ins2);
        }

        [TestMethod]
        public void TestNullFlash()
        {
            var container = MakeContainer();
            container.Flash(() =>
            {
            }, null);

            // no throw error is success
        }

        [TestMethod]
        public void TestEmptyFlash()
        {
            var container = MakeContainer();
            container.Flash(() =>
            {
            }, new KeyValuePair<string, object>[] { });

            // no throw error is success
        }

        [TestMethod]
        public void TestFlashRecursive()
        {
            var container = MakeContainer();

            var call = 0;
            container.Flash(() =>
            {
                call++;
                Assert.AreEqual(1, container.Make("hello"));
                Assert.AreEqual(2, container.Make("world"));
                container.Flash(() =>
                {
                    call++;
                    Assert.AreEqual(10, container.Make("hello"));
                    Assert.AreEqual(2, container.Make("world"));
                }, new KeyValuePair<string, object>("hello", 10));
                Assert.AreEqual(1, container.Make("hello"));
                Assert.AreEqual(2, container.Make("world"));
            },new KeyValuePair<string, object>("hello", 1)
                , new KeyValuePair<string, object>("world", 2));

            Assert.AreEqual(false, container.HasInstance("hello"));
            Assert.AreEqual(false, container.HasInstance("world"));
            Assert.AreEqual(2, call);
        }

        [TestMethod]
        public void OnResolvingExistsObject()
        {
            var container = MakeContainer();
            var data = new List<string> { "hello world" };
            container.Instance("TestInstance", data);

            var isCall = false;
            container.OnResolving((bind, obj) =>
            {
                isCall = true;
                return obj;
            });

            Assert.AreEqual(false, isCall);
        }

        /// <summary>
        /// 测试释放所有静态服务
        /// </summary>
        [TestMethod]
        public void TestReleaseAllStaticService()
        {
            var container = MakeContainer();
            var data = new List<string> { "hello world" };
            var isCallTest = false;
            container.Singleton("Test", (c, p) => { return "Test1"; }).OnRelease((bind, o) => { isCallTest = true; });
            container.Instance("TestInstance2", data);

            Assert.AreEqual("Test1", container.Make("Test"));

            container.Flush();

            Assert.AreEqual(true, isCallTest);

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make("TestInstance2");
            });

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make("Test");
            });
        }

        [TestMethod]
        public void TestSameAliaseServiceName()
        {
            var container = MakeContainer();

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                container.Singleton<ContainerTest>().Alias<ContainerTest>();
            });
        }

        public class TestParamsMakeClass
        {
            public TestParamsMakeClass()
            {
            }
        }

        [TestMethod]
        public void TestMakeWithParams()
        {
            var container = MakeContainer();
            container.Bind<TestParamsMakeClass>();
            Assert.AreEqual(typeof(TestParamsMakeClass), container.Make<TestParamsMakeClass>(null).GetType());
        }

        [TestMethod]
        public void TestBaseStructChange()
        {
            var container = new Container();
            var result = container.Call(this, "TestContainerCall", "100");
            Assert.AreEqual(100, result);
        }

        [TestMethod]
        public void TestBaseStructChangeInvalid()
        {
            var container = new Container();
            container.Instance<int>(10000);
            Assert.AreEqual(10000, container.Call(this, "TestContainerCall", "100000000000000000000"));
        }

        [TestMethod]
        public void TestFormatException()
        {
            var container = new Container();
            container.Instance("@num", 10);
            Assert.AreEqual(10, container.Call(this, "TestContainerCall", new ContainerTest()));
        }

        internal class TestNoConstructorAccessClass
        {
            private TestNoConstructorAccessClass() { }
        }

        [TestMethod]
        public void TestNoConstructorAccessClassFunction()
        {
            var container = MakeContainer();
            container.Singleton<TestNoConstructorAccessClass>();

            var isThrow = false;
            try
            {
                container.Make<TestNoConstructorAccessClass>();
            }
            catch (RuntimeException ex)
            {
                isThrow = ex.InnerException.GetType() == typeof(MissingMethodException);
            }
            Assert.AreEqual(true, isThrow);
        }

        internal class TestConstructorExceptionClass
        {
            public TestConstructorExceptionClass()
            {
                throw new Exception("TestConstructorExceptionClass");
            }
        }

        [TestMethod]
        public void TestConstructorExceptionFunction()
        {
            var container = MakeContainer();
            container.Singleton<TestConstructorExceptionClass>();

            var isThrow = false;
            var isException = false;
            try
            {
                container.Make<TestConstructorExceptionClass>();
            }
            catch (RuntimeException ex)
            {
                isThrow = ex.InnerException.GetType() == typeof(TargetInvocationException);
                isException = ex.InnerException.InnerException.Message == "TestConstructorExceptionClass";
            }
            Assert.AreEqual(true, isThrow);
            Assert.AreEqual(true, isException);
        }

        private class ParamsTypeInjectTest
        {
            public int num1;
            public long num2;
            public string str;

            public ParamsTypeInjectTest(int num1, long num2,string str)
            {
                this.num1 = num1;
                this.num2 = num2;
                this.str = str;
            }
        }

        [TestMethod]
        public void TestParamsUserParams()
        {
            var container = MakeContainer();
            container.Bind<ParamsTypeInjectTest>();

            var result = container.Make<ParamsTypeInjectTest>(new Params
            {
                {"num2", 100},
                {"num1", 50},
                {"str", "helloworld"},
            }, 100, 200, "dog");

            Assert.AreEqual(50, result.num1);
            Assert.AreEqual(100, result.num2);
            Assert.AreEqual("helloworld", result.str);
        }

        [TestMethod]
        public void TestMultParamsUserParams()
        {
            var container = MakeContainer();
            container.Bind<ParamsTypeInjectTest>();

            var result = container.Make<ParamsTypeInjectTest>(new Params
            {
                {"num2", 100},
                {"num1", 50},
            }, 100, new Params
            {
                {"num2", 500},
                {"num1", 4000},
                {"str", "helloworld"},
            }, 200, "dog");

            Assert.AreEqual(50, result.num1);
            Assert.AreEqual(100, result.num2);
            Assert.AreEqual("helloworld", result.str);
        }

        [TestMethod]
        public void TestParamsUserParamsThrowError()
        {
            var container = MakeContainer();
            container.Bind<ParamsTypeInjectTest>();

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                var result = container.Make<ParamsTypeInjectTest>(new Params
                {
                    {"num2", 100},
                    {"num1", "helloworld"},
                    {"str", "helloworld"},
                });
            });
        }

        private class ContainerReplaceThrow : Container
        {
            // 测试由于开发者写错代码导致的bug是否被正确抛出异常
            protected override Func<ParameterInfo, object> GetParamsMatcher(ref object[] userParams)
            {
                return (param) =>
                {
                    return 200;
                };
            }
        }

        [TestMethod]
        public void TestReplaceContainerParamsUserParamsThrowError()
        {
            var container = new ContainerReplaceThrow();
            container.Bind<ParamsTypeInjectTest>();

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make<ParamsTypeInjectTest>(new Params
                {
                    {"num2", 100},
                    {"num1", "helloworld"},
                    {"str", "helloworld"},
                });
            });
        }

        public class TestResloveAttrClassSpeculationService
        {
            [Inject]
            public RuntimeException rex { get; set; }

            public UnresolvableException ex;
            public TestResloveAttrClassSpeculationService(UnresolvableException ex)
            {
                this.ex = ex;
            }
        }

        [TestMethod]
        public void TestResloveAttrClassSpeculationServiceFunc()
        {
            var container = new Container();
            container.Bind<TestResloveAttrClassSpeculationService>();
            container.Instance("@ex", new UnresolvableException());
            container.Instance("@rex", new UnresolvableException());
            var cls = container.Make<TestResloveAttrClassSpeculationService>();

            Assert.AreSame(container.Make("@ex"), cls.ex);
        }

        [TestMethod]
        public void TestResloveAttrClassSpeculationServiceAttrs()
        {
            var container = new Container();
            container.Bind<TestResloveAttrClassSpeculationService>();
            container.Instance("@ex", new UnresolvableException());

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make<TestResloveAttrClassSpeculationService>();
            });
        }

        public class VariantModel : IVariant
        {
            public int num;
            public VariantModel(int num)
            {
                this.num = num;
                if (num == 0)
                {
                    throw new Exception();
                }
            }
        }

        public class VariantFather
        {
            public long code;
            public VariantModel model;
            public VariantFather(long code, VariantModel model)
            {
                this.code = code;
                this.model = model;
            }
        }

        /// <summary>
        /// 测试类型变换
        /// </summary>
        [TestMethod]
        public void TestVariant()
        {
            var container = new Container();
            container.Bind<VariantModel>();
            container.Bind<VariantFather>();

            var cls = container.Make<VariantFather>(10, 20);

            Assert.AreEqual(10, cls.code);
            Assert.AreEqual(20, cls.model.num);
        }

        [TestMethod]
        public void TestVariantThrowError()
        {
            var container = new Container();
            container.Bind<VariantModel>();
            container.Bind<VariantFather>();

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make<VariantFather>(10, 0);
            });
        }

        [TestMethod]
        public void TestNullFromParams()
        {
            var container = new Container();
            container.Bind<VariantModel>();
            container.Bind<VariantFather>();

            ExceptionAssert.Throws<UnresolvableException>(() =>
            {
                container.Make<VariantFather>(10, new Params {{"model", null}});
            });
        }

        class CallInjectClass
        {
            public string Value { get; set; }
        }

        [TestMethod]
        public void TestCallInject()
        {
            var container = new Container();
            var isCall = false;
            container.Call((CallInjectClass cls) =>
            {
                Assert.AreEqual("100", cls.Value);
                isCall = true;
            }, new CallInjectClass { Value = "100"});
            Assert.AreEqual(true, isCall);
        }

        class CallInjectClassCurrent : IParams
        {
            public string Value { get; set; }

            /// <summary>
            /// 获取一个参数
            /// </summary>
            /// <param name="key">参数名</param>
            /// <param name="value">参数值</param>
            /// <returns>是否成功获取</returns>
            public bool TryGetValue(string key, out object value)
            {
                value = null;
                return false;
            }
        }

        [TestMethod]
        public void TestCallInjectCurrent()
        {
            var container = new Container();
            var isCall = false;
            container.Call((CallInjectClassCurrent cls) =>
            {
                Assert.AreEqual("100", cls.Value);
                isCall = true;
            }, new CallInjectClassCurrent { Value = "100" });
            Assert.AreEqual(true, isCall);
        }

        /// <summary>
        /// 测试基础容器调用
        /// </summary>
        /// <param name="num"></param>
        public int TestContainerCall(int num)
        {
            return num;
        }

        #region Rebound
        [TestMethod]
        public void TestOnRebound()
        {
            var container = new Container();
            var callRebound = false;
            container.OnRebound("TestService", (instance) =>
            {
                Assert.AreEqual(300, instance);
                callRebound = true;
            });

            container.Bind("TestService", (c, p) => 100).Unbind();
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
            container.OnRebound("TestService", (instance) =>
            {
                Assert.AreEqual(300, instance);
                callRebound = true;
            });

            container.Instance("TestService", 100);
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
        public void TestWatch()
        {
            var container = new Container();

            container.Instance<IContainer>(container);
            var cls = new TestWatchCLass();
            container.Watch("WatchService", cls, "OnChange");
            container.Instance("WatchService", 100);
            container.Instance("WatchService", 200);

            Assert.AreEqual(200, cls.value);
            Assert.AreSame(container, cls.container);
        }

        [TestMethod]
        public void TestInstanceAndDecorator()
        {
            var container = new Container();
            var oldObject = new object();
            object newObject = null;
            container.OnResolving((bindData, obj) =>
            {
                return newObject = new object();
            });

            container.Instance("Hello", oldObject);

            Assert.AreSame(newObject, container["Hello"]);
        }

        [TestMethod]
        public void TestOccupiedKeyInstance()
        {
            var container = new Container();
            container.Instance<IBindData>(null);
            var cls = new TestWatchCLass();
            container.Watch("WatchService", cls, "OnChange");
            container.Instance("WatchService", 100);

            var isThrow = false;
            try
            {
                container.Instance("WatchService", 200);
            }
            catch (RuntimeException)
            {
                isThrow = true;
            }

            Assert.AreEqual(true, isThrow);
        }

        [TestMethod]
        public void TestFlashOnBind()
        {
            var container = new Application();
            container.Bind<IBindData>((c, p) => 100);

            ExceptionAssert.Throws<RuntimeException>(() =>
            {
                App.Flash(() =>
                {

                }, App.Type2Service(typeof(IBindData)), 200);
            });
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
        #endregion

        /// <summary>
        /// 生成容器
        /// </summary>
        /// <returns>容器</returns>
        private Container MakeContainer()
        {
            var container = new Container();
            return container;
        }
    }
}