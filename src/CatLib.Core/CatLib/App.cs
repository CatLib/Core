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
    /// CatLib instance.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class App
    {
        #region Original
        /// <summary>
        /// Callback when a new application instance is created.
        /// </summary>
        public static event Action<IApplication> OnNewApplication;

        /// <summary>
        /// CatLib instance.
        /// </summary>
        private static IApplication instance;

        /// <summary>
        /// Gets or sets the CatLib instance.
        /// </summary>
        public static IApplication Handler
        {
            get
            {
                if (instance == null)
                {
                    Application.New();
                }
                return instance;
            }
            set
            {
                instance = value;
                if (OnNewApplication != null)
                {
                    OnNewApplication.Invoke(instance);
                }
            }
        }
        #endregion

        #region Application API
        /// <summary>
        /// Terminates the CatLib framework.
        /// </summary>
        public static void Terminate()
        {
            Handler.Terminate();
        }

        /// <summary>
        /// Registers the given service provider.
        /// </summary>
        /// <param name="provider">Service provider.</param>
        public static void Register(IServiceProvider provider)
        {
            Handler.Register(provider);
        }

        /// <summary>
        /// Checks whether the given service provider is registered.
        /// </summary>
        /// <param name="provider">Service provider.</param>
        /// <returns>Whether the given service provider is registered.</returns>
        public static bool IsRegisted(IServiceProvider provider)
        {
            return Handler.IsRegisted(provider);
        }

        /// <summary>
        /// Gets the unique runtime ID.
        /// </summary>
        /// <returns>The unique runtime ID.</returns>
        public static long GetRuntimeId()
        {
            return Handler.GetRuntimeId();
        }

        /// <summary>
        /// Gets whether we're on the main thread.
        /// </summary>
        public static bool IsMainThread
        {
            get
            {
                return Handler.IsMainThread;
            }
        }

        /// <summary>
        /// Gets the CatLib version, which complies to semver.
        /// </summary>
        public static string Version
        {
            get
            {
                return Handler.Version;
            }
        }

        /// <summary>
        /// Compares to another CatLib version, which complies to semver.
        /// <para>Returns <code>-1</code> when the input version is greater than the current.</para>
        /// <para>Returns <code>0</code> when the input version is equal to the current.</para>
        /// <para>Returns <code>1</code> when the input version is less than the current.</para>
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="revised">The revised version number.</param>
        /// <returns>The comparison result.</returns>
        public static int Compare(int major, int minor, int revised)
        {
            return Handler.Compare(major, minor, revised);
        }

        /// <summary>
        /// Compares to another CatLib version, which complies to semver.
        /// <para>Returns <code>-1</code> when the input version is greater than the current.</para>
        /// <para>Returns <code>0</code> when the input version is equal to the current.</para>
        /// <para>Returns <code>1</code> when the input version is less than the current.</para>
        /// </summary>
        /// <param name="version">Another version in string.</param>
        /// <returns>The comparison result.</returns>
        public static int Compare(string version)
        {
            return Handler.Compare(version);
        }

        /// <summary>
        /// Gets the prioirty. If there exists a method priority definition then returns it.
        /// Otherwise, returns <c>int.MaxValue</c>.
        /// </summary>
        /// <param name="type">The type of priority to get.</param>
        /// <param name="method">The method via which to get the prioirty.</param>
        /// <returns>The priority.</returns>
        public static int GetPriority(Type type, string method = null)
        {
            return Handler.GetPriority(type, method);
        }

        /// <summary>
        /// Gets or sets the debug level.
        /// </summary>
        public static DebugLevels DebugLevel
        {
            get { return Handler.DebugLevel; }
            set { Handler.DebugLevel = value; }
        }
        #endregion

        #region Dispatcher API
        /// <summary>
        /// Check whether the given event has any listener.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="strict">
        /// Strict mode.
        /// <para>When on, regular expressions are not used to match listeners.</para>
        /// </param>
        /// <returns>Whether the given event has any listener.</returns>
        public static bool HasListeners(string eventName, bool strict = false)
        {
            return Handler.HasListeners(eventName, strict);
        }

        /// <summary>
        /// Triggers an event, and returns the result.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="payloads">The payloads.</param>
        /// <returns>The result of the event.</returns>
        public static object[] Trigger(string eventName, params object[] payloads)
        {
            return Handler.Trigger(eventName, payloads);
        }

        /// <summary>
        /// Triggers an event, halts event dispatching once a listener gives a result and returns this result.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="payloads">The payloads.</param>
        /// <returns>The result of the event.</returns>
        public static object TriggerHalt(string eventName, params object[] payloads)
        {
            return Handler.TriggerHalt(eventName, payloads);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="target">The target of the event.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent On(string eventName, object target, string method = null)
        {
            return Handler.On(eventName, target, method);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent On(string eventName, Action method, object group = null)
        {
            return Handler.On(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent On<T1>(string eventName, Action<T1> method, object group = null)
        {
            return Handler.On(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent On<T1, T2>(string eventName, Action<T1, T2> method, object group = null)
        {
            return Handler.On(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent On<T1, T2, T3>(string eventName, Action<T1, T2, T3> method, object group = null)
        {
            return Handler.On(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent On<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> method, object group = null)
        {
            return Handler.On(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent Listen(string eventName, Func<string, object[], object> execution, object group = null)
        {
            return Handler.On(eventName, execution, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent Listen<TResult>(string eventName, Func<TResult> method, object group = null)
        {
            return Handler.Listen(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent Listen<T1, TResult>(string eventName, Func<T1, TResult> method, object group = null)
        {
            return Handler.Listen(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent Listen<T1, T2, TResult>(string eventName, Func<T1, T2, TResult> method, object group = null)
        {
            return Handler.Listen(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent Listen<T1, T2, T3, TResult>(string eventName, Func<T1, T2, T3, TResult> method, object group = null)
        {
            return Handler.Listen(eventName, method, group);
        }

        /// <summary>
        /// Registers a listener for the given event.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="method">The method to handle the event.</param>
        /// <param name="group">The group of the event.</param>
        /// <returns>The event object.</returns>
        public static IEvent Listen<T1, T2, T3, T4, TResult>(string eventName, Func<T1, T2, T3, T4, TResult> method, object group = null)
        {
            return Handler.Listen(eventName, method, group);
        }

        /// <summary>
        /// Unregisters event listeners.
        /// </summary>
        /// <param name="target">
        /// The target to unregister.
        /// <para>If the target is an event name (<code>string</code>), then all the listeners to this event will be unregistered.</para>
        /// <para>If the target is an event object (<code>IEvent</code>), then this event object will be unregistered.</para>
        /// <para>If the target is something else (<code>object</code>), all the listeners under it will be unregistered.</para>
        /// </param>
        public static void Off(object target)
        {
            Handler.Off(target);
        }
        #endregion

        #region Container API
        /// <summary>
        /// Gets the binding data of the given service. If there is no binding data, returns null.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>The binding data, or null.</returns>
        public static IBindData GetBind(string service)
        {
            return Handler.GetBind(service);
        }

        /// <summary>
        /// Checks whether the given service has been bound.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>Whether the given service has been bound.</returns>
        public static bool HasBind(string service)
        {
            return Handler.HasBind(service);
        }

        /// <summary>
        /// Checks whether the service has been made static.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>Whether the service has been made static.</returns>
        public static bool HasInstance<TService>()
        {
#if CATLIB_PERFORMANCE
            return Facade<TService>.HasInstance || Handler.HasInstance<TService>();
#else
            return Handler.HasInstance<TService>();
#endif
        }

        /// <summary>
        /// Checks whether the service has been resolved.
        /// </summary>
        /// <typeparam name="TService">The service class.</typeparam>
        /// <returns>Whether the service has been resolved.</returns>
        public static bool IsResolved<TService>()
        {
            return Handler.IsResolved<TService>();
        }

        /// <summary>
        /// Checks whether the given service can be made.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>Whether the given service can be made.</returns>
        public static bool CanMake(string service)
        {
            return Handler.CanMake(service);
        }

        /// <summary>
        /// Checks whether the given service is static. When the service doesn't exist, also returns <code>false</code>.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>Whether the given service is static.</returns>
        public static bool IsStatic(string service)
        {
            return Handler.IsStatic(service);
        }

        /// <summary>
        /// Checks whether the given name is an alias.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Whether the given name is an alias.</returns>
        public static bool IsAlias(string name)
        {
            return Handler.IsAlias(name);
        }

        /// <summary>
        /// Binds a service.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service implementation type.</param>
        /// <param name="isStatic">Whether the service is static.</param>
        /// <returns>The binding data.</returns>
        public static IBindData Bind(string service, Type concrete, bool isStatic)
        {
            return Handler.Bind(service, concrete, isStatic);
        }

        /// <summary>
        /// Binds a service.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service implementation type.</param>
        /// <param name="isStatic">Whether the service is static.</param>
        /// <returns>>The binding data..</returns>
        public static IBindData Bind(string service, Func<IContainer, object[], object> concrete, bool isStatic)
        {
            return Handler.Bind(service, concrete, isStatic);
        }

        /// <summary>
        /// Binds a service if it doesn't exists.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service implementation type.</param>
        /// <param name="isStatic">Whether the service is static.</param>
        /// <param name="bindData">If the binding fails, the history bound data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool BindIf(string service, Func<IContainer, object[], object> concrete, bool isStatic, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, isStatic, out bindData);
        }

        /// <summary>
        /// Binds a service if it doesn't exists.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service implementation type.</param>
        /// <param name="isStatic">Whether the service is static.</param>
        /// <param name="bindData">If the binding fails, the history bound data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool BindIf(string service, Type concrete, bool isStatic, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, isStatic, out bindData);
        }

        /// <summary>
        /// Binds a method to the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="target">The invoking target.</param>
        /// <param name="call">The method info to invoke.</param>
        /// <returns>The method binding data.</returns>
        public static IMethodBind BindMethod(string method, object target, MethodInfo call)
        {
            return Handler.BindMethod(method, target, call);
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
        public static void UnbindMethod(object target)
        {
            Handler.UnbindMethod(target);
        }

        /// <summary>
        /// Unbinds a service.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        public static void Unbind(string service)
        {
            Handler.Unbind(service);
        }

        /// <summary>
        /// Tags the given serivces.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="service">The service names.</param>
        public static void Tag(string tag, params string[] service)
        {
            Handler.Tag(tag, service);
        }

        /// <summary>
        /// Makes all the services tagged with the given tag name.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <returns>All the services tagged with the given tag name.</returns>
        public static object[] Tagged(string tag)
        {
            return Handler.Tagged(tag);
        }

        /// <summary>
        /// Makes a service static.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <param name="instance">The service instance.</param>
        public static object Instance(string service, object instance)
        {
            return Handler.Instance(service, instance);
        }

        /// <summary>
        /// Release a static service.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        public static bool Release(string service)
        {
            return Handler.Release(service);
        }

        /// <summary>
        /// Invokes a bound method.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The invocation result.</returns>
        public static object Invoke(string method, params object[] userParams)
        {
            return Handler.Invoke(method, userParams);
        }

        /// <summary>
        /// Calls a method with dependency injection.
        /// </summary>
        /// <param name="instance">The instance on which to call the method.</param>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="userParams">The user params.</param>
        /// <returns>The return value of method.</returns>
        public static object Call(object instance, MethodInfo methodInfo, params object[] userParams)
        {
            return Handler.Call(instance, methodInfo, userParams);
        }

        /// <summary>
        /// Makes a service.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The serivce instance, or null if making fails.</returns>
        public static object Make(string service, params object[] userParams)
        {
            return Handler.Make(service, userParams);
        }

        /// <summary>
        /// Gets a callback, which makes a service when called.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <returns>The callback.</returns>
        public static Func<object> Factory(string service)
        {
            return Handler.Factory(service);
        }

        /// <summary>
        /// Sets an alias to a service.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="service">The serivce name.</param>
        /// <returns>The current container.</returns>
        public static IContainer Alias(string alias, string service)
        {
            return Handler.Alias(alias, service);
        }

        /// <summary>
        /// Add a callback for when a service is resolved.
        /// </summary>
        /// <param name="func">The callback.</param>
        /// <returns>The current container.</returns>
        public static IContainer OnResolving(Func<IBindData, object, object> func)
        {
            return Handler.OnResolving(func);
        }

        /// <summary>
        /// Adds a callback for when a static service is released. 
        /// </summary>
        /// <param name="action">The callback.</param>
        /// <returns>The current container.</returns>
        public static IContainer OnRelease(Action<IBindData, object> action)
        {
            return Handler.OnRelease(action);
        }

        /// <summary>
        /// Adds a callback for when type finding fails.
        /// </summary>
        /// <param name="func">The callback.</param>
        /// <param name="priority">The priority. (The smaller, the higher.)</param>
        /// <returns>The current container.</returns>
        public static IContainer OnFindType(Func<string, Type> func, int priority = int.MaxValue)
        {
            return Handler.OnFindType(func, priority);
        }

        /// <summary>
        /// Adds a callback for when a resolved service is rebound.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="callback">The callback.</param>
        /// <returns>The current container.</returns>
        public static IContainer OnRebound(string service, Action<object> callback)
        {
            return Handler.OnRebound(service, callback);
        }

        /// <summary>
        /// Watches the service, so as to invoke the given method on the given target when the service is rebound.
        /// <para>The invocation runs in the form of dependency injection.</para>
        /// <para>Won't be triggered when the serivce is resovled for the first time.</para>
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="target">The invocation target.</param>
        /// <param name="methodInfo">The method info.</param>
        public static void Watch(string service, object target, MethodInfo methodInfo)
        {
            Handler.Watch(service, target, methodInfo);
        }

        /// <summary>
        /// Temporarily makes the given services static in the callback lifetime.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="services">The services.</param>
        public static void Flash(Action callback, params KeyValuePair<string, object>[] services)
        {
            Handler.Flash(callback, services);
        }

        /// <summary>
        /// Converts the given type to the service name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The service name.</returns>
        public static string Type2Service(Type type)
        {
            return Handler.Type2Service(type);
        }
        #endregion

        #region Container Extend API
        /// <summary>
        /// 获取服务的绑定数据,如果绑定不存在则返回null
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <returns>服务绑定数据或者null</returns>
        public static IBindData GetBind<TService>()
        {
            return Handler.GetBind<TService>();
        }

        /// <summary>
        /// 是否已经绑定了服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <returns>代表服务是否被绑定</returns>
        public static bool HasBind<TService>()
        {
            return Handler.HasBind<TService>();
        }

        /// <summary>
        /// 是否可以生成服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <returns>服务是否可以被构建</returns>
        public static bool CanMake<TService>()
        {
            return Handler.CanMake<TService>();
        }

        /// <summary>
        /// 服务是否是静态化的,如果服务不存在也将返回false
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <returns>服务是否是静态化的</returns>
        public static bool IsStatic<TService>()
        {
            return Handler.IsStatic<TService>();
        }

        /// <summary>
        /// 是否是别名
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <returns>是否是别名</returns>
        public static bool IsAlias<TService>()
        {
            return Handler.IsAlias<TService>();
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService>()
        {
            return Handler.Bind<TService>();
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TAlias">服务别名</typeparam>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService, TAlias>()
        {
            return Handler.Bind<TService, TAlias>();
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService>(Func<IContainer, object[], object> concrete)
        {
            return Handler.Bind<TService>(concrete);
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService>(Func<object> concrete)
        {
            return Handler.Bind<TService>(concrete);
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind(string service, Func<IContainer, object[], object> concrete)
        {
            return Handler.Bind(service, concrete);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TAlias">服务别名</typeparam>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf<TService, TAlias>(out IBindData bindData)
        {
            return Handler.BindIf<TService, TAlias>(out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf<TService>(out IBindData bindData)
        {
            return Handler.BindIf<TService>(out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf<TService>(Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.BindIf<TService>(concrete, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf<TService>(Func<object> concrete, out IBindData bindData)
        {
            return Handler.BindIf<TService>(concrete, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf(string service, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, out bindData);
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TAlias">服务别名</typeparam>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService, TAlias>()
        {
            return Handler.Singleton<TService, TAlias>();
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService>()
        {
            return Handler.Singleton<TService>();
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService>(Func<IContainer, object[], object> concrete)
        {
            return Handler.Singleton<TService>(concrete);
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService>(Func<object> concrete)
        {
            return Handler.Singleton<TService>(concrete);
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton(string service, Func<IContainer, object[], object> concrete)
        {
            return Handler.Singleton(service, concrete);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TAlias">服务别名</typeparam>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf<TService, TAlias>(out IBindData bindData)
        {
            return Handler.SingletonIf<TService, TAlias>(out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf<TService>(out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf<TService>(Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(concrete, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf<TService>(Func<object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(concrete, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf(string service, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf(service, concrete, out bindData);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="target">调用目标</param>
        /// <param name="call">调用方法</param>
        public static IMethodBind BindMethod(string method, object target,
            string call = null)
        {
            return Handler.BindMethod(method, target, call);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod(string method, Func<object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod<T1>(string method, Func<T1, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod<T1, T2>(string method, Func<T1, T2, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod<T1, T2, T3>(string method, Func<T1, T2, T3, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod<T1, T2, T3, T4>(string method, Func<T1, T2, T3, T4, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// 解除服务绑定
        /// </summary>
        /// <typeparam name="TService">解除绑定的服务</typeparam>
        public static void Unbind<TService>()
        {
            Handler.Unbind<TService>();
        }

        /// <summary>
        /// 为一个服务定义一个标记
        /// </summary>
        /// <typeparam name="TService">服务</typeparam>
        /// <param name="tag">标记名</param>
        public static void Tag<TService>(string tag)
        {
            Handler.Tag<TService>(tag);
        }

        /// <summary>
        /// 静态化一个服务,实例值会经过解决修饰器
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="instance">实例值</param>
        public static void Instance<TService>(object instance)
        {
            Handler.Instance<TService>(instance);
        }

        /// <summary>
        /// 释放服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        public static bool Release<TService>()
        {
            return Handler.Release<TService>();
        }

        /// <summary>
        /// 根据实例对象释放静态化实例
        /// </summary>
        /// <param name="instances">需要释放静态化实例对象</param>
        /// <param name="reverse">以相反的顺序释放实例</param>
        /// <returns>只要有一个没有释放成功那么返回false，<paramref name="instances"/>为没有释放掉的实例</returns>
        public static bool Release(ref object[] instances, bool reverse = true)
        {
            return Handler.Release(ref instances, reverse);
        }

        /// <summary>
        /// 以依赖注入形式调用一个方法
        /// </summary>
        /// <param name="instance">方法对象</param>
        /// <param name="method">方法名</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法返回值</returns>
        public static object Call(object instance, string method, params object[] userParams)
        {
            return Handler.Call(instance, method, userParams);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        public static void Call<T1>(Action<T1> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        public static void Call<T1, T2>(Action<T1, T2> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        public static void Call<T1, T2, T3>(Action<T1, T2, T3> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        public static void Call<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
        public static Action Wrap<T1>(Action<T1> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
        public static Action Wrap<T1, T2>(Action<T1, T2> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
        public static Action Wrap<T1, T2, T3>(Action<T1, T2, T3> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
        public static Action Wrap<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <summary>
        /// 构造一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="userParams">用户参数</param>
        /// <returns>服务实例</returns>
        public static TService Make<TService>(params object[] userParams)
        {
#if CATLIB_PERFORMANCE
            return Facade<TService>.Make(userParams);
#else
            return Handler.Make<TService>(userParams);
#endif
        }

        /// <summary>
        /// 构造一个服务
        /// </summary>
        /// <param name="type">服务类型</param>
        /// <param name="userParams">用户提供的参数</param>
        /// <returns>服务实例</returns>
        public static object Make(Type type, params object[] userParams)
        {
            return Handler.Make(type, userParams);
        }

        /// <summary>
        /// 获取一个回调，当执行回调可以生成指定的服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <returns>回调方案</returns>
        public static Func<TService> Factory<TService>(params object[] userParams)
        {
            return () => Make<TService>(userParams);
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="callback">处理释放时的回调</param>
        /// <returns>当前容器实例</returns>
        public static IContainer OnRelease(Action<object> callback)
        {
            return Handler.OnRelease(callback);
        }

        /// <summary>
        /// 当服务被解决时，生成的服务会经过注册的回调函数
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <returns>当前容器对象</returns>
        public static IContainer OnResolving(Action<object> callback)
        {
            return Handler.OnResolving(callback);
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <param>调用是以依赖注入的形式进行的</param>
        /// </summary>
        /// <param name="service">关注的服务名</param>
        /// <param name="target">当服务发生重定义时调用的目标</param>
        /// <param name="method">方法名</param>
        public static void Watch(string service, object target, string method)
        {
            Handler.Watch(service, target, method);
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <param>调用是以依赖注入的形式进行的</param>
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="target">当服务发生重定义时调用的目标</param>
        /// <param name="method">方法名</param>
        public static void Watch<TService>(object target, string method)
        {
            Handler.Watch<TService>(target, method);
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <param>调用是以依赖注入的形式进行的</param>
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="method">回调</param>
        public static void Watch<TService>(Action method)
        {
            Handler.Watch<TService>(method);
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <param>调用是以依赖注入的形式进行的</param>
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="method">回调</param>
        public static void Watch<TService>(Action<TService> method)
        {
            Handler.Watch(method);
        }

        /// <summary>
        /// 在回调区间内暂时性的静态化服务实例
        /// </summary>
        /// <param name="callback">回调区间</param>
        /// <param name="service">服务名</param>
        /// <param name="instance">实例名</param>
        public static void Flash(Action callback, string service, object instance)
        {
            Handler.Flash(callback, service, instance);
        }

        /// <summary>
        /// 类型转为服务名
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns>服务名</returns>
        public static string Type2Service<TService>()
        {
            return Handler.Type2Service<TService>();
        }
        #endregion
    }
}
