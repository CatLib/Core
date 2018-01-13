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
    }
}
