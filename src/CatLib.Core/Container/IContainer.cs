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
using System.Reflection;

#pragma warning disable CA1716

namespace CatLib.Container
{
    /// <summary>
    /// <see cref="IContainer"/> is the interface implemented by all IOC Container classes.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Resolve the given type from the container.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>The serivce instance. Throw exception if the service can not resolved.</returns>
        object this[string service] { get; set; }

        /// <summary>
        /// Gets the binding data of the given service.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>Return null If there is no binding data.</returns>
        IBindData GetBind(string service);

        /// <summary>
        /// Whether the given service has been bound.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>True if the service has been bound.</returns>
        bool HasBind(string service);

        /// <summary>
        /// Whether the existing instance is exists in the container.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>True if the instance existed.</returns>
        bool HasInstance(string service);

        /// <summary>
        /// Whether the service has been resolved.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>True if the service has been resolved.</returns>
        bool IsResolved(string service);

        /// <summary>
        /// Whether the given service can be made.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>True if the given service can be made.</returns>
        bool CanMake(string service);

        /// <summary>
        /// Whether the given service is singleton bind. false if the service not exists.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>True if the service is singleton bind.</returns>
        bool IsStatic(string service);

        /// <summary>
        /// Whether the given name is an alias.
        /// </summary>
        /// <param name="name">The given name.</param>
        /// <returns>True if the given name is an alias.</returns>
        bool IsAlias(string name);

        /// <summary>
        /// Register a binding with the container.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service type.</param>
        /// <param name="isStatic">Whether the service is singleton bind.</param>
        /// <returns>The binding data.</returns>
        IBindData Bind(string service, Type concrete, bool isStatic);

        /// <summary>
        /// Register a binding with the container.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">Closure return service instance.</param>
        /// <param name="isStatic">Whether the service is singleton bind.</param>
        /// <returns>The binding data.</returns>
        IBindData Bind(string service, Func<IContainer, object[], object> concrete, bool isStatic);

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">Closure return service instance.</param>
        /// <param name="isStatic">Whether the service is singleton bind.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        bool BindIf(string service, Func<IContainer, object[], object> concrete, bool isStatic, out IBindData bindData);

        /// <summary>
        /// Register a binding with the container if the service not exists.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service type.</param>
        /// <param name="isStatic">Whether the service is singleton bind.</param>
        /// <param name="bindData">The binding data.</param>
        /// <returns>True if register a binding with the container.</returns>
        bool BindIf(string service, Type concrete, bool isStatic, out IBindData bindData);

        /// <summary>
        /// Register a method with the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="target">The invoking target.</param>
        /// <param name="called">The method info to invoke.</param>
        /// <returns>The method binding data.</returns>
        IMethodBind BindMethod(string method, object target, MethodInfo called);

        /// <summary>
        /// Unbinds a method from the container.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// <para><code>string</code> will be taken as the method name.</para>
        /// <para><code>IMethodBind</code> will be taken as a given method.</para>
        /// <para>Other object will be taken as the invoking target.</para>
        /// </param>
        void UnbindMethod(object target);

        /// <summary>
        /// Unbinds a service from the container.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        void Unbind(string service);

        /// <summary>
        /// Assign a set of tags to a given binding.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="services">The array of service name or alias.</param>
        void Tag(string tag, params string[] services);

        /// <summary>
        /// Resolve all of the bindings for a given tag.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <returns>All the services tagged with the given tag name.</returns>
        object[] Tagged(string tag);

        /// <summary>
        /// Register an existing instance as shared in the container.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <param name="instance">The service instance.</param>
        /// <returns>New instance after being processed by the decorator.</returns>
        object Instance(string service, object instance);

        /// <summary>
        /// Release an existing instance in the container.
        /// </summary>
        /// <param name="mixed">The service name or alias or instance.</param>
        /// <returns>True if the existing instance has been released.</returns>
        bool Release(object mixed);

        /// <summary>
        /// Flush the container of all bindings and resolved instances.
        /// </summary>
        void Flush();

        /// <summary>
        /// Call the method in bonded container and inject its dependencies.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The return value of method.</returns>
        object Invoke(string method, params object[] userParams);

        /// <summary>
        /// Call the given method and inject its dependencies.
        /// </summary>
        /// <param name="target">The instance on which to call the method.</param>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The return value of method.</returns>
        object Call(object target, MethodInfo methodInfo, params object[] userParams);

        /// <summary>
        /// Resolve the given service or alias from the container.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The serivce instance. Throw exception if the service can not resolved.</returns>
        object Make(string service, params object[] userParams);

        /// <summary>
        /// Alias a service to a different name.
        /// </summary>
        /// <param name="alias">The alias name to service.</param>
        /// <param name="service">The service name.</param>
        /// <returns>The container instance.</returns>
        IContainer Alias(string alias, string service);

        /// <summary>
        /// <see cref="Extend"/> an abstract type in the container.
        /// <para>Allow configuration or replacement of services during service resolving.</para>
        /// </summary>
        /// <param name="service">The service name or alias, null if the apply to gloabl.</param>
        /// <param name="closure">The closure replacement instance.</param>
        void Extend(string service, Func<object, IContainer, object> closure);

        /// <summary>
        /// Register a new resolving callback.
        /// </summary>
        /// <param name="closure">The callback.</param>
        /// <returns>The container instance.</returns>
        IContainer OnResolving(Action<IBindData, object> closure);

        /// <summary>
        /// Register a new after resolving callback.
        /// </summary>
        /// <param name="closure">The callback.</param>
        /// <returns>The container instance.</returns>
        IContainer OnAfterResolving(Action<IBindData, object> closure);

        /// <summary>
        /// Register a new release callback.
        /// </summary>
        /// <param name="closure">The callback.</param>
        /// <returns>The container instance.</returns>
        IContainer OnRelease(Action<IBindData, object> closure);

        /// <summary>
        /// Register a callback for when type finding fails.
        /// </summary>
        /// <param name="func">The callback.</param>
        /// <param name="priority">The priority.</param>
        /// <returns>The container instance.</returns>
        IContainer OnFindType(Func<string, Type> func, int priority = int.MaxValue);

        /// <summary>
        /// Register a new callback to an abstract's rebind event.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>The container instance.</returns>
        IContainer OnRebound(string service, Action<object> callback);

        /// <summary>
        /// Converts the given type to the service name.
        /// </summary>
        /// <param name="type">The given type.</param>
        /// <returns>The service name.</returns>
        string Type2Service(Type type);
    }
}
