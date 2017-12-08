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

namespace CatLib
{
    /// <summary>
    /// 方法容器
    /// </summary>
    internal sealed class MethodContainer
    {
        /// <summary>
        /// 调用方法目标 映射到 方法名字
        /// </summary>
        private Dictionary<object, List<string>> targetToMethodsMappings;

        /// <summary>
        /// 绑定数据
        /// </summary>
        private Dictionary<string, MethodBind> methodMappings;

        /// <summary>
        /// 依赖注入容器
        /// </summary>
        private Container container;

        /// <summary>
        /// 依赖解决器
        /// </summary>
        private readonly Func<Bindable, IList<ParameterInfo>, object[], object[]> dependenciesResolved;

        /// <summary>
        /// 构建一个新的方法容器
        /// </summary>
        /// <param name="container"></param>
        /// <param name="dependenciesResolved">依赖解决器</param>
        internal MethodContainer(Container container, Func<Bindable, IList<ParameterInfo>, object[], object[]> dependenciesResolved)
        {
            this.container = container;
            targetToMethodsMappings = new Dictionary<object, List<string>>();
            methodMappings = new Dictionary<string, MethodBind>();
            this.dependenciesResolved = dependenciesResolved;
        }

        /// <summary>
        /// 绑定一个方法
        /// </summary>
        /// <param name="method">通过这个名字可以调用方法</param>
        /// <param name="target">方法调用目标</param>
        /// <param name="call">在方法调用目标中被调用的方法</param>
        /// <returns></returns>
        public IMethodBind BindMethod(string method, object target, MethodInfo call)
        {
            Guard.NotEmptyOrNull(method, "method");
            Guard.Requires<ArgumentNullException>(target != null);
            Guard.Requires<ArgumentNullException>(call != null);
            return null;
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法调用结果</returns>
        public object Invoke(string method, params object[] userParams)
        {
            Guard.NotEmptyOrNull(method, "method");
            MethodBind methodBind;

            if (!methodMappings.TryGetValue(method, out methodBind))
            {
                throw MakeMethodNotFoundException(method);
            }

            var injectParams = methodBind.ParameterInfos.Length > 0
                ? dependenciesResolved(methodBind, methodBind.ParameterInfos, userParams)
                : new object[] { };

            return methodBind.MethodInfo.Invoke(methodBind.Target, injectParams);
        }

        /// <summary>
        /// 生成一个方法没有找到异常
        /// </summary>
        /// <param name="method"></param>
        private RuntimeException MakeMethodNotFoundException(string method)
        {
            return new RuntimeException("Method [" + method + "] is not found.");
        }
    }
}
