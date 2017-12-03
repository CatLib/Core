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

namespace CatLib
{
    ///<summary>
    /// 容器拓展
    /// </summary>
    public static class ContainerExtend
    {
        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        public static void Call<T1>(this IContainer container, Action<T1> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        public static void Call<T1, T2>(this IContainer container, Action<T1, T2> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        public static void Call<T1, T2, T3>(this IContainer container, Action<T1, T2, T3> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="method">方法</param>
        public static void Call<T1, T2, T3, T4>(this IContainer container, Action<T1, T2, T3, T4> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            container.Call(method.Target, method.Method);
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
        /// 以依赖注入形式调用一个方法
        /// </summary>
        /// <param name="container">服务容器</param>
        /// <param name="instance">方法对象</param>
        /// <param name="method">方法名</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法返回值</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/>,<paramref name="method"/>为<c>null</c>或者空字符串</exception>
        public static object Call(this IContainer container, object instance, string method, params object[] userParams)
        {
            Guard.NotNull(instance, "instance");
            Guard.NotEmptyOrNull(method, "method");

            var methodInfo = instance.GetType().GetMethod(method);
            return container.Call(instance, methodInfo, userParams);
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
        /// <typeparam name="TAlias">服务别名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService, TAlias>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TService), true)
                .Alias(container.Type2Service(typeof(TAlias)));
        }

        /// <summary>
        /// 以单例的形式绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Singleton<TService>(this IContainer container) where TService : class
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
            Func<IContainer, object[], object> concrete) where TService : class
        {
            return container.Bind(container.Type2Service(typeof(TService)), concrete, true);
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <typeparam name="TAlias">服务别名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService, TAlias>(this IContainer container)
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TService), false)
                .Alias(container.Type2Service(typeof(TAlias)));
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名，同时也是服务实现</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService>(this IContainer container) where TService : class
        {
            return container.Bind(container.Type2Service(typeof(TService)), typeof(TService), false);
        }

        /// <summary>
        /// 常规绑定一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="concrete">服务实现</param>
        /// <returns>服务绑定数据</returns>
        public static IBindData Bind<TService>(this IContainer container, Func<IContainer, object[], object> concrete)
            where TService : class
        {
            return container.Bind(container.Type2Service(typeof(TService)), concrete, false);
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
        /// 为服务设定一个别名
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="alias">别名</param>
        /// <returns>服务容器</returns>
        public static IContainer Alias<TService>(this IContainer container, string alias)
        {
            return container.Alias(alias, container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 构造一个服务，允许传入构造参数
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="param">构造参数</param>
        /// <returns>服务实例</returns>
        public static TService MakeWith<TService>(this IContainer container, params object[] param)
        {
            return (TService) container.MakeWith(container.Type2Service(typeof(TService)), param);
        }

        /// <summary>
        /// 构造一个服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>服务实例</returns>
        public static TService Make<TService>(this IContainer container)
        {
            return (TService) container.Make(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 构造一个服务
        /// </summary>
        /// <typeparam name="TConvert">服务实例转换到的类型</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="service">服务名或者别名</param>
        /// <returns>服务实例</returns>
        public static TConvert Make<TConvert>(this IContainer container, string service)
        {
            return (TConvert) container.Make(service);
        }

        /// <summary>
        /// 获取一个回调，当执行回调可以生成指定的服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <returns>回调方案</returns>
        public static Func<TService> Factory<TService>(this IContainer container)
        {
            return () => (TService)container.Factory(container.Type2Service(typeof(TService))).Invoke();
        }

        /// <summary>
        /// 获取一个回调，当执行回调可以生成指定的服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="service">服务名或者别名</param>
        /// <returns>回调方案</returns>
        public static Func<TService> Factory<TService>(this IContainer container, string service)
        {
            return () => (TService)container.Factory(service).Invoke();
        }

        /// <summary>
        /// 释放服务
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        public static void Release<TService>(this IContainer container)
        {
            container.Release(container.Type2Service(typeof(TService)));
        }

        /// <summary>
        /// 静态化一个服务,实例值会经过解决修饰器
        /// </summary>
        /// <typeparam name="TService">服务名</typeparam>
        /// <param name="container">服务容器</param>
        /// <param name="instance">实例值</param>
        public static void Instance<TService>(this IContainer container, object instance)
        {
            container.Instance(container.Type2Service(typeof(TService)), instance);
        }
    }
}