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
using System.Collections.Generic;
using System.Reflection;

namespace CatLib
{
    /// <summary>
    /// The catlib ioc method container implemented.
    /// </summary>
    internal sealed class MethodContainer
    {
        /// <summary>
        /// Call method target map to method names.
        /// </summary>
        private readonly Dictionary<object, List<string>> targetToMethodsMappings;

        /// <summary>
        /// An map of the method bing data.
        /// </summary>
        private readonly Dictionary<string, MethodBind> methodMappings;

        /// <summary>
        /// The <see cref="Container"/> instnace.
        /// </summary>
        private readonly Container container;

        /// <summary>
        /// The sync lock.
        /// </summary>
        private readonly object syncRoot;

        /// <summary>
        /// Initialize a new one <see cref="MethodContainer"/> instance.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> instance.</param>
        internal MethodContainer(Container container)
        {
            this.container = container;
            targetToMethodsMappings = new Dictionary<object, List<string>>();
            methodMappings = new Dictionary<string, MethodBind>();
            syncRoot = new object();
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
            Guard.NotEmptyOrNull(method, nameof(method));
            Guard.Requires<ArgumentNullException>(methodInfo != null);

            if (!methodInfo.IsStatic)
            {
                Guard.Requires<ArgumentNullException>(target != null);
            }

            lock (syncRoot)
            {
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
        }

        /// <summary>
        /// Call the method in bonded container and inject its dependencies.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The return value of method.</returns>
        public object Invoke(string method, params object[] userParams)
        {
            Guard.NotEmptyOrNull(method, nameof(method));

            lock (syncRoot)
            {
                if (!methodMappings.TryGetValue(method, out MethodBind methodBind))
                {
                    throw MakeMethodNotFoundException(method);
                }

                var injectParams = container.GetDependencies(methodBind, methodBind.ParameterInfos, userParams) ??
                                   new object[] { };
                return methodBind.MethodInfo.Invoke(methodBind.Target, injectParams);
            }
        }

        /// <summary>
        /// Unbinds a method from the container.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// <para>A <code>string</code> will be taken as the method name.</para>
        /// <para>A <code>IMethodBind</code> will be taken as a given method.</para>
        /// <para>Other object will be taken as the invoking target.</para>
        /// </param>
        public void Unbind(object target)
        {
            Guard.Requires<ArgumentNullException>(target != null);

            lock (syncRoot)
            {
                var methodBind = target as MethodBind;

                if (methodBind != null)
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
        }

        /// <summary>
        /// Unbinds a method from the container.
        /// </summary>
        /// <param name="methodBind">The method binding data.</param>
        internal void Unbind(MethodBind methodBind)
        {
            lock (syncRoot)
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
        }

        /// <summary>
        /// Remove all methods bound to the object.
        /// </summary>
        /// <param name="target">The object.</param>
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

        /// <summary>
        /// Flush the container of all method bindings.
        /// </summary>
        public void Flush()
        {
            lock (syncRoot)
            {
                targetToMethodsMappings.Clear();
                methodMappings.Clear();
            }
        }

        /// <summary>
        /// Create a method without not found exception.
        /// </summary>
        /// <param name="method">The method name.</param>
        private LogicException MakeMethodNotFoundException(string method)
        {
            return new LogicException($"Method [{method}] is not found.");
        }
    }
}
