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
using IEnumerator = System.Collections.IEnumerator;
using System.Collections.Generic;
using System.Threading;

namespace CatLib
{
    /// <summary>
    /// CatLib程序
    /// </summary>
    public class Application : Container, IApplication, IOriginalDispatcher
    {
        /// <summary>
        /// 版本号
        /// </summary>
        private static readonly Version version = new Version("1.3.0");

        /// <summary>
        /// 框架启动流程
        /// </summary>
        public enum StartProcess
        {
            /// <summary>
            /// 构建阶段
            /// </summary>
            Construct = 0,

            /// <summary>
            /// 引导流程之前
            /// </summary>
            Bootstrap = 1,

            /// <summary>
            /// 引导流程进行中
            /// </summary>
            Bootstrapping = 2,

            /// <summary>
            /// 引导流程结束之后
            /// </summary>
            Bootstraped = 3,

            /// <summary>
            /// 初始化开始之前
            /// </summary>
            Init = 4,

            /// <summary>
            /// 初始化中
            /// </summary>
            Initing = 5,

            /// <summary>
            /// 初始化完成后
            /// </summary>
            Inited = 6,

            /// <summary>
            /// 框架运行中
            /// </summary>
            Running = 7,

            /// <summary>
            /// 框架终止之前
            /// </summary>
            Terminate = 8,

            /// <summary>
            /// 框架终止进行中
            /// </summary>
            Terminating = 9,

            /// <summary>
            /// 框架终止之后
            /// </summary>
            Terminated = 10,
        }

        /// <summary>
        /// 服务提供者
        /// </summary>
        private readonly SortedList<int, List<IServiceProvider>> serviceProviders 
            = new SortedList<int, List<IServiceProvider>>();

        /// <summary>
        /// 注册服务提供者
        /// </summary>
        private readonly HashSet<Type> serviceProviderTypes = new HashSet<Type>();

        /// <summary>
        /// 是否已经完成引导程序
        /// </summary>
        private bool bootstrapped;

        /// <summary>
        /// 是否已经完成初始化
        /// </summary>
        private bool inited;

        /// <summary>
        /// 是否正在注册中
        /// </summary>
        private bool registering;

        /// <summary>
        /// 启动流程
        /// </summary>
        public StartProcess Process { get; private set; }

        /// <summary>
        /// 增量Id
        /// </summary>
        private long incrementId;

        /// <summary>
        /// 主线程ID
        /// </summary>
        private readonly int mainThreadId;

        /// <summary>
        /// 是否是主线程
        /// </summary>
        public bool IsMainThread => mainThreadId == Thread.CurrentThread.ManagedThreadId;

        /// <summary>
        /// 事件系统
        /// </summary>
        private IDispatcher dispatcher;

        /// <summary>
        /// 事件系统
        /// </summary>
        public IDispatcher Dispatcher => dispatcher ?? (dispatcher = this.Make<IDispatcher>());

        /// <summary>
        /// 调试等级
        /// </summary>
        private DebugLevels debugLevel;

        /// <summary>
        /// 构建一个CatLib实例
        /// </summary>
        /// <param name="global">是否将当前实例应用到全局</param>
        public Application(bool global = true)
        {
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            RegisterCoreAlias();
            RegisterCoreService();
            OnFindType(finder => { return Type.GetType(finder); });
            DebugLevel = DebugLevels.Production;
            Process = StartProcess.Construct;

            if (global)
            {
                App.Handler = this;
            }
        }

        /// <summary>
        /// 构建一个新的Application实例
        /// </summary>
        /// <param name="global">是否将当前实例应用到全局</param>
        /// <returns>Application实例</returns>
        public static Application New(bool global = true)
        {
            return new Application(global);
        }

        /// <summary>
        /// 终止CatLib框架
        /// </summary>
        public virtual void Terminate()
        {
            Process = StartProcess.Terminate;
            Trigger(ApplicationEvents.OnTerminate, this);
            Process = StartProcess.Terminating;
            Flush();
            App.Handler = null;
            Process = StartProcess.Terminated;
            Trigger(ApplicationEvents.OnTerminated, this);
        }

