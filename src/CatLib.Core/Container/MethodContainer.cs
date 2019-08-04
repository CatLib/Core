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

using CatLib.Exception;
using CatLib.Support;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CatLib.Container
{
    /// <summary>
    /// The catlib ioc method container implemented.
    /// </summary>
    internal sealed class MethodContainer
    {
        private readonly Dictionary<object, List<string>> targetToMethodsMappings;
        private readonly Dictionary<string, MethodBind> methodMappings;
        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodContainer"/> class.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        internal MethodContainer(Container container)
        {
            this.container = container;
            targetToMethodsMappings = new Dictionary<object, List<string>>();
            methodMappings = new Dictionary<string, MethodBind>();
        }

        /// <summary>
        /// Register a method with the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="target">The invoking target.</param>
        /// <param name="methodInfo">The method info to invoke.</param>
        /// <returns>The method binding data.</returns>
        public IMethodBind Bind(string method, object target, MethodInfo methodInfo)
        {
            Guard.ParameterNotNull(method, nameof(method));
            Guard.ParameterNotNull(methodInfo, nameof(methodInfo));

            if (!methodInfo.IsStatic)
            {
                Guard.Requires<ArgumentNullException>(target != null);
            }

            if (methodMappings.ContainsKey(method))
            {
                throw new LogicException($"Method [{method}] is already {nameof(Bind)}");
            }

            var methodBind = new MethodBind(this, container, method, target, methodInfo);
            methodMappings[method] = methodBind;

            if (target == null)
            {
                return methodBind;
            }

            if (!targetToMethodsMappings.TryGetValue(target, out List<string> targetMappings))
            {
                targetToMethodsMappings[target] = targetMappings = new List<string>();
            }

            targetMappings.Add(method);
            return methodBind;
        }

        /// <summary>
        /// Call the method in bonded container and inject its dependencies.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The return value of method.</returns>
        public object Invoke(string method, params object[] userParams)
        {
            Guard.ParameterNotNull(method, nameof(method));

            if (!methodMappings.TryGetValue(method, out MethodBind methodBind))
            {
                throw MakeMethodNotFoundException(method);
            }

            var injectParams = container.GetDependencies(methodBind, methodBind.ParameterInfos, userParams) ??
                               Array.Empty<object>();
            return methodBind.MethodInfo.Invoke(methodBind.Target, injectParams);
        }

        /// <summary>
        /// Unbinds a method from the container.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// <para><code>string</code> will be taken as the method name.</para>
        /// <para><code>IMethodBind</code> will be taken as a given method.</para>
        /// <para>Other object will be taken as the invoking target.</para>
        /// </param>
        public void Unbind(object target)
        {
            Guard.Requires<ArgumentNullException>(target != null);

            if (target is MethodBind methodBind)
            {
                methodBind.Unbind();
                return;
            }

            if (target is string)
            {
                if (!methodMappings.TryGetValue(target.ToString(), out methodBind))
                {
                    return;
                }

                methodBind.Unbind();
                return;
            }

            UnbindWithObject(target);
        }

        /// <summary>
        /// Flush the container of all method bindings.
        /// </summary>
        public void Flush()
        {
            targetToMethodsMappings.Clear();
            methodMappings.Clear();
        }

        /// <summary>
        /// Unbinds a method from the container.
        /// </summary>
        /// <param name="methodBind">The method binding data.</param>
        internal void Unbind(MethodBind methodBind)
        {
            methodMappings.Remove(methodBind.Service);

            if (methodBind.Target == null)
            {
                return;
            }

            if (!targetToMethodsMappings.TryGetValue(methodBind.Target, out List<string> methods))
            {
                return;
            }

            methods.Remove(methodBind.Service);

            if (methods.Count <= 0)
            {
                targetToMethodsMappings.Remove(methodBind.Target);
            }
        }

        /// <summary>
        /// Create a method without not found exception.
        /// </summary>
        private static LogicException MakeMethodNotFoundException(string method)
        {
            return new LogicException($"Method [{method}] is not found.");
        }

        /// <summary>
        /// Remove all methods bound to the object.
        /// </summary>
        private void UnbindWithObject(object target)
        {
            if (!targetToMethodsMappings.TryGetValue(target, out List<string> methods))
            {
                return;
            }

            foreach (var method in methods.ToArray())
            {
                Unbind(method);
            }
        }
    }
}
