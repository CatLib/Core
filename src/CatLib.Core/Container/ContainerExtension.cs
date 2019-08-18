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

#pragma warning disable SA1618

using CatLib.Exception;
using CatLib.Util;
using System;

namespace CatLib.Container
{
    /// <summary>
    /// An extension function for <see cref="Container"/>.
    /// </summary>
    public static class ContainerExtension
    {
        /// <summary>
        /// Gets the binding data of the given service.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>Return null If there is no binding data.</returns>
        public static IBindData GetBind<TService>(this IContainer container)
        {
            return container.GetBind(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Whether the given service has been bound.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>True if the service has been bound.</returns>
        public static bool HasBind<TService>(this IContainer container)
        {
            return container.HasBind(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Whether the existing instance is exists in the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>True if the instance existed.</returns>
        public static bool HasInstance<TService>(this IContainer container)
        {
            return container.HasInstance(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Whether the service has been resolved.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>True if the service has been resolved.</returns>
        public static bool IsResolved<TService>(this IContainer container)
        {
            return container.IsResolved(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Whether the given service can be made.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>True if the given service can be made.</returns>
        public static bool CanMake<TService>(this IContainer container)
        {
            return container.CanMake(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Whether the given service is singleton bind. false if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>True if the service is singleton bind.</returns>
        public static bool IsStatic<TService>(this IContainer container)
        {
            return container.IsStatic(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Whether the given name is an alias.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>True if the given name is an alias.</returns>
        public static bool IsAlias<TService>(this IContainer container)
        {
            return container.IsAlias(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Alias a service to a different name.
        /// </summary>
        /// <typeparam name="TAlias">The alias name.</typeparam>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The container instance.</param>
        /// <returns>Returns the container instance.</returns>
        public static IContainer Alias<TAlias, TService>(this IContainer container)
        {
            return container.Alias(container.Type2Service(typeof(TAlias)), container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Register a binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name (also indicates specific implementation).</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Bind<TService>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TService), false);
        }

        /// <summary>
        /// Register a binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <typeparam name="TConcrete">The service concrete.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Bind<TService, TConcrete>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TConcrete), false);
        }

        /// <summary>
        /// Register a binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Bind<TService>(this IContainer container, Func<IContainer, object[], object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(container.Type2Service(typeof(TService)), concrete, false);
        }

        /// <summary>
        /// Register a binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Bind<TService>(this IContainer container, Func<object[], object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(p), false);
        }

        /// <summary>
        /// Register a binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Bind<TService>(this IContainer container, Func<object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(), false);
        }

        /// <summary>
        /// Register a binding with the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Bind(this IContainer container, string service,
            Func<IContainer, object[], object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(service, concrete, false);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <typeparam name="TConcrete">The service concrete.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool BindIf<TService, TConcrete>(this IContainer container, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), typeof(TConcrete), false, out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name (also indicates specific implementation).</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool BindIf<TService>(this IContainer container, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), typeof(TService), false, out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool BindIf<TService>(this IContainer container, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.BindIf(container.Type2Service(typeof(TService)), concrete, false, out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool BindIf<TService>(this IContainer container, Func<object[], object> concrete, out IBindData bindData)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.BindIf(container.Type2Service(typeof(TService)), (c, @params) => concrete(@params), false,
                out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool BindIf<TService>(this IContainer container, Func<object> concrete, out IBindData bindData)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.BindIf(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(), false,
                out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool BindIf(this IContainer container, string service,
            Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return container.BindIf(service, concrete, false, out bindData);
        }

        /// <summary>
        /// Register a singleton binding with the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Singleton(this IContainer container, string service,
            Func<IContainer, object[], object> concrete)
        {
            return container.Bind(service, concrete, true);
        }

        /// <summary>
        /// Register a singleton binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <typeparam name="TConcrete">The service concrete.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Singleton<TService, TConcrete>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TConcrete), true);
        }

        /// <summary>
        /// Register a singleton binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name (also indicates specific implementation).</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Singleton<TService>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TService), true);
        }

        /// <summary>
        /// Register a singleton binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Singleton<TService>(
            this IContainer container,
            Func<IContainer, object[], object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(container.Type2Service(typeof(TService)), concrete, true);
        }

        /// <summary>
        /// Register a singleton binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Singleton<TService>(this IContainer container, Func<object[], object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(p), true);
        }

        /// <summary>
        /// Register a singleton binding with the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <returns>The service binding data.</returns>
        public static IBindData Singleton<TService>(
            this IContainer container,
            Func<object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(), true);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <typeparam name="TConcrete">The service concrete.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool SingletonIf<TService, TConcrete>(this IContainer container, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), typeof(TConcrete), true, out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name (also indicates specific implementation).</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool SingletonIf<TService>(this IContainer container, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), typeof(TService), true, out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool SingletonIf<TService>(this IContainer container, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), concrete, true, out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool SingletonIf<TService>(this IContainer container, Func<object> concrete, out IBindData bindData)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.BindIf(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(), true,
                out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool SingletonIf<TService>(this IContainer container, Func<object[], object> concrete, out IBindData bindData)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.BindIf(container.Type2Service(typeof(TService)), (c, @params) => concrete(@params), true,
                out bindData);
        }

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service concrete.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        public static bool SingletonIf(this IContainer container, string service,
            Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return container.BindIf(service, concrete, true, out bindData);
        }

        /// <summary>
        /// Register a method with the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method name.</param>
        /// <param name="target">The invoking target.</param>
        /// <param name="call">The method info to invoke.</param>
        /// <returns>Returns the method bind instance.</returns>
        public static IMethodBind BindMethod(this IContainer container, string method, object target, string call = null)
        {
            Guard.ParameterNotNull(method, nameof(method));
            Guard.ParameterNotNull(target, nameof(target));

            return container.BindMethod(method, target, target.GetType().GetMethod(call ?? Str.Method(method)));
        }

        /// <summary>
        /// Register a method with the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The method to invoke.</param>
        /// <returns>Returns the method bind instance.</returns>
        public static IMethodBind BindMethod(this IContainer container, string method, Func<object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// Register a method with the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The method to invoke.</param>
        /// <returns>Returns the method bind instance.</returns>
        public static IMethodBind BindMethod<T1>(this IContainer container, string method, Func<T1, object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// Register a method with the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The method to invoke.</param>
        /// <returns>Returns the method bind instance.</returns>
        public static IMethodBind BindMethod<T1, T2>(this IContainer container, string method, Func<T1, T2, object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// Register a method with the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The method to invoke.</param>
        /// <returns>Returns the method bind instance.</returns>
        public static IMethodBind BindMethod<T1, T2, T3>(this IContainer container, string method, Func<T1, T2, T3, object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// Register a method with the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The method to invoke.</param>
        /// <returns>Returns the method bind instance.</returns>
        public static IMethodBind BindMethod<T1, T2, T3, T4>(this IContainer container, string method, Func<T1, T2, T3, T4, object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// Unbinds a service from the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        public static void Unbind<TService>(this IContainer container)
        {
            container.Unbind(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Assign a set of tags to a given binding.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="tag">The tag name.</param>
        public static void Tag<TService>(this IContainer container, string tag)
        {
            container.Tag(tag, container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Register an existing instance as shared in the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="instance">The service instance.</param>
        /// <returns>Object processed by the decorator.</returns>
        public static object Instance<TService>(this IContainer container, object instance)
        {
            return container.Instance(container.Type2Service(typeof(TService)), instance);
        }

        /// <summary>
        /// Release an existing instance in the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>True if the instance is released. otherwise if instance not exits return false.</returns>
        public static bool Release<TService>(this IContainer container)
        {
            return container.Release(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// Release an existing instance in the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="instances">Service instance that needs to be released.</param>
        /// <param name="reverse">Whether to reverse the release order.</param>
        /// <returns>Returns false if one has not been successfully released, <paramref name="instances"/> is an instance that has not been released.</returns>
        public static bool Release(this IContainer container, ref object[] instances, bool reverse = true)
        {
            if (instances == null || instances.Length <= 0)
            {
                return true;
            }

            if (reverse)
            {
                Array.Reverse(instances);
            }

            var errorIndex = 0;

            for (var index = 0; index < instances.Length; index++)
            {
                if (instances[index] == null)
                {
                    continue;
                }

                if (!container.Release(instances[index]))
                {
                    instances[errorIndex++] = instances[index];
                }
            }

            Array.Resize(ref instances, errorIndex);

            if (reverse && errorIndex > 0)
            {
                Array.Reverse(instances);
            }

            return errorIndex <= 0;
        }

        /// <summary>
        /// Call the given method and inject its dependencies.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method to called.</param>
        /// <param name="userParams">The user parameters.</param>
        public static void Call<T1>(this IContainer container, Action<T1> method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method, userParams);
        }

        /// <summary>
        /// Call the given method and inject its dependencies.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method to called.</param>
        /// <param name="userParams">The user parameters.</param>
        public static void Call<T1, T2>(this IContainer container, Action<T1, T2> method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method, userParams);
        }

        /// <summary>
        /// Call the given method and inject its dependencies.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method to called.</param>
        /// <param name="userParams">The user parameters.</param>
        public static void Call<T1, T2, T3>(this IContainer container, Action<T1, T2, T3> method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method, userParams);
        }

        /// <summary>
        /// Call the given method and inject its dependencies.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method to called.</param>
        /// <param name="userParams">The user parameters.</param>
        public static void Call<T1, T2, T3, T4>(this IContainer container, Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method, userParams);
        }

        /// <summary>
        /// Call the given method and inject its dependencies.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="target">The instance on which to call the method.</param>
        /// <param name="method">The method name.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The return value of method.</returns>
        public static object Call(this IContainer container, object target, string method, params object[] userParams)
        {
            Guard.ParameterNotNull(method, nameof(method));
            Guard.ParameterNotNull(target, nameof(target));

            var methodInfo = target.GetType().GetMethod(method);

            if (methodInfo == null)
            {
                throw new LogicException($"Function \"{method}\" not found.");
            }

            return container.Call(target, methodInfo, userParams);
        }

        /// <summary>
        /// Wrap a method called in a dependency injection form.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method to called.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>Return the wrapper method, which can be called to trigger another method in the form of dependency injection.</returns>
        public static Action Wrap<T1>(this IContainer container, Action<T1> method, params object[] userParams)
        {
            return () =>
            {
                if (method != null)
                {
                    container.Call(method.Target, method.Method, userParams);
                }
            };
        }

        /// <summary>
        /// Wrap a method called in a dependency injection form.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method to called.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>Return the wrapper method, which can be called to trigger another method in the form of dependency injection.</returns>
        public static Action Wrap<T1, T2>(this IContainer container, Action<T1, T2> method, params object[] userParams)
        {
            return () =>
            {
                if (method != null)
                {
                    container.Call(method.Target, method.Method, userParams);
                }
            };
        }

        /// <summary>
        /// Wrap a method called in a dependency injection form.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method to called.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>Return the wrapper method, which can be called to trigger another method in the form of dependency injection.</returns>
        public static Action Wrap<T1, T2, T3>(this IContainer container, Action<T1, T2, T3> method, params object[] userParams)
        {
            return () =>
            {
                if (method != null)
                {
                    container.Call(method.Target, method.Method, userParams);
                }
            };
        }

        /// <summary>
        /// Wrap a method called in a dependency injection form.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The method to called.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>Return the wrapper method, which can be called to trigger another method in the form of dependency injection.</returns>
        public static Action Wrap<T1, T2, T3, T4>(this IContainer container, Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            return () =>
            {
                if (method != null)
                {
                    container.Call(method.Target, method.Method, userParams);
                }
            };
        }

        /// <summary>
        /// Resolve the given service or alias from the container.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The serivce instance. Throw exception if the service can not resolved.</returns>
        public static TService Make<TService>(this IContainer container, params object[] userParams)
        {
            return (TService)container.Make(container.Type2Service(typeof(TService)), userParams);
        }

        /// <summary>
        /// Resolve the given service or alias from the container.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="type">The service type.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The serivce instance. Throw exception if the service can not resolved.</returns>
        public static object Make(this IContainer container, Type type, params object[] userParams)
        {
            var service = container.Type2Service(type);
            container.BindIf(service, type, false, out _);
            return container.Make(service, userParams);
        }

        /// <summary>
        /// Lazially resolve a service that is built when the call returns a callback.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The callback.</returns>
        public static Func<TService> Factory<TService>(this IContainer container, params object[] userParams)
        {
            return () => (TService)container.Make(container.Type2Service(typeof(TService)), userParams);
        }

        /// <summary>
        /// <see cref="Extend"/> an abstract type in the container.
        /// <para>Allow configuration or replacement of services during service resolving.</para>
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="service">The service name.</param>
        /// <param name="closure">The closure.</param>
        public static void Extend(this IContainer container, string service, Func<object, object> closure)
        {
            container.Extend(service, (instance, c) => closure(instance));
        }

        /// <summary>
        /// <see cref="Extend"/> an abstract type in the container.
        /// <para>Allow configuration or replacement of services during service resolving.</para>
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The closure.</param>
        public static void Extend<TService, TConcrete>(this IContainer container, Func<TConcrete, object> closure)
        {
            container.Extend(container.Type2Service(typeof(TService)), (instance, c) => closure((TConcrete)instance));
        }

        /// <summary>
        /// <see cref="Extend"/> an abstract type in the container.
        /// <para>Allow configuration or replacement of services during service resolving.</para>
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The closure.</param>
        public static void Extend<TService, TConcrete>(this IContainer container, Func<TConcrete, IContainer, object> closure)
        {
            container.Extend(
                container.Type2Service(typeof(TService)),
                (instance, c) => closure((TConcrete)instance, c));
        }

        /// <summary>
        /// <see cref="Extend"/> an abstract type in the container.
        /// <para>Allow configuration or replacement of services during service resolving.</para>
        /// </summary>
        /// <typeparam name="TConcrete">Expected type or interface.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The closure.</param>
        public static void Extend<TConcrete>(this IContainer container, Func<TConcrete, IContainer, object> closure)
        {
            container.Extend(null, (instance, c) =>
            {
                if (instance is TConcrete)
                {
                    return closure((TConcrete)instance, c);
                }

                return instance;
            });
        }

        /// <summary>
        /// <see cref="Extend"/> an abstract type in the container.
        /// <para>Allow configuration or replacement of services during service resolving.</para>
        /// </summary>
        /// <typeparam name="TConcrete">Expected type or interface.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The closure.</param>
        public static void Extend<TConcrete>(this IContainer container, Func<TConcrete, object> closure)
        {
            container.Extend(null, (instance, _) =>
            {
                if (instance is TConcrete)
                {
                    return closure((TConcrete)instance);
                }

                return instance;
            });
        }

        /// <summary>
        /// Register a new release callback.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnRelease(this IContainer container, Action<object> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.OnRelease((_, instance) => callback(instance));
        }

        /// <summary>
        /// Register a new release callback.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The callback.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnRelease<T>(this IContainer container, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return container.OnRelease((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <summary>
        /// Register a new release callback.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The callback.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnRelease<T>(this IContainer container, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return container.OnRelease((bindData, instance) =>
            {
                if (instance is T)
                {
                    closure(bindData, (T)instance);
                }
            });
        }

        /// <summary>
        /// Register a new resolving callback.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnResolving(this IContainer container, Action<object> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.OnResolving((_, instance) =>
            {
                callback(instance);
            });
        }

        /// <summary>
        /// Register a new resolving callback.
        /// <para>Only the type matches the given type will be called back.</para>
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The closure.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnResolving<T>(this IContainer container, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return container.OnResolving((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <summary>
        /// Register a new resolving callback.
        /// <para>Only the type matches the given type will be called back.</para>
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The closure.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnResolving<T>(this IContainer container, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return container.OnResolving((bindData, instance) =>
            {
                if (instance is T)
                {
                    closure(bindData, (T)instance);
                }
            });
        }

        /// <summary>
        /// Register a new after resolving callback.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnAfterResolving(this IContainer container, Action<object> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.OnAfterResolving((_, instance) =>
            {
                callback(instance);
            });
        }

        /// <summary>
        /// Register a new after resolving callback.
        /// <para>Only the type matches the given type will be called back.</para>
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The closure.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnAfterResolving<T>(this IContainer container, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return container.OnAfterResolving((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <summary>
        /// Register a new after resolving callback.
        /// <para>Only the type matches the given type will be called back.</para>
        /// </summary>
        /// <typeparam name="T">The specified type.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="closure">The closure.</param>
        /// <returns>Returns the <see cref="IContainer"/> instance.</returns>
        public static IContainer OnAfterResolving<T>(this IContainer container, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return container.OnAfterResolving((bindData, instance) =>
            {
                if (instance is T)
                {
                    closure(bindData, (T)instance);
                }
            });
        }

        /// <summary>
        /// Watch the specified service, trigger callback when rebinding the service.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The callback.</param>
        public static void Watch<TService>(this IContainer container, Action method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.OnRebound(container.Type2Service(typeof(TService)), (instance) => method());
        }

        /// <summary>
        /// Watch the specified service, trigger callback when rebinding the service.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="method">The callback.</param>
        public static void Watch<TService>(this IContainer container, Action<TService> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.OnRebound(container.Type2Service(typeof(TService)), (instance) => method((TService)instance));
        }

        /// <summary>
        /// Converts the given type to the service name.
        /// </summary>
        /// <typeparam name="TService">The given type.</typeparam>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <returns>The service name.</returns>
        public static string Type2Service<TService>(this IContainer container)
        {
            return container.Type2Service(typeof(TService));
        }

        /// <summary>
        /// Lazially resolve a service that is built when the call returns a callback.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> instance.</param>
        /// <param name="service">The service name or alias.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The callback.</returns>
        public static Func<object> Factory(this IContainer container, string service, params object[] userParams)
        {
            return () => container.Make(service, userParams);
        }
    }
}