        /// <summary>
        /// 引导程序
        /// </summary>
        /// <param name="bootstraps">引导程序</param>
        /// <returns>CatLib实例</returns>
        /// <exception cref="ArgumentNullException">当引导类型为null时引发</exception>
        public virtual void Bootstrap(params IBootstrap[] bootstraps)
        {
            Guard.Requires<ArgumentNullException>(bootstraps != null);

            if (bootstrapped || Process != StartProcess.Construct)
            {
                throw new CodeStandardException($"Cannot repeatedly trigger the {nameof(Bootstrap)}()");
            }

            Process = StartProcess.Bootstrap;
            Trigger(ApplicationEvents.OnBootstrap, this);
            Process = StartProcess.Bootstrapping;

            var sorting = new SortedList<int, List<IBootstrap>>();
            var existed = new HashSet<IBootstrap>();

            foreach (var bootstrap in bootstraps)
            {
                if (bootstrap == null)
                {
                    continue;
                }

                if (existed.Contains(bootstrap))
                {
                    throw new LogicException($"The bootstrap already exists : {bootstrap}");
                }

                existed.Add(bootstrap);
                AddSortedList(sorting, bootstrap, nameof(IBootstrap.Bootstrap));
            }

            foreach (var sorted in sorting)
            {
                foreach (var bootstrap in sorted.Value)
                {
                    var allow = TriggerHalt(ApplicationEvents.Bootstrapping, bootstrap) == null;
                    if (bootstrap != null && allow)
                    {
                        bootstrap.Bootstrap();
                    }
                }
            }

            Process = StartProcess.Bootstraped;
            bootstrapped = true;
            Trigger(ApplicationEvents.OnBootstraped, this);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init()
        {
            StartCoroutine(CoroutineInit());
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <exception cref="CodeStandardException">没有调用<c>Bootstrap(...)</c>就尝试初始化时触发</exception>
        protected IEnumerator CoroutineInit()
        {
            if (!bootstrapped)
            {
                throw new CodeStandardException($"You must call {nameof(Bootstrap)}() first.");
            }

            if (inited || Process != StartProcess.Bootstraped)
            {
                throw new CodeStandardException($"Cannot repeatedly trigger the {nameof(Init)}()");
            }

            Process = StartProcess.Init;
            Trigger(ApplicationEvents.OnInit, this);
            Process = StartProcess.Initing;

            foreach (var sorted in serviceProviders)
            {
                foreach (var provider in sorted.Value)
                {
                    yield return InitProvider(provider);
                }
            }

            inited = true;
            Process = StartProcess.Inited;
            Trigger(ApplicationEvents.OnInited, this);

            Process = StartProcess.Running;
            Trigger(ApplicationEvents.OnStartCompleted, this);
        }

        /// <summary>
        /// 注册服务提供者
        /// </summary>
        /// <param name="provider">注册服务提供者</param>
        /// <exception cref="LogicException">服务提供者被重复注册时触发</exception>
        public virtual void Register(IServiceProvider provider)
        {
            StartCoroutine(CoroutineRegister(provider));
        }

        /// <summary>
        /// 注册服务提供者
        /// </summary>
        /// <param name="provider">注册服务提供者</param>
        /// <exception cref="LogicException">服务提供者被重复注册时触发</exception>
        protected IEnumerator CoroutineRegister(IServiceProvider provider)
        {
            Guard.Requires<ArgumentNullException>(provider != null);

            if (IsRegisted(provider))
            {
                throw new LogicException($"Provider [{provider.GetType()}] is already register.");
            }

            if (Process == StartProcess.Initing)
            {
                throw new CodeStandardException($"Unable to add service provider during {nameof(StartProcess.Initing)}");
            }

            if (Process > StartProcess.Running)
            {
                throw new CodeStandardException($"Unable to {nameof(Terminate)} in-process registration service provider");
            }

            var allow = TriggerHalt(ApplicationEvents.OnRegisterProvider, provider) == null;
            if (!allow)
            {
                yield break;
            }

            try
            {
                registering = true;
                provider.Register();
            }
            finally
            {
                registering = false;
            }

            AddSortedList(serviceProviders, provider, nameof(IServiceProvider.Init));
            serviceProviderTypes.Add(GetProviderBaseType(provider));

            if (inited)
            {
                yield return InitProvider(provider);
            }
        }

        /// <summary>
        /// 增加到排序列表
        /// </summary>
        /// <param name="list">列表</param>
        /// <param name="insert">需要插入的记录</param>
        /// <param name="priorityMethod">优先级函数</param>
        private void AddSortedList<T>(SortedList<int, List<T>> list, T insert, string priorityMethod)
        {
            var priority = GetPriority(insert.GetType(), priorityMethod );

            if (!list.TryGetValue(priority, out List<T> providers))
            {
                list.Add(priority, providers = new List<T>());
            }

            providers.Add(insert);
        }

        /// <summary>
        /// 初始化服务提供者
        /// </summary>
        /// <param name="provider">服务提供者</param>
        private IEnumerator InitProvider(IServiceProvider provider)
        {
            Trigger(ApplicationEvents.OnProviderInit, provider);

            provider.Init();
            if (provider is ICoroutineInit coroutine)
            {
                yield return coroutine.CoroutineInit();
            }

            Trigger(ApplicationEvents.OnProviderInited, provider);
        }

        /// <summary>
        /// 服务提供者是否已经注册过
        /// </summary>
        /// <param name="provider">服务提供者</param>
        /// <returns>服务提供者是否已经注册过</returns>
        public bool IsRegisted(IServiceProvider provider)
        {
            Guard.Requires<ArgumentNullException>(provider != null);
            return serviceProviderTypes.Contains(GetProviderBaseType(provider));
        }

        /// <summary>
        /// 获取运行时唯一Id
        /// </summary>
        /// <returns>应用程序内唯一id</returns>
        public long GetRuntimeId()
        {
            return Interlocked.Increment(ref incrementId);
        }

        /// <summary>
        /// 获取优先级
        /// </summary>
        /// <param name="type">识别的类型</param>
        /// <param name="method">识别的方法</param>
        /// <returns>优先级</returns>
        public int GetPriority(Type type, string method = null)
        {
            return Util.GetPriority(type, method);
        }

        /// <summary>
        /// 调试等级
        /// </summary>
        public DebugLevels DebugLevel
        {
            get => debugLevel;
            set
            {
                debugLevel = value;
                Instance(Type2Service(typeof(DebugLevels)), debugLevel);
            }
        }

        /// <summary>
        /// 触发一个事件,并获取事件的返回结果
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="payloads">载荷</param>
        /// <returns>事件结果</returns>
        public object[] Trigger(string eventName, params object[] payloads)
        {
            return Dispatcher.Trigger(eventName, payloads);
        }

        /// <summary>
        /// 触发一个事件,遇到第一个事件存在处理结果后终止,并获取事件的返回结果
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="payloads">载荷</param>
        /// <returns>事件结果</returns>
        public object TriggerHalt(string eventName, params object[] payloads)
        {
            return Dispatcher.TriggerHalt(eventName, payloads);
        }

        /// <summary>
        /// 判断给定事件是否存在事件监听器
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="strict">
        /// 严格模式
        /// <para>启用严格模式则不使用正则来进行匹配事件监听器</para>
        /// </param>
        /// <returns>是否存在事件监听器</returns>
        public bool HasListeners(string eventName, bool strict = false)
        {
            return Dispatcher.HasListeners(eventName, strict);
        }

        /// <summary>
        /// 注册一个事件监听器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="execution">事件调用方法</param>
        /// <param name="group">事件分组</param>
        /// <returns>事件对象</returns>
        public IEvent On(string eventName, Func<string, object[], object> execution, object group = null)
        {
            return Dispatcher.On(eventName, execution, group);
        }

        /// <summary>
        /// 解除注册的事件监听器
        /// </summary>
        /// <param name="target">
        /// 事件解除目标
        /// <para>如果传入的是字符串(<code>string</code>)将会解除对应事件名的所有事件</para>
        /// <para>如果传入的是事件对象(<code>IEvent</code>)那么解除对应事件</para>
        /// <para>如果传入的是其他实例(<code>object</code>)会解除该实例下的所有事件</para>
        /// </param>
        public void Off(object target)
        {
            Dispatcher.Off(target);
        }

        /// <summary>
        /// CatLib版本(遵循semver)
        /// </summary>
        [ExcludeFromCodeCoverage]
        public static string Version => version.ToString();

        /// <summary>
        /// 比较CatLib版本(遵循semver)
        /// <para>输入版本大于当前版本则返回<code>-1</code></para>
        /// <para>输入版本等于当前版本则返回<code>0</code></para>
        /// <para>输入版本小于当前版本则返回<code>1</code></para>
        /// </summary>
        /// <param name="major">主版本号</param>
        /// <param name="minor">次版本号</param>
        /// <param name="revised">修订版本号</param>
        /// <returns>比较结果</returns>
        [ExcludeFromCodeCoverage]
        public static int Compare(int major, int minor, int revised)
        {
            return Compare($"{major}.{minor}.{revised}");
        }

        /// <summary>
        /// 比较CatLib版本(遵循semver)
        /// <para>输入版本大于当前版本则返回<code>-1</code></para>
        /// <para>输入版本等于当前版本则返回<code>0</code></para>
        /// <para>输入版本小于当前版本则返回<code>1</code></para>
        /// </summary>
        /// <param name="comparison">版本号</param>
        /// <returns>比较结果</returns>
        [ExcludeFromCodeCoverage]
        public static int Compare(string comparison)
        {
            return version.Compare(comparison);
        }

        /// <summary>
        /// 验证构建状态
        /// </summary>
        /// <param name="method">函数名</param>
        protected override void GuardConstruct(string method)
        {
            if (registering)
            {
                throw new CodeStandardException(
                    $"It is not allowed to make services or dependency injection in the registration process, method:{method}");
            }
            base.GuardConstruct(method);
        }

        /// <summary>
        /// 注册核心别名
        /// </summary>
        private void RegisterCoreAlias()
        {
            var application = Type2Service(typeof(Application));
            Instance(application, this);
            foreach (var type in new[]
            {
                typeof(IApplication),
                typeof(App),
                typeof(IContainer)
            })
            {
                Alias(Type2Service(type), application);
            }
        }

        /// <summary>
        /// 注册核心服务
        /// </summary>
        private void RegisterCoreService()
        {
            var bindable = new BindData(this, null, null, false);
            this.Singleton<GlobalDispatcher>(
                    (_, __) => new GlobalDispatcher(
                        (paramInfos, userParams) => GetDependencies(bindable, paramInfos, userParams)))
                .Alias<IDispatcher>();
        }

        /// <summary>
        /// 获取服务提供者基础类型
        /// </summary>
        /// <param name="provider">服务提供者</param>
        /// <returns>基础类型</returns>
        private Type GetProviderBaseType(IServiceProvider provider)
        {
            var providerType = provider as IServiceProviderType;
            return providerType == null ? provider.GetType() : providerType.BaseType;
        }

        /// <summary>
        /// 启动迭代器
        /// </summary>
        /// <param name="coroutine">迭代程序</param>
        private void StartCoroutine(IEnumerator coroutine)
        {
            var stack = new Stack<IEnumerator>();
            stack.Push(coroutine);
            do
            {
                coroutine = stack.Pop();
                while (coroutine.MoveNext())
                {
                    if (!(coroutine.Current is IEnumerator nextCoroutine))
                    {
                        continue;
                    }

                    stack.Push(coroutine);
                    coroutine = nextCoroutine;
                }
            } while (stack.Count > 0);
        }
    }
}