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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatLib.Tests.Stl
{
    /// <summary>
    /// 容器性能测试
    /// </summary>
    [TestClass]
    public class ContainerPerformanceTests
    {
        public void Watch(string name ,Action action, int count = 1)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (count-- > 0)
            {
                action();
            }
            sw.Stop();
            Console.WriteLine("["+ name + "]执行花费{0}ms.", sw.Elapsed.TotalMilliseconds);
        }

        public class TestSerializeClass
        {
            
        }

        [TestMethod]
        public void TestCreateInstance()
        {
            Watch("CreateInstance()", () =>
            {
                Activator.CreateInstance(typeof(TestSerializeClass));
            }, 1000000);

            Watch("CreateInstance(null)",() =>
            {
                Activator.CreateInstance(typeof(TestSerializeClass), null);
            }, 1000000);

            Watch("CreateInstance(object[])", () =>
            {
                Activator.CreateInstance(typeof(TestSerializeClass), new object[]{});
            }, 1000000);
        }

        public class TestMakeHandClass
        {
            public TestMakeHandClass(TestSerializeClass cls)
            {

            }
        }

        public class TestMakeClass : ITestMakeClass
        {
            public TestMakeClass(TestSerializeClass cls)
            {
                
            }
        }

        public class TestMakeClass2
        {
            public TestMakeClass2(TestSerializeClass cls)
            {

            }
        }

        public class TestMakeNullParamsClass : ITestMakeNullParamsClass
        {
            public TestMakeNullParamsClass()
            {

            }
        }

        public interface ITestMakeClass { }
        public interface ITestMakeNullParamsClass { }

        [TestMethod]
        public void TestSingleMake()
        {
            var container = new Container();
            container.Singleton<TestMakeHandClass>((_, __) => new TestMakeHandClass(null));
            container.Singleton<TestMakeNullParamsClass>();
            container.Singleton<TestSerializeClass>();
            container.Singleton<TestMakeClass>().Alias<ITestMakeClass>();

            Watch("TestSingleMake(非反射) 1000000次", () =>
            {
                container.Make<TestMakeHandClass>();
            }, 1000000);

            Watch("TestSingleMake(反射，依赖注入) 1000000次", () =>
            {
                container.Make<ITestMakeClass>();
            }, 1000000);

            Watch("TestSingleMake(反射，无注入) 1000000次", () =>
            {
                container.Make<TestMakeNullParamsClass>();
            }, 1000000);
        }

        [TestMethod]
        public void TestBindMake()
        {
            var container = new Application();
            container.Bind<TestMakeHandClass>((_, __) => new TestMakeHandClass(null));
            container.Singleton<TestSerializeClass>();
            container.Bind<TestMakeClass>().Alias<ITestMakeClass>();
            container.Bind<TestMakeNullParamsClass>().Alias<ITestMakeNullParamsClass>();
        
            Watch("TestBindMake(非反射) 1000000次", () =>
            {
                App.Make<TestMakeHandClass>();
            }, 1000000);
            
            Watch("TestBindMake(反射，依赖注入) 1000000次", () =>
            {
                App.Make<ITestMakeClass>();
            }, 1000000);

            Watch("TestBindMake(反射，无注入) 1000000次", () =>
            {
                App.Make<ITestMakeNullParamsClass>();
            }, 1000000);
        }

        public class TestMakeClassFacade : Facade<ITestMakeClass>
        {
            
        }

        public class TestMakeClassNoParamsFacade : Facade<ITestMakeNullParamsClass>
        {

        }

        public abstract class OriginalFacade<TInterface> where TInterface : new()
        {
            private static TInterface instance;
            /// <summary>
            /// 门面实例
            /// </summary>
            public static TInterface Instance
            {
                get
                {
                    if (instance != null)
                    {
                        return instance;
                    }

                    return instance = new TInterface();
                }
            }
        }

        public class TestOriginalFacade : OriginalFacade<TestMakeNullParamsClass>
        {
        }

        [TestMethod]
        public void TestOriginalFacadeSpeed()
        {
            Watch("TestOriginalFacadeSpeed() 1000000次", () =>
            {
                var obj = TestOriginalFacade.Instance;
            }, 1000000);
        }

        [TestMethod]
        public void TestSingletonFacade()
        {
            var container = new Application();
            container.Singleton<TestSerializeClass>();
            container.Singleton<TestMakeClass>().Alias<ITestMakeClass>();

            Watch("TestSingletonFacade() 1000000次", () =>
            {
                var obj = TestMakeClassFacade.Instance;
            }, 1000000);
        }

        [TestMethod]
        public void TestBindFacade()
        {
            var container = new Application();
            container.Singleton<TestSerializeClass>();
            container.Bind<TestMakeClass>().Alias<ITestMakeClass>();
            container.Bind<TestMakeNullParamsClass>().Alias<ITestMakeNullParamsClass>();

            Watch("TestBindFacade() 1000000次", () =>
            {
                var obj = TestMakeClassFacade.Instance;
            }, 1000000);

            Watch("TestBindFacade(无参数) 1000000次", () =>
            {
                var obj = TestMakeClassNoParamsFacade.Instance;
            }, 1000000);
        }
    }
}
