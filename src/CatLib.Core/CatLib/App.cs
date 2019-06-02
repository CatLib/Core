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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#pragma warning disable CA1030

namespace CatLib
{
    /// <summary>
    /// The <see cref="IApplication"/> static facade.
    /// </summary>
    [ExcludeFromCodeCoverage]
#pragma warning disable S1118
    public abstract class App
#pragma warning restore S1118
    {
        /// <summary>
        /// The <see cref="IApplication"/> instance.
        /// </summary>
        private static IApplication instance;

        /// <summary>
        /// Callback when a new <see cref="IApplication"/> instance is created.
        /// </summary>
        public static event Action<IApplication> OnNewApplication
        {
            add
            {
                RaiseOnNewApplication += value;
                if (instance != null)
                {
                    value?.Invoke(instance);
                }
            }
            remove => RaiseOnNewApplication -= value;
        }

        /// <summary>
        /// Callback when a new <see cref="IApplication"/> instance is created.
        /// </summary>
        private static event Action<IApplication> RaiseOnNewApplication;

        /// <summary>
        /// Gets or Sets the <see cref="IApplication"/> instance.
        /// </summary>
        public static IApplication Handler
        {
            get
            {
                return instance;
            }

            set
            {
                instance = value;
                RaiseOnNewApplication?.Invoke(instance);
            }
        }

        /// <summary>
        /// Gets a value indicating whether true if the <see cref="IApplication"/> instance exists.
        /// </summary>
        public static bool HasHandler => instance != null;

        /// <inheritdoc cref="IApplication.IsMainThread"/>
        public static bool IsMainThread => Handler.IsMainThread;

        /// <inheritdoc cref="Application.Version"/>
        public static string Version => Application.Version;

        /// <inheritdoc cref="IApplication.DebugLevel"/>
        public static DebugLevel DebugLevel
        {
            get => Handler.DebugLevel;
            set => Handler.DebugLevel = value;
        }

        /// <inheritdoc cref="IApplication.Terminate"/>
        public static void Terminate()
        {
            Handler.Terminate();
        }

        /// <inheritdoc cref="IApplication.Register"/>
        public static void Register(IServiceProvider provider, bool force = false)
        {
            Handler.Register(provider, force);
        }

        /// <inheritdoc cref="IApplication.IsRegistered"/>
        public static bool IsRegistered(IServiceProvider provider)
        {
            return Handler.IsRegistered(provider);
        }

        /// <inheritdoc cref="IApplication.GetRuntimeId"/>
        public static long GetRuntimeId()
        {
            return Handler.GetRuntimeId();
        }

        /// <inheritdoc cref="IApplication.GetPriority"/>
        public static int GetPriority(Type type, string method = null)
        {
            return Handler.GetPriority(type, method);
        }

        /// <inheritdoc cref="IContainer.GetBind"/>
        public static IBindData GetBind(string service)
        {
            return Handler.GetBind(service);
        }

        /// <inheritdoc cref="IContainer.HasBind(string)"/>
        public static bool HasBind(string service)
        {
            return Handler.HasBind(service);
        }

        /// <inheritdoc cref="IContainer.HasInstance(string)"/>
        public static bool HasInstance(string service)
        {
            return Handler.HasInstance(service);
        }

        /// <inheritdoc cref="IContainer.IsResolved(string)"/>
        public static bool IsResolved(string service)
        {
            return Handler.IsResolved(service);
        }

        /// <inheritdoc cref="IContainer.CanMake(string)"/>
        public static bool CanMake(string service)
        {
            return Handler.CanMake(service);
        }

        /// <inheritdoc cref="IContainer.IsStatic(string)"/>
        public static bool IsStatic(string service)
        {
            return Handler.IsStatic(service);
        }

        /// <inheritdoc cref="IContainer.IsAlias(string)"/>
        public static bool IsAlias(string name)
        {
            return Handler.IsAlias(name);
        }

        /// <inheritdoc cref="IContainer.Bind(string, Type, bool)"/>
        public static IBindData Bind(string service, Type concrete, bool isStatic)
        {
            return Handler.Bind(service, concrete, isStatic);
        }

        /// <inheritdoc cref="IContainer.Bind(string, Func{IContainer, object[], object}, bool)"/>
        public static IBindData Bind(string service, Func<IContainer, object[], object> concrete, bool isStatic)
        {
            return Handler.Bind(service, concrete, isStatic);
        }

        /// <inheritdoc cref="IContainer.BindIf(string, Func{IContainer, object[], object}, bool, out IBindData)"/>
        public static bool BindIf(string service, Func<IContainer, object[], object> concrete, bool isStatic, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, isStatic, out bindData);
        }

