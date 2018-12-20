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
    ///<summary>
    /// 容器拓展
    /// </summary>
    public static class ContainerExtend
    {
        /// <summary>
        /// 获取服务的绑定数据,如果绑定不存在则返回null
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据或者null</returns>
        public static IBindData GetBind<TService>(this IContainer container)
        {
            return container.GetBind(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 是否已经绑定了服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>代表服务是否被绑定</returns>
        public static bool HasBind<TService>(this IContainer container)
        {
            return container.HasBind(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 是否已经实例静态化
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>是否已经静态化</returns>
        public static bool HasInstance<TService>(this IContainer container)
        {
            return container.HasInstance(container.Type2Service<TService>());
        }

        /// <summary>
        /// 服务是否已经被解决过
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>是否已经被解决过</returns>
        public static bool IsResolved<TService>(this IContainer container)
        {
            return container.IsResolved(container.Type2Service<TService>());
        }

        /// <summary>
        /// 是否可以生成服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务是否可以被构建</returns>
        public static bool CanMake<TService>(this IContainer container)
        {
            return container.CanMake(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 服务是否是静态化的,如果服务不存在也将返回false
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务是否是静态化的</returns>
        public static bool IsStatic<TService>(this IContainer container)
        {
            return container.IsStatic(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 是否是别名
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>是否是别名</returns>
        public static bool IsAlias<TService>(this IContainer container)
        {
            return container.IsAlias(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 为服务设定一个别名
        /// </summary>
        /// <typeparam name="TAlias">别名</typeparam>
        /// <typeparam name="TService">服务名</typeparam>
        public static IContainer Alias<TAlias, TService>(this IContainer container)
        {
            return container.Alias(container.Type2Service(typeof(TAlias)), container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TService), false);
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TConcrete">服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService, TConcrete>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TConcrete), false);
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService>(this IContainer container, Func<IContainer, object[], object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(container.Type2Service(typeof(TService)), concrete, false);
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService>(this IContainer container, Func<object> concrete)
        {
            return container.Bind(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(), false);
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind(this IContainer container, string service,
            Func<IContainer, object[], object> concrete)
        {
            return container.Bind(service, concrete, false);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TConcrete">服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf<TService, TConcrete>(this IContainer container, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), typeof(TConcrete), false, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf<TService>(this IContainer container, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), typeof(TService), false, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf<TService>(this IContainer container, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), concrete, false, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf<TService>(this IContainer container, Func<object> concrete, out IBindData bindData)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.BindIf(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(), false,
                out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool BindIf(this IContainer container, string service,
            Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return container.BindIf(service, concrete, false, out bindData);
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton(this IContainer container, string service,
            Func<IContainer, object[], object> concrete)
        {
            return container.Bind(service, concrete, true);
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TConcrete">服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService, TConcrete>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TConcrete), true);
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TService), true);
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService>(this IContainer container,
            Func<IContainer, object[], object> concrete)
        {
            return container.Bind(container.Type2Service(typeof(TService)), concrete, true);
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService>(this IContainer container,
            Func<object> concrete)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.Bind(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(), true);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TConcrete">服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf<TService, TConcrete>(this IContainer container, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), typeof(TConcrete), true, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf<TService>(this IContainer container, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), typeof(TService), true, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf<TService>(this IContainer container, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return container.BindIf(container.Type2Service(typeof(TService)), concrete, true, out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf<TService>(this IContainer container, Func<object> concrete, out IBindData bindData)
        {
            Guard.Requires<ArgumentNullException>(concrete != null);
            return container.BindIf(container.Type2Service(typeof(TService)), (c, p) => concrete.Invoke(), true,
                out bindData);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>是否完成绑定</returns>
        public static bool SingletonIf(this IContainer container, string service,
            Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return container.BindIf(service, concrete, true, out bindData);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法名</param>
        /// <param name="target">调用目标</param>
        /// <param name="call">调用方法</param>
        public static IMethodBind BindMethod(this IContainer container, string method, object target, string call = null)
        {
            Guard.NotEmptyOrNull(method, nameof(method));
            Guard.Requires<ArgumentNullException>(target != null);

            return container.BindMethod(method, target, target.GetType().GetMethod(call ?? Str.Method(method)));
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod(this IContainer container, string method, Func<object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod<T1>(this IContainer container, string method, Func<T1, object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod<T1, T2>(this IContainer container, string method, Func<T1, T2, object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod<T1, T2, T3>(this IContainer container, string method, Func<T1, T2, T3, object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法名</param>
        /// <param name="callback">调用方法</param>
        public static IMethodBind BindMethod<T1, T2, T3, T4>(this IContainer container, string method, Func<T1, T2, T3, T4, object> callback)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.BindMethod(method, callback.Target, callback.Method);
        }

        /// <summary>
        /// 解除服务绑定
        /// </summary>
        /// <typeparam name="TService">解除绑定的服务</typeparam>
        /// <param name="container">服务容器</param>
        public static void Unbind<TService>(this IContainer container)
        {
            container.Unbind(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 为一个服务定义一个标记
        /// </summary>
        /// <typeparam name="TService">服务</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="tag">标记名</param>
        public static void Tag<TService>(this IContainer container, string tag)
        {
            container.Tag(tag, container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 静态化一个服务,实例值会经过解决修饰器
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="instance">实例值</param>
        public static object Instance<TService>(this IContainer container, object instance)
        {
            return container.Instance(container.Type2Service(typeof(TService)), instance);
        }

        /// <summary>
        /// 释放服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        public static bool Release<TService>(this IContainer container)
        {
            return container.Release(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 根据实例对象释放静态化实例
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="instances">需要释放静态化实例对象</param>
        /// <param name="reverse">以相反的顺序开始释放</param>
        /// <returns>只要有一个没有释放成功那么返回false, <paramref name="instances"/>为没有释放掉的实例</returns>
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
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户参数</param>
        public static void Call<T1>(this IContainer container, Action<T1> method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method, userParams);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户参数</param>
        public static void Call<T1, T2>(this IContainer container, Action<T1, T2> method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method, userParams);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户参数</param>
        public static void Call<T1, T2, T3>(this IContainer container, Action<T1, T2, T3> method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method, userParams);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户参数</param>
        public static void Call<T1, T2, T3, T4>(this IContainer container, Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method, userParams);
        }

        /// <summary>
        /// 以依赖注入形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="target">方法对象</param>
        /// <param name="method">方法名</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法返回值</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/>,<paramref name="method"/>为<c>null</c>或者空字符串</exception>
        public static object Call(this IContainer container, object target, string method, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(target != null);
            Guard.NotEmptyOrNull(method, nameof(method));

            var methodInfo = target.GetType().GetMethod(method);
            return container.Call(target, methodInfo, userParams);
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
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
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
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
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
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
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
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
        /// 构造一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="userParams">用户提供的参数</param>
        /// <returns>服务实例</returns>
        public static TService Make<TService>(this IContainer container, params object[] userParams)
        {
            return (TService)container.Make(container.Type2Service(typeof(TService)), userParams);
        }

        /// <summary>
        /// 构造一个服务
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="type">服务类型</param>
        /// <param name="userParams">用户提供的参数</param>
        /// <returns>服务实例</returns>
        public static object Make(this IContainer container, Type type, params object[] userParams)
        {
            var service = container.Type2Service(type);
            container.BindIf(service, type, false, out IBindData binder);
            return container.Make(service, userParams);
        }

        /// <summary>
        /// 获取一个回调，当执行回调可以生成指定的服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>回调方案</returns>
        public static Func<TService> Factory<TService>(this IContainer container, params object[] userParams)
        {
            return () => (TService)container.Make(container.Type2Service(typeof(TService)), userParams);
        }

        /// <summary>
        /// 扩展容器中的服务
        /// <para>允许在服务构建的过程中配置或者替换服务</para>
        /// <para>如果服务已经被构建，拓展会立即生效。</para>
        /// </summary>
        /// <typeparam name="TService">服务名或别名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="closure">闭包</param>
        public static void Extend<TService>(this IContainer container, Func<TService, IContainer, object> closure)
        {
            container.Extend(container.Type2Service(typeof(TService)), (instance, c) =>
            {
                if (instance is TService)
                {
                    return closure((TService)instance, c);
                }
                return instance;
            });
        }

        /// <summary>
        /// 扩展容器中的服务
        /// <para>允许在服务构建的过程中配置或者替换服务</para>
        /// <para>如果服务已经被构建，拓展会立即生效。</para>
        /// </summary>
        /// <typeparam name="TService">服务名或别名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="closure">闭包</param>
        public static void Extend<TService>(this IContainer container, Func<TService, object> closure)
        {
            container.Extend(container.Type2Service(typeof(TService)), (instance, _) =>
            {
                if (instance is TService)
                {
                    return closure((TService)instance);
                }
                return instance;
            });
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="callback">处理释放时的回调</param>
        /// <returns>当前容器实例</returns>
        public static IContainer OnRelease(this IContainer container, Action<object> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.OnRelease((_, instance) => callback(instance));
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="closure">处理释放时的回调</param>
        /// <returns>当前容器实例</returns>
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
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="closure">处理释放时的回调</param>
        /// <returns>当前容器实例</returns>
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
        /// 当服务被解决时，生成的服务会经过注册的回调函数
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="callback">回调函数</param>
        /// <returns>当前容器对象</returns>
        public static IContainer OnResolving(this IContainer container, Action<object> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.OnResolving((_, instance) =>
            {
                callback(instance);
            });
        }

        /// <summary>
        /// 当服务被解决时，生成的服务会经过注册的回调函数
        /// <para>只有类型和给定的类型相匹配才会被回调</para>
        /// </summary>
        /// <typeparam name="T">指定的类型</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="closure">闭包</param>
        /// <returns>当前容器对象</returns>
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
        /// 当服务被解决时，生成的服务会经过注册的回调函数
        /// <para>只有类型和给定的类型相匹配才会被回调</para>
        /// </summary>
        /// <typeparam name="T">指定的类型</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="closure">闭包</param>
        /// <returns>当前容器对象</returns>
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
        /// 当服务被解决事件之后的回调
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="callback">回调函数</param>
        /// <returns>当前容器对象</returns>
        public static IContainer OnAfterResolving(this IContainer container, Action<object> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);
            return container.OnAfterResolving((_, instance) =>
            {
                callback(instance);
            });
        }

        /// <summary>
        /// 当服务被解决事件之后的回调
        /// <para>只有类型和给定的类型相匹配才会被回调</para>
        /// </summary>
        /// <typeparam name="T">指定的类型</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="closure">闭包</param>
        /// <returns>当前容器对象</returns>
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
        /// 当服务被解决事件之后的回调
        /// <para>只有类型和给定的类型相匹配才会被回调</para>
        /// </summary>
        /// <typeparam name="T">指定的类型</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="closure">闭包</param>
        /// <returns>当前容器对象</returns>
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
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <para>调用是以依赖注入的形式进行的</para>
        /// <para>服务的新建（第一次解决服务）操作并不会触发重定义</para>
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="service">关注的服务名</param>
        /// <param name="target">当服务发生重定义时调用的目标</param>
        /// <param name="methodInfo">方法信息</param>
        public static void Watch(this IContainer container, string service, object target, MethodInfo methodInfo)
        {
            Guard.Requires<ArgumentNullException>(methodInfo != null);

            if (!methodInfo.IsStatic)
            {
                Guard.Requires<ArgumentNullException>(target != null);
            }

            container.OnRebound(service, (instance) =>
            {
                container.Call(target, methodInfo, instance);
            });
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <param>调用是以依赖注入的形式进行的</param>
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="service">关注的服务名</param>
        /// <param name="target">当服务发生重定义时调用的目标</param>
        /// <param name="method">方法名</param>
        public static void Watch(this IContainer container, string service, object target, string method)
        {
            Guard.Requires<ArgumentNullException>(target != null);
            Guard.NotEmptyOrNull(method, nameof(method));

            var methodInfo = target.GetType().GetMethod(method);
            container.Watch(service, target, methodInfo);
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <param>调用是以依赖注入的形式进行的</param>
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="target">当服务发生重定义时调用的目标</param>
        /// <param name="method">方法名</param>
        public static void Watch<TService>(this IContainer container, object target, string method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Watch(container.Type2Service<TService>(), target, method);
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <param>调用是以依赖注入的形式进行的</param>
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="method">回调</param>
        public static void Watch<TService>(this IContainer container, Action method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Watch(container.Type2Service<TService>(), method.Target, method.Method);
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <param>调用是以依赖注入的形式进行的</param>
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="method">回调</param>
        public static void Watch<TService>(this IContainer container, Action<TService> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Watch(container.Type2Service<TService>(), method.Target, method.Method);
        }

        /// <summary>
        /// 在回调区间内暂时性的静态化服务实例
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="callback">回调区间</param>
        /// <param name="service">服务名</param>
        /// <param name="instance">实例名</param>
        public static void Flash(this IContainer container, Action callback, string service, object instance)
        {
            container.Flash(callback, new KeyValuePair<string, object>(service, instance));
        }

        /// <summary>
        /// 类型转为服务名
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务名</returns>
        public static string Type2Service<TService>(this IContainer container)
        {
            return container.Type2Service(typeof(TService));
        }

        /// <summary>
        /// 获取一个回调，当执行回调可以生成指定的服务
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="service">服务名或别名</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>回调方案</returns>
        public static Func<object> Factory(this IContainer container, string service, params object[] userParams)
        {
            return () => container.Make(service, userParams);
        }
    }
}