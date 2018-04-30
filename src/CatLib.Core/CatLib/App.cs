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
        /// Gets the data to bind to the given service. If there is no data to bind, returns null.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>The data to bind, or null.</returns>
        public static IBindData GetBind(string service)
        {
            return Handler.GetBind(service);
        }

        /// <summary>
        /// 是否已经绑定了服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>返回一个bool值代表服务是否被绑定</returns>
        public static bool HasBind(string service)
        {
            return Handler.HasBind(service);
        }

        /// <summary>
        /// 是否已经实例静态化
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <returns>是否已经静态化</returns>
        public static bool HasInstance<TService>()
        {
#if CATLIB_PERFORMANCE
            return Facade<TService>.HasInstance || Handler.HasInstance<TService>();
#else
            return Handler.HasInstance<TService>();
#endif
        }

        /// <summary>
        /// 服务是否已经被解决过
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <returns>是否已经被解决过</returns>
        public static bool IsResolved<TService>()
        {
            return Handler.IsResolved<TService>();
        }

        /// <summary>
        /// 是否可以生成服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>是否可以生成服务</returns>
        public static bool CanMake(string service)
        {
            return Handler.CanMake(service);
        }

        /// <summary>
        /// 服务是否是静态化的,如果服务不存在也将返回false
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>是否是静态化的</returns>
        public static bool IsStatic(string service)
        {
            return Handler.IsStatic(service);
        }

        /// <summary>
        /// 是否是别名
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>是否是别名</returns>
        public static bool IsAlias(string name)
        {
            return Handler.IsAlias(name);
        }

        /// <summary>
        /// 绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否静态化</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind(string service, Type concrete, bool isStatic)
        {
            return Handler.Bind(service, concrete, isStatic);
        }

        /// <summary>
        /// 绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实体</param>
        /// <param name="isStatic">服务是否静态化</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind(string service, Func<IContainer, object[], object> concrete, bool isStatic)
        {
            return Handler.Bind(service, concrete, isStatic);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>服务绑定数据</returns>
        public static bool BindIf(string service, Func<IContainer, object[], object> concrete, bool isStatic, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, isStatic, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>服务绑定数据</returns>
        public static bool BindIf(string service, Type concrete, bool isStatic, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, isStatic, out bindData);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="target">调用目标</param>
        /// <param name="call">调用方法</param>
        /// <returns>方法绑定数据</returns>
        public static IMethodBind BindMethod(string method, object target, MethodInfo call)
        {
            return Handler.BindMethod(method, target, call);
        }

        /// <summary>
        /// 解除绑定的方法
        /// </summary>
        /// <param name="target">
        /// 解除目标
        /// <para>如果为字符串则作为调用方法名</para>
        /// <para>如果为<code>IMethodBind</code>则作为指定方法</para>
        /// <para>如果为其他对象则作为调用目标做全体解除</para>
        /// </param>
        public static void UnbindMethod(object target)
        {
            Handler.UnbindMethod(target);
        }

        /// <summary>
        /// 解除绑定服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        public static void Unbind(string service)
        {
            Handler.Unbind(service);
        }

        /// <summary>
        /// 为一个及以上的服务定义一个标记
        /// </summary>
        /// <param name="tag">标记名</param>
        /// <param name="service">服务名</param>
        public static void Tag(string tag, params string[] service)
        {
            Handler.Tag(tag, service);
        }

        /// <summary>
        /// 根据标记名生成标记所对应的所有服务实例
        /// </summary>
        /// <param name="tag">标记名</param>
        /// <returns>将会返回标记所对应的所有服务实例</returns>
        public static object[] Tagged(string tag)
        {
            return Handler.Tagged(tag);
        }

        /// <summary>
        /// 静态化一个服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <param name="instance">服务实例</param>
        public static object Instance(string service, object instance)
        {
            return Handler.Instance(service, instance);
        }

        /// <summary>
        /// 释放某个静态化实例
        /// </summary>
        /// <param name="service">服务名或别名</param>
        public static bool Release(string service)
        {
            return Handler.Release(service);
        }

        /// <summary>
        /// 调用一个已经被绑定的方法
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="userParams">用户提供的参数</param>
        /// <returns>调用结果</returns>
        public static object Invoke(string method, params object[] userParams)
        {
            return Handler.Invoke(method, userParams);
        }

        /// <summary>
        /// 以依赖注入形式调用一个方法
        /// </summary>
        /// <param name="instance">方法对象</param>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法返回值</returns>
        public static object Call(object instance, MethodInfo methodInfo, params object[] userParams)
        {
            return Handler.Call(instance, methodInfo, userParams);
        }

        /// <summary>
        /// 构造服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        public static object Make(string service, params object[] userParams)
        {
            return Handler.Make(service, userParams);
        }

        /// <summary>
        /// 获取一个回调，当执行回调可以生成指定的服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>回调方案</returns>
        public static Func<object> Factory(string service)
        {
            return Handler.Factory(service);
        }

        /// <summary>
        /// 为服务设定一个别名
        /// </summary>
        /// <param name="alias">别名</param>
        /// <param name="service">映射到的服务名</param>
        /// <returns>当前容器对象</returns>
        public static IContainer Alias(string alias, string service)
        {
            return Handler.Alias(alias, service);
        }

        /// <summary>
        /// 当服务被解决时触发的事件
        /// </summary>
        /// <param name="func">回调函数</param>
        /// <returns>当前容器实例</returns>
        public static IContainer OnResolving(Func<IBindData, object, object> func)
        {
            return Handler.OnResolving(func);
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="action">处理释放时的回调</param>
        public static IContainer OnRelease(Action<IBindData, object> action)
        {
            return Handler.OnRelease(action);
        }

        /// <summary>
        /// 当查找类型无法找到时会尝试去调用开发者提供的查找类型函数
        /// </summary>
        /// <param name="func">查找类型的回调</param>
        /// <param name="priority">查询优先级(值越小越优先)</param>
        /// <returns>当前容器实例</returns>
        public static IContainer OnFindType(Func<string, Type> func, int priority = int.MaxValue)
        {
            return Handler.OnFindType(func, priority);
        }

        /// <summary>
        /// 当一个已经被解决的服务，发生重定义时触发
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="callback">回调</param>
        /// <returns>服务容器</returns>
        public static IContainer OnRebound(string service, Action<object> callback)
        {
            return Handler.OnRebound(service, callback);
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <para>调用是以依赖注入的形式进行的</para>
        /// <para>服务的新建（第一次解决服务）操作并不会触发重定义</para>
        /// </summary>
        /// <param name="service">关注的服务名</param>
        /// <param name="target">当服务发生重定义时调用的目标</param>
        /// <param name="methodInfo">方法信息</param>
        public static void Watch(string service, object target, MethodInfo methodInfo)
        {
            Handler.Watch(service, target, methodInfo);
        }

        /// <summary>
        /// 在回调区间内暂时性的静态化服务实例
        /// </summary>
        /// <param name="callback">回调区间</param>
        /// <param name="services">服务映射</param>
        public static void Flash(Action callback, params KeyValuePair<string, object>[] services)
        {
            Handler.Flash(callback, services);
        }

        /// <summary>
        /// 类型转为服务名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>转换后的服务名</returns>
        public static string Type2Service(Type type)
        {
            return Handler.Type2Service(type);
        }
        #endregion

        #region Container Extend API
        /// <summary>
        ///  Gets the binding data of the given service. If there is no binding data, returns null.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The binding data, or null.</returns>
        public static IBindData GetBind<TService>()
        {
            return Handler.GetBind<TService>();
        }

        /// <summary>
        /// Checks whether the given service has been bound.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>Whether the given service has been bound.</returns>
        public static bool HasBind<TService>()
        {
            return Handler.HasBind<TService>();
        }

        /// <summary>
        /// Checks whether the given service can be made.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>Whether the given service can be made.</returns>
        public static bool CanMake<TService>()
        {
            return Handler.CanMake<TService>();
        }

        /// <summary>
        /// Checks whether the given service is static. When the service doesn't exist, also returns <code>false</code>.
        /// </summary>
        /// <typeparam name="TService">The service name.</typeparam>
        /// <returns>Whether the given service is static.</returns>
        public static bool IsStatic<TService>()
        {
            return Handler.IsStatic<TService>();
        }

        /// <summary>
        /// Checks whether the given name is an alias.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>Whether the given name is an alias.</returns>
        public static bool IsAlias<TService>()
        {
            return Handler.IsAlias<TService>();
        }

        /// <summary>
        /// Binds a non-static service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The binding data.</returns>
        public static IBindData Bind<TService>()
        {
            return Handler.Bind<TService>();
        }

        /// <summary>
        /// Binds a non-static service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TAlias">The alias type.</typeparam>
        /// <returns>The binding data.</returns>
        public static IBindData Bind<TService, TAlias>()
        {
            return Handler.Bind<TService, TAlias>();
        }

        /// <summary>
        /// Binds a non-static service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implementation.</param>
        /// <returns>The binding data.</returns>
        public static IBindData Bind<TService>(Func<IContainer, object[], object> concrete)
        {
            return Handler.Bind<TService>(concrete);
        }

        /// <summary>
        /// Binds a non-static service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implmentation.</param>
        /// <returns>The binding data.</returns>
        public static IBindData Bind<TService>(Func<object> concrete)
        {
            return Handler.Bind<TService>(concrete);
        }

        /// <summary>
        /// Binds a non-static service.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service implementation.</param>
        /// <returns>The binding data.</returns>
        public static IBindData Bind(string service, Func<IContainer, object[], object> concrete)
        {
            return Handler.Bind(service, concrete);
        }

        /// <summary>
        /// Binds a service if it doesn't exists.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TAlias">The alias type.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool BindIf<TService, TAlias>(out IBindData bindData)
        {
            return Handler.BindIf<TService, TAlias>(out bindData);
        }

        /// <summary>
        /// Binds a service if it doesn't exists.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool BindIf<TService>(out IBindData bindData)
        {
            return Handler.BindIf<TService>(out bindData);
        }

        /// <summary>
        /// Binds a service if it doesn't exists.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TAlias">The alias type.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool BindIf<TService>(Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.BindIf<TService>(concrete, out bindData);
        }

        /// <summary>
        /// Binds a service if it doesn't exists.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implementation.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool BindIf<TService>(Func<object> concrete, out IBindData bindData)
        {
            return Handler.BindIf<TService>(concrete, out bindData);
        }

        /// <summary>
        /// Binds a service if it doesn't exists.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implementation.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool BindIf(string service, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, out bindData);
        }

        /// <summary>
        /// Binds a singleton service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TAlias">The alias type.</typeparam>
        /// <returns>The binding data.</returns>
        public static IBindData Singleton<TService, TAlias>()
        {
            return Handler.Singleton<TService, TAlias>();
        }

        /// <summary>
        /// Binds a singleton service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The binding data.</returns>
        public static IBindData Singleton<TService>()
        {
            return Handler.Singleton<TService>();
        }

        /// <summary>
        /// Binds a singleton service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implementation.</typeparam>
        /// <returns>The binding data.</returns>
        public static IBindData Singleton<TService>(Func<IContainer, object[], object> concrete)
        {
            return Handler.Singleton<TService>(concrete);
        }

        /// <summary>
        /// Binds a singleton service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implementation.</typeparam>
        /// <returns>The binding data.</returns>
        public static IBindData Singleton<TService>(Func<object> concrete)
        {
            return Handler.Singleton<TService>(concrete);
        }

        /// <summary>
        /// Binds a singleton service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implementation.</typeparam>
        /// <returns>The binding data.</returns>
        public static IBindData Singleton(string service, Func<IContainer, object[], object> concrete)
        {
            return Handler.Singleton(service, concrete);
        }

        /// <summary>
        /// Binds a singleton service if it doesn't exist.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="TAlias">The alias type.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool SingletonIf<TService, TAlias>(out IBindData bindData)
        {
            return Handler.SingletonIf<TService, TAlias>(out bindData);
        }

        /// <summary>
        /// Binds a singleton service if it doesn't exist.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool SingletonIf<TService>(out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(out bindData);
        }

        /// <summary>
        /// Binds a singleton service if it doesn't exists.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implementation.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool SingletonIf<TService>(Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(concrete, out bindData);
        }

        /// <summary>
        /// Binds a singleton service if it doesn't exists.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="concrete">The service implementation.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool SingletonIf<TService>(Func<object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(concrete, out bindData);
        }

        /// <summary>
        /// Binds a singleton service if it doesn't exists.
        /// </summary>
        /// <param name="service">The service name.</typeparam>
        /// <param name="concrete">The service implementation.</typeparam>
        /// <param name="bindData">If the binding fails, the history binding data.</param>
        /// <returns>Whether the binding succeeds.</returns>
        public static bool SingletonIf(string service, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf(service, concrete, out bindData);
        }

        /// <summary>
        /// Binds a method to the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="target">The invoking target.</param>
        /// <param name="call">The method name to invoke.</param>
        public static IMethodBind BindMethod(string method, object target,
            string call = null)
        {
            return Handler.BindMethod(method, target, call);
        }

        /// <summary>
        /// Binds a method to the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The callback to invoke.</param>
        public static IMethodBind BindMethod(string method, Func<object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// Binds a method to the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The callback to invoke.</param>
        public static IMethodBind BindMethod<T1>(string method, Func<T1, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// Binds a method to the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The callback to invoke.</param>
        public static IMethodBind BindMethod<T1, T2>(string method, Func<T1, T2, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// Binds a method to the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The callback to invoke.</param>
        public static IMethodBind BindMethod<T1, T2, T3>(string method, Func<T1, T2, T3, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// Binds a method to the container.
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="callback">The callback to invoke.</param>
        public static IMethodBind BindMethod<T1, T2, T3, T4>(string method, Func<T1, T2, T3, T4, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <summary>
        /// Unbinds a service.
        /// </summary>
        /// <typeparam name="TService">The service type to unbind.</typeparam>
        public static void Unbind<TService>()
        {
            Handler.Unbind<TService>();
        }

        /// <summary>
        /// Tags the given service.
        /// </summary>
        /// <typeparam name="TService">The serivce type.</typeparam>
        /// <param name="tag">The tag name.</param>
        public static void Tag<TService>(string tag)
        {
            Handler.Tag<TService>(tag);
        }

        /// <summary>
        /// Makes a service static.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="instance">The service instance.</param>
        public static void Instance<TService>(object instance)
        {
            Handler.Instance<TService>(instance);
        }

        /// <summary>
        /// Releases a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        public static bool Release<TService>()
        {
            return Handler.Release<TService>();
        }

        /// <summary>
        /// Releases a static service according to the given instances.
        /// </summary>
        /// <param name="instances">Instances to release.</param>
        /// <param name="reverse">Whether the releasing procedure goes in the reverse order.</param>
        /// <returns>Returns <code>false</code> if any given instance is not released，where <paramref name="instances"/> contains instances not released.</returns>
        public static bool Release(ref object[] instances, bool reverse = true)
        {
            return Handler.Release(ref instances, reverse);
        }

        /// <summary>
        /// Calls a method with dependency injection.
        /// </summary>
        /// <param name="instance">The instance on which to call the method.</param>
        /// <param name="method">The method name.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The return value of the method.</returns>
        public static object Call(object instance, string method, params object[] userParams)
        {
            return Handler.Call(instance, method, userParams);
        }

        /// <summary>
        /// Calls a method with dependency injection.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="userParams">The user parameters.</param>
        public static void Call<T1>(Action<T1> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <summary>
        /// Calls a method with dependency injection.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="userParams">The user parameters.</param>
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
        /// Calls a method with dependency injection.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="userParams">The user parameters.</param>
        public static void Call<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <summary>
        /// Wraps a method to call with dependency injection.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The wrapped method.</returns>
        public static Action Wrap<T1>(Action<T1> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <summary>
        /// Wraps a method to call with dependency injection.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The wrapped method.</returns>
        public static Action Wrap<T1, T2>(Action<T1, T2> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <summary>
        /// Wraps a method to call with dependency injection.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The wrapped method.</returns>
        public static Action Wrap<T1, T2, T3>(Action<T1, T2, T3> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <summary>
        /// Wraps a method to call with dependency injection.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The wrapped method.</returns>
        public static Action Wrap<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <summary>
        /// Makes a service.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The service instance.</returns>
        public static TService Make<TService>(params object[] userParams)
        {
#if CATLIB_PERFORMANCE
            return Facade<TService>.Make(userParams);
#else
            return Handler.Make<TService>(userParams);
#endif
        }

        /// <summary>
        /// Makes a service.
        /// </summary>
        /// <param name="type">The service type.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The service instance.</returns>
        public static object Make(Type type, params object[] userParams)
        {
            return Handler.Make(type, userParams);
        }

        /// <summary>
        /// Gets a callback, which makes a service when called.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The callback.</returns>
        public static Func<TService> Factory<TService>(params object[] userParams)
        {
            return () => Make<TService>(userParams);
        }

        /// <summary>
        /// Adds a callback for when a static service is released.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns>The current container.</returns>
        public static IContainer OnRelease(Action<object> callback)
        {
            return Handler.OnRelease(callback);
        }

        /// <summary>
        /// Add a callback for when a service is resolved.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns>The current container.</returns>
        public static IContainer OnResolving(Action<object> callback)
        {
            return Handler.OnResolving(callback);
        }

        /// <summary>
        /// Watches the service, so as to invoke the given method on the given target when the service is rebound.
        /// <param>The invocation runs in the form of dependency injection.</param>
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="target">The invocation target.</param>
        /// <param name="method">The method name.</param>
        public static void Watch(string service, object target, string method)
        {
            Handler.Watch(service, target, method);
        }

        /// <summary>
        /// Watches the service, so as to invoke the given method on the given target when the service is rebound.
        /// <param>The invocation runs in the form of dependency injection.</param>
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="target">The invocation target.</param>
        /// <param name="method">The method name.</param>
        public static void Watch<TService>(object target, string method)
        {
            Handler.Watch<TService>(target, method);
        }

        /// <summary>
        /// Watches the service, so as to invoke the given method on the given target when the service is rebound.
        /// <param>The invocation runs in the form of dependency injection.</param>
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="method">The method to invoke.</param>
        public static void Watch<TService>(Action method)
        {
            Handler.Watch<TService>(method);
        }

        /// <summary>
        /// Watches the service, so as to invoke the given method on the given target when the service is rebound.
        /// <param>The invocation runs in the form of dependency injection.</param>
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="method">The method to invoke.</param>
        public static void Watch<TService>(Action<TService> method)
        {
            Handler.Watch(method);
        }

        /// <summary>
        /// Temporarily makes the given service static in the callback lifetime.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="service">The service name.</param>
        /// <param name="instance">The service instance.</param>
        public static void Flash(Action callback, string service, object instance)
        {
            Handler.Flash(callback, service, instance);
        }

        /// <summary>
        /// Converts the given type to the service name.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <returns>The service name.</returns>
        public static string Type2Service<TService>()
        {
            return Handler.Type2Service<TService>();
        }
        #endregion
    }
}