        /// <inheritdoc cref="IContainer.BindIf(string, Type, bool, out IBindData)"/>
        public static bool BindIf(string service, Type concrete, bool isStatic, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, isStatic, out bindData);
        }

        /// <inheritdoc cref="IContainer.BindMethod(string, object, MethodInfo)"/>
        public static IMethodBind BindMethod(string method, object target, MethodInfo call)
        {
            return Handler.BindMethod(method, target, call);
        }

        /// <inheritdoc cref="IContainer.UnbindMethod(object)"/>
        public static void UnbindMethod(object target)
        {
            Handler.UnbindMethod(target);
        }

        /// <inheritdoc cref="IContainer.Unbind(string)"/>
        public static void Unbind(string service)
        {
            Handler.Unbind(service);
        }

        /// <inheritdoc cref="IContainer.Tag(string, string[])"/>
        public static void Tag(string tag, params string[] service)
        {
            Handler.Tag(tag, service);
        }

        /// <inheritdoc cref="IContainer.Tagged(string)"/>
        public static object[] Tagged(string tag)
        {
            return Handler.Tagged(tag);
        }

        /// <inheritdoc cref="IContainer.Instance(string, object)"/>
        public static object Instance(string service, object instance)
        {
            return Handler.Instance(service, instance);
        }

        /// <inheritdoc cref="IContainer.Release(object)"/>
        public static bool Release(string service)
        {
            return Handler.Release(service);
        }

        /// <inheritdoc cref="IContainer.Invoke(string, object[])"/>
        public static object Invoke(string method, params object[] userParams)
        {
            return Handler.Invoke(method, userParams);
        }

        /// <inheritdoc cref="IContainer.Call(object, MethodInfo, object[])"/>
        public static object Call(object instance, MethodInfo methodInfo, params object[] userParams)
        {
            return Handler.Call(instance, methodInfo, userParams);
        }

        /// <inheritdoc cref="IContainer.Make(string, object[])"/>
        public static object Make(string service, params object[] userParams)
        {
            return Handler.Make(service, userParams);
        }

        /// <inheritdoc cref="IContainer.Alias(string, string)"/>
        public static IContainer Alias(string alias, string service)
        {
            return Handler.Alias(alias, service);
        }

        /// <inheritdoc cref="IContainer.Extend(string, Func{object, IContainer, object})"/>
        public static void Extend(string service, Func<object, IContainer, object> closure)
        {
            Handler.Extend(service, closure);
        }

        /// <inheritdoc cref="IContainer.OnResolving(Action{IBindData, object})"/>
        public static IContainer OnResolving(Action<IBindData, object> closure)
        {
            return Handler.OnResolving(closure);
        }

        /// <inheritdoc cref="IContainer.OnRelease(Action{IBindData, object})"/>
        public static IContainer OnRelease(Action<IBindData, object> action)
        {
            return Handler.OnRelease(action);
        }

        /// <inheritdoc cref="IContainer.OnAfterResolving(Action{IBindData, object})"/>
        public static IContainer OnAfterResolving(Action<IBindData, object> closure)
        {
            return Handler.OnAfterResolving(closure);
        }

        /// <inheritdoc cref="IContainer.OnFindType(Func{string, Type}, int)"/>
        public static IContainer OnFindType(Func<string, Type> func, int priority = int.MaxValue)
        {
            return Handler.OnFindType(func, priority);
        }

        /// <inheritdoc cref="IContainer.OnRebound(string, Action{object})"/>
        public static IContainer OnRebound(string service, Action<object> callback)
        {
            return Handler.OnRebound(service, callback);
        }

        /// <inheritdoc cref="IContainer.Type2Service(Type)"/>
        public static string Type2Service(Type type)
        {
            return Handler.Type2Service(type);
        }

        /// <inheritdoc cref="ExtendContainer.Factory(IContainer, string, object[])"/>
        public static Func<object> Factory(string service, params object[] userParams)
        {
            return Handler.Factory(service, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.GetBind{TService}(IContainer)"/>
        public static IBindData GetBind<TService>()
        {
            return Handler.GetBind<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.HasInstance{TService}(IContainer)"/>
        public static bool HasInstance<TService>()
        {
#if CATLIB_PERFORMANCE
            return Facade<TService>.HasInstance || Handler.HasInstance<TService>();
#else
            return Handler.HasInstance<TService>();
#endif
        }

        /// <inheritdoc cref="ExtendContainer.IsResolved{TService}(IContainer)"/>
        public static bool IsResolved<TService>()
        {
            return Handler.IsResolved<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.HasBind{TService}(IContainer)"/>
        public static bool HasBind<TService>()
        {
            return Handler.HasBind<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.CanMake{TService}(IContainer)"/>
        public static bool CanMake<TService>()
        {
            return Handler.CanMake<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.IsStatic{TService}(IContainer)"/>
        public static bool IsStatic<TService>()
        {
            return Handler.IsStatic<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.IsAlias{TService}(IContainer)"/>
        public static bool IsAlias<TService>()
        {
            return Handler.IsAlias<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.Alias{TAlias, TService}(IContainer)"/>
        public static IContainer Alias<TAlias, TService>()
        {
            return Handler.Alias<TAlias, TService>();
        }

        /// <inheritdoc cref="ExtendContainer.Extend(IContainer, string, Func{object, object})"/>
        public static void Extend(string service, Func<object, object> closure)
        {
            Handler.Extend(service, closure);
        }

        /// <inheritdoc cref="ExtendContainer.Extend{TConcrete}(IContainer, Func{TConcrete, object})"/>
        public static void Extend<TService, TConcrete>(Func<TConcrete, object> closure)
        {
            Handler.Extend<TService, TConcrete>(closure);
        }

        /// <inheritdoc cref="ExtendContainer.Extend{TConcrete}(IContainer, Func{TConcrete, IContainer, object})"/>
        public static void Extend<TService, TConcrete>(Func<TConcrete, IContainer, object> closure)
        {
            Handler.Extend<TService, TConcrete>(closure);
        }

        /// <inheritdoc cref="ExtendContainer.Extend{TService, TConcrete}(IContainer, Func{TConcrete, IContainer, object})"/>
        public static void Extend<TConcrete>(Func<TConcrete, IContainer, object> closure)
        {
            Handler.Extend(closure);
        }

        /// <inheritdoc cref="ExtendContainer.Extend{TService, TConcrete}(IContainer, Func{TConcrete, object})"/>
        public static void Extend<TConcrete>(Func<TConcrete, object> closure)
        {
            Handler.Extend(closure);
        }

        /// <inheritdoc cref="ExtendContainer.Bind{TService}(IContainer)"/>
        public static IBindData Bind<TService>()
        {
            return Handler.Bind<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.Bind{TService, TConcrete}(IContainer)"/>
        public static IBindData Bind<TService, TConcrete>()
        {
            return Handler.Bind<TService, TConcrete>();
        }

        /// <inheritdoc cref="ExtendContainer.Bind(IContainer, Func{IContainer, object[], object})"/>
        public static IBindData Bind<TService>(Func<IContainer, object[], object> concrete)
        {
            return Handler.Bind<TService>(concrete);
        }

        /// <inheritdoc cref="ExtendContainer.Bind{TService}(IContainer, Func{object[], object})"/>
        public static IBindData Bind<TService>(Func<object[], object> concrete)
        {
            return Handler.Bind<TService>(concrete);
        }

        /// <inheritdoc cref="ExtendContainer.Bind{TService}(IContainer, Func{object})"/>
        public static IBindData Bind<TService>(Func<object> concrete)
        {
            return Handler.Bind<TService>(concrete);
        }

        /// <inheritdoc cref="ExtendContainer.Bind(IContainer, string, Func{IContainer, object[], object})"/>
        public static IBindData Bind(string service, Func<IContainer, object[], object> concrete)
        {
            return Handler.Bind(service, concrete);
        }

        /// <inheritdoc cref="ExtendContainer.BindIf{TService, TConcrete}(IContainer, out IBindData)"/>
        public static bool BindIf<TService, TConcrete>(out IBindData bindData)
        {
            return Handler.BindIf<TService, TConcrete>(out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.BindIf{TService}(IContainer, out IBindData)"/>
        public static bool BindIf<TService>(out IBindData bindData)
        {
            return Handler.BindIf<TService>(out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.BindIf{TService}(IContainer, Func{IContainer, object[], object}, out IBindData)"/>
        public static bool BindIf<TService>(Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.BindIf<TService>(concrete, out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.BindIf{TService}(IContainer, Func{object[], object}, out IBindData)"/>
        public static bool BindIf<TService>(Func<object[], object> concrete, out IBindData bindData)
        {
            return Handler.BindIf<TService>(concrete, out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.BindIf{TService}(IContainer, Func{object}, out IBindData)"/>
        public static bool BindIf<TService>(Func<object> concrete, out IBindData bindData)
        {
            return Handler.BindIf<TService>(concrete, out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.BindIf(IContainer, string, Func{IContainer, object[], object}, out IBindData)"/>
        public static bool BindIf(string service, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.BindIf(service, concrete, out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.Singleton{TService, TConcrete}(IContainer)"/>
        public static IBindData Singleton<TService, TConcrete>()
        {
            return Handler.Singleton<TService, TConcrete>();
        }

        /// <inheritdoc cref="ExtendContainer.Singleton{TService}(IContainer)"/>
        public static IBindData Singleton<TService>()
        {
            return Handler.Singleton<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.Singleton{TService}(IContainer, Func{IContainer, object[], object})"/>
        public static IBindData Singleton<TService>(Func<IContainer, object[], object> concrete)
        {
            return Handler.Singleton<TService>(concrete);
        }

        /// <inheritdoc cref="ExtendContainer.Singleton{TService}(IContainer, Func{object[], object})"/>
        public static IBindData Singleton<TService>(Func<object[], object> concrete)
        {
            return Handler.Singleton<TService>(concrete);
        }

        /// <inheritdoc cref="ExtendContainer.Singleton{TService}(IContainer, Func{object})"/>
        public static IBindData Singleton<TService>(Func<object> concrete)
        {
            return Handler.Singleton<TService>(concrete);
        }

        /// <inheritdoc cref="ExtendContainer.Singleton(IContainer, string, Func{IContainer, object[], object})"/>
        public static IBindData Singleton(string service, Func<IContainer, object[], object> concrete)
        {
            return Handler.Singleton(service, concrete);
        }

        /// <inheritdoc cref="ExtendContainer.SingletonIf{TService, TConcrete}(IContainer, out IBindData)"/>
        public static bool SingletonIf<TService, TConcrete>(out IBindData bindData)
        {
            return Handler.SingletonIf<TService, TConcrete>(out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.SingletonIf{TService}(IContainer, out IBindData)"/>
        public static bool SingletonIf<TService>(out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.SingletonIf{TService}(IContainer, Func{IContainer, object[], object}, out IBindData)"/>
        public static bool SingletonIf<TService>(Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(concrete, out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.SingletonIf{TService}(IContainer, Func{object[], object}, out IBindData)"/>
        public static bool SingletonIf<TService>(Func<object[], object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(concrete, out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.SingletonIf{TService}(IContainer, Func{object}, out IBindData)"/>
        public static bool SingletonIf<TService>(Func<object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf<TService>(concrete, out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.SingletonIf(IContainer, string, Func{IContainer, object[], object}, out IBindData)"/>
        public static bool SingletonIf(string service, Func<IContainer, object[], object> concrete, out IBindData bindData)
        {
            return Handler.SingletonIf(service, concrete, out bindData);
        }

        /// <inheritdoc cref="ExtendContainer.BindMethod(IContainer, string, object, string)"/>
        public static IMethodBind BindMethod(string method, object target,
            string call = null)
        {
            return Handler.BindMethod(method, target, call);
        }

        /// <inheritdoc cref="ExtendContainer.BindMethod(IContainer, string, Func{object})"/>
        public static IMethodBind BindMethod(string method, Func<object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <inheritdoc cref="ExtendContainer.BindMethod{T1}(IContainer, string, Func{T1, object})"/>
        public static IMethodBind BindMethod<T1>(string method, Func<T1, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <inheritdoc cref="ExtendContainer.BindMethod{T1, T2}(IContainer, string, Func{T1, T2, object})"/>
        public static IMethodBind BindMethod<T1, T2>(string method, Func<T1, T2, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <inheritdoc cref="ExtendContainer.BindMethod{T1, T2, T3}(IContainer, string, Func{T1, T2, T3, object})"/>
        public static IMethodBind BindMethod<T1, T2, T3>(string method, Func<T1, T2, T3, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <inheritdoc cref="ExtendContainer.BindMethod{T1, T2, T3, T4}(IContainer, string, Func{T1, T2, T3, T4, object})"/>
        public static IMethodBind BindMethod<T1, T2, T3, T4>(string method, Func<T1, T2, T3, T4, object> callback)
        {
            return Handler.BindMethod(method, callback);
        }

        /// <inheritdoc cref="ExtendContainer.Unbind{TService}(IContainer)"/>
        public static void Unbind<TService>()
        {
            Handler.Unbind<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.Tag{TService}(IContainer, string)"/>
        public static void Tag<TService>(string tag)
        {
            Handler.Tag<TService>(tag);
        }

        /// <inheritdoc cref="ExtendContainer.Instance{TService}(IContainer, object)"/>
        public static void Instance<TService>(object instance)
        {
            Handler.Instance<TService>(instance);
        }

        /// <inheritdoc cref="ExtendContainer.Release{TService}(IContainer)"/>
        public static bool Release<TService>()
        {
            return Handler.Release<TService>();
        }

        /// <inheritdoc cref="ExtendContainer.Release(IContainer, ref object[], bool)"/>
        public static bool Release(ref object[] instances, bool reverse = true)
        {
            return Handler.Release(ref instances, reverse);
        }

        /// <inheritdoc cref="ExtendContainer.Call(IContainer, object, string, object[])"/>
        public static object Call(object instance, string method, params object[] userParams)
        {
            return Handler.Call(instance, method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Call{T1}(IContainer, Action{T1}, object[])"/>
        public static void Call<T1>(Action<T1> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Call{T1, T2}(IContainer, Action{T1, T2}, object[])"/>
        public static void Call<T1, T2>(Action<T1, T2> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Call{T1, T2, T3}(IContainer, Action{T1, T2, T3}, object[])"/>
        public static void Call<T1, T2, T3>(Action<T1, T2, T3> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Call{T1, T2, T3, T4}(IContainer, Action{T1, T2, T3, T4}, object[])"/>
        public static void Call<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            Handler.Call(method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Wrap{T1}(IContainer, Action{T1}, object[])"/>
        public static Action Wrap<T1>(Action<T1> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Wrap{T1, T2}(IContainer, Action{T1, T2}, object[])"/>
        public static Action Wrap<T1, T2>(Action<T1, T2> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Wrap{T1, T2, T3}(IContainer, Action{T1, T2, T3}, object[])"/>
        public static Action Wrap<T1, T2, T3>(Action<T1, T2, T3> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Wrap{T1, T2, T3, T4}(IContainer, Action{T1, T2, T3, T4}, object[])"/>
        public static Action Wrap<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            return Handler.Wrap(method, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Make{TService}(IContainer, object[])"/>
        public static TService Make<TService>(params object[] userParams)
        {
#if CATLIB_PERFORMANCE
            return Facade<TService>.Make(userParams);
#else
            return Handler.Make<TService>(userParams);
#endif
        }

        /// <inheritdoc cref="ExtendContainer.Make(IContainer, Type, object[])"/>
        public static object Make(Type type, params object[] userParams)
        {
            return Handler.Make(type, userParams);
        }

        /// <inheritdoc cref="ExtendContainer.Factory{TService}(IContainer, object[])"/>
        public static Func<TService> Factory<TService>(params object[] userParams)
        {
            return Handler.Factory<TService>(userParams);
        }

        /// <inheritdoc cref="ExtendContainer.OnRelease(IContainer, Action{object})"/>
        public static IContainer OnRelease(Action<object> callback)
        {
            return Handler.OnRelease(callback);
        }

        /// <inheritdoc cref="ExtendContainer.OnRelease{T}(IContainer, Action{T})"/>
        public static IContainer OnRelease<TWhere>(Action<TWhere> closure)
        {
            return Handler.OnRelease(closure);
        }

        /// <inheritdoc cref="ExtendContainer.OnRelease{T}(IContainer, Action{IBindData, T})"/>
        public static IContainer OnRelease<TWhere>(Action<IBindData, TWhere> closure)
        {
            return Handler.OnRelease(closure);
        }

        /// <inheritdoc cref="ExtendContainer.OnRelease(IContainer, Action{object})"/>
        public static IContainer OnResolving(Action<object> callback)
        {
            return Handler.OnResolving(callback);
        }

        /// <inheritdoc cref="ExtendContainer.OnResolving{T}(IContainer, Action{T})"/>
        public static IContainer OnResolving<TWhere>(Action<TWhere> closure)
        {
            return Handler.OnResolving(closure);
        }

        /// <inheritdoc cref="ExtendContainer.OnResolving{T}(IContainer, Action{IBindData, T})"/>
        public static IContainer OnResolving<TWhere>(Action<IBindData, TWhere> closure)
        {
            return Handler.OnResolving(closure);
        }

        /// <inheritdoc cref="ExtendContainer.OnAfterResolving(IContainer, Action{object})"/>
        public static IContainer OnAfterResolving(Action<object> closure)
        {
            return Handler.OnAfterResolving(closure);
        }

        /// <inheritdoc cref="ExtendContainer.OnAfterResolving{T}(IContainer, Action{T})"/>
        public static IContainer OnAfterResolving<TWhere>(Action<TWhere> closure)
        {
            return Handler.OnAfterResolving(closure);
        }

        /// <inheritdoc cref="ExtendContainer.OnAfterResolving{T}(IContainer, Action{IBindData, T})"/>
        public static IContainer OnAfterResolving<TWhere>(Action<IBindData, TWhere> closure)
        {
            return Handler.OnAfterResolving(closure);
        }

        /// <inheritdoc cref="ExtendContainer.Watch{TService}(IContainer, Action)"/>
        public static void Watch<TService>(Action method)
        {
            Handler.Watch<TService>(method);
        }

        /// <inheritdoc cref="ExtendContainer.Watch{TService}(IContainer, Action{TService})"/>
        public static void Watch<TService>(Action<TService> method)
        {
            Handler.Watch(method);
        }

        /// <inheritdoc cref="ExtendContainer.Type2Service{TService}(IContainer)"/>
        public static string Type2Service<TService>()
        {
            return Handler.Type2Service<TService>();
        }
    }
}
