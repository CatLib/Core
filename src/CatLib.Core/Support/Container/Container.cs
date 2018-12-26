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
using System.Text;

namespace CatLib
{
    ///<summary>
    /// 依赖注入容器
    /// </summary>
    public class Container : IContainer
    {
        /// <summary>
        /// 服务所绑定的相关数据，记录了服务的关系
        /// </summary>
        private readonly Dictionary<string, BindData> binds;

        ///<summary>
        /// 如果所属服务是静态的那么构建后将会储存在这里
        ///</summary>
        private readonly Dictionary<string, object> instances;

        /// <summary>
        /// 单例化对象的反查表
        /// </summary>
        private readonly Dictionary<object, string> instancesReverse;

        ///<summary>
        /// 服务的别名(key: 别名 , value: 映射的服务名)
        ///</summary>
        private readonly Dictionary<string, string> aliases;

        /// <summary>
        /// 可以通过服务的真实名字来查找别名
        /// </summary>
        private readonly Dictionary<string, List<string>> aliasesReverse;

        /// <summary>
        /// 服务标记，一个标记允许标记多个服务
        /// </summary>
        private readonly Dictionary<string, List<string>> tags;

        /// <summary>
        /// 服务构建时的修饰器
        /// </summary>
        private readonly List<Action<IBindData, object>> resolving;

        /// <summary>
        /// 在服务构建修饰器之后的修饰器
        /// </summary>
        private readonly List<Action<IBindData, object>> afterResloving;

        /// <summary>
        /// 静态服务释放时的修饰器
        /// </summary>
        private readonly List<Action<IBindData, object>> release;

        /// <summary>
        /// 全局服务扩展方法
        /// </summary>
        private readonly Dictionary<string, List<Func<object, IContainer, object>>> extenders;

        /// <summary>
        /// 类型查询回调
        /// 当类型无法被解决时会尝试去开发者提供的查询器中查询类型
        /// </summary>
        private readonly SortSet<Func<string, Type>, int> findType;

        /// <summary>
        /// 类型查询回调缓存
        /// </summary>
        private readonly Dictionary<string, Type> findTypeCache;

        /// <summary>
        /// 已经被解决过的服务名
        /// </summary>
        private readonly HashSet<string> resolved;

        /// <summary>
        /// 单例服务构建时序
        /// </summary>
        private readonly SortSet<string, int> instanceTiming;

        /// <summary>
        /// 重定义事件
        /// </summary>
        private readonly Dictionary<string, List<Action<object>>> rebound;

        /// <summary>
        /// 方法容器
        /// </summary>
        private readonly MethodContainer methodContainer;

        /// <summary>
        /// 同步锁
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// 注入目标
        /// </summary>
        private readonly Type injectTarget;

        /// <summary>
        /// 编译堆栈
        /// </summary>
        protected Stack<string> BuildStack { get; }

        /// <summary>
        /// 用户参数堆栈
        /// </summary>
        protected Stack<object[]> UserParamsStack { get; }

        /// <summary>
        /// 是否在清空过程中
        /// </summary>
        private bool flushing;

        /// <summary>
        /// 单例化Id
        /// </summary>
        private int instanceId;

        /// <summary>
        /// 服务禁用字符
        /// </summary>
        private static readonly char[] ServiceBanChars = { '@', ':', '$' };

        /// <summary>
        /// 构造一个容器
        /// </summary>
        /// <param name="prime">初始预计服务数量</param>
        public Container(int prime = 64)
        {
            prime = Math.Max(8, prime);
            tags = new Dictionary<string, List<string>>((int)(prime * 0.25));
            aliases = new Dictionary<string, string>(prime * 4);
            aliasesReverse = new Dictionary<string, List<string>>(prime * 4);
            instances = new Dictionary<string, object>(prime * 4);
            instancesReverse = new Dictionary<object, string>(prime * 4);
            binds = new Dictionary<string, BindData>(prime * 4);
            resolving = new List<Action<IBindData, object>>((int)(prime * 0.25));
            afterResloving = new List<Action<IBindData, object>>((int)(prime * 0.25));
            release = new List<Action<IBindData, object>>((int)(prime * 0.25));
            extenders = new Dictionary<string, List<Func<object, IContainer, object>>>((int)(prime * 0.25));
            resolved = new HashSet<string>();
            findType = new SortSet<Func<string, Type>, int>();
            findTypeCache = new Dictionary<string, Type>(prime * 4);
            rebound = new Dictionary<string, List<Action<object>>>(prime);
            instanceTiming = new SortSet<string, int>();
            instanceTiming.ReverseIterator(false);
            BuildStack = new Stack<string>(32);
            UserParamsStack = new Stack<object[]>(32);

            injectTarget = typeof(InjectAttribute);
            methodContainer = new MethodContainer(this, GetDependencies);
            flushing = false;
            instanceId = 0;
        }

        /// <summary>
        /// 为一个及以上的服务定义一个标记
        /// 如果标记已经存在那么服务会被追加进列表
        /// </summary>
        /// <param name="tag">标记名</param>
        /// <param name="service">服务名或者别名</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>为<c>null</c>或者<paramref name="service"/>中的元素为<c>null</c>或者空字符串</exception>
        public void Tag(string tag, params string[] service)
        {
            Guard.NotEmptyOrNull(tag, nameof(tag));
            Guard.NotNull(service, nameof(service));
            Guard.CountGreaterZero(service, nameof(service));
            Guard.ElementNotEmptyOrNull(service, nameof(service));

            lock (syncRoot)
            {
                GuardFlushing();
                if (!tags.TryGetValue(tag, out List<string> list))
                {
                    tags[tag] = list = new List<string>();
                }
                list.AddRange(service);
            }
        }

        /// <summary>
        /// 根据标记名生成标记所对应的所有服务实例
        /// </summary>
        /// <param name="tag">标记名</param>
        /// <returns>将会返回标记所对应的所有服务实例</returns>
        /// <exception cref="LogicException"><paramref name="tag"/>不存在</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tag"/>为<c>null</c>或者空字符串</exception>
        public object[] Tagged(string tag)
        {
            Guard.NotEmptyOrNull(tag, nameof(tag));
            lock (syncRoot)
            {
                if (!tags.TryGetValue(tag, out List<string> services))
                {
                    throw new LogicException($"Tag [{tag}] is not exist.");
                }

                var result = new object[services.Count];
                for (var i = 0; i < services.Count; i++)
                {
                    result[i] = Make(services[i]);
                }

                return result;
            }
        }

        /// <summary>
        /// 获取服务的绑定数据,如果绑定不存在则返回null（只有进行过bind才视作绑定）
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>服务绑定数据或者null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>为<c>null</c>或者空字符串</exception>
        public IBindData GetBind(string service)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            lock (syncRoot)
            {
                service = AliasToService(service);
                return binds.TryGetValue(service, out BindData bindData) ? bindData : null;
            }
        }

        /// <summary>
        /// 是否已经绑定了服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>服务是否被绑定</returns>
        public bool HasBind(string service)
        {
            return GetBind(service) != null;
        }

        /// <summary>
        /// 是否已经实例静态化
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>是否已经静态化</returns>
        public bool HasInstance(string service)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            lock (syncRoot)
            {
                service = AliasToService(service);
                return instances.ContainsKey(service);
            }
        }

        /// <summary>
        /// 服务是否已经被解决过
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>是否已经被解决过</returns>
        public bool IsResolved(string service)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            lock (syncRoot)
            {
                service = AliasToService(service);
                return resolved.Contains(service) || instances.ContainsKey(service);
            }
        }

        /// <summary>
        /// 是否可以生成服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>是否可以生成服务</returns>
        public bool CanMake(string service)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            lock (syncRoot)
            {
                service = AliasToService(service);

                if (HasBind(service) || HasInstance(service))
                {
                    return true;
                }

                var type = SpeculatedServiceType(service);
                return !IsBasicType(type) && !IsUnableType(type);
            }
        }

        /// <summary>
        /// 服务是否是静态化的,如果服务不存在也将返回false
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>是否是静态化的</returns>
        public bool IsStatic(string service)
        {
            var bind = GetBind(service);
            return bind != null && bind.IsStatic;
        }

        /// <summary>
        /// 是否是别名
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>是否是别名</returns>
        public bool IsAlias(string name)
        {
            name = FormatService(name);
            return aliases.ContainsKey(name);
        }

        /// <summary>
        /// 以全局的方式为服务设定一个别名
        /// </summary>
        /// <param name="alias">别名</param>
        /// <param name="service">映射到的服务名</param>
        /// <returns>当前容器对象</returns>
        /// <exception cref="LogicException"><paramref name="alias"/>别名冲突或者<paramref name="service"/>的绑定与实例都不存在</exception>
        /// <exception cref="ArgumentNullException"><paramref name="alias"/>,<paramref name="service"/>为<c>null</c>或者空字符串</exception>
        public IContainer Alias(string alias, string service)
        {
            Guard.NotEmptyOrNull(alias, nameof(alias));
            Guard.NotEmptyOrNull(service, nameof(service));

            if (alias == service)
            {
                throw new LogicException($"Alias is same as service name: [{alias}].");
            }

            alias = FormatService(alias);
            service = FormatService(service);

            lock (syncRoot)
            {
                GuardFlushing();
                if (aliases.ContainsKey(alias))
                {
                    throw new LogicException($"Alias [{alias}] is already exists.");
                }

                if (!binds.ContainsKey(service) && !instances.ContainsKey(service))
                {
                    throw new CodeStandardException(
                        $"You must {nameof(Bind)}() or {nameof(Instance)}() serivce before you can call {nameof(Alias)}().");
                }

                aliases.Add(alias, service);

                if (!aliasesReverse.TryGetValue(service, out List<string> serviceList))
                {
                    aliasesReverse[service] = serviceList = new List<string>();
                }
                serviceList.Add(alias);
            }

            return this;
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>服务绑定数据</returns>
        public bool BindIf(string service, Func<IContainer, object[], object> concrete, bool isStatic, out IBindData bindData)
        {
            var bind = GetBind(service);
            if (bind == null && (HasInstance(service) || IsAlias(service)))
            {
                bindData = null;
                return false;
            }
            bindData = bind ?? Bind(service, concrete, isStatic);
            return bind == null;
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        /// <param name="bindData">如果绑定失败则返回历史绑定对象</param>
        /// <returns>服务绑定数据</returns>
        public bool BindIf(string service, Type concrete, bool isStatic, out IBindData bindData)
        {
            if (!IsUnableType(concrete))
            {
                service = FormatService(service);
                return BindIf(service, WrapperTypeBuilder(service, concrete), isStatic, out bindData);
            }

            bindData = null;
            return false;
        }

        /// <summary>
        /// 绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否静态化</param>
        /// <returns>服务绑定数据</returns>
        /// <exception cref="ArgumentNullException"><paramref name="concrete"/>为<c>null</c>或者空字符串</exception>
        public IBindData Bind(string service, Type concrete, bool isStatic)
        {
            Guard.NotNull(concrete, nameof(concrete));
            if (IsUnableType(concrete))
            {
                throw new LogicException($"Bind type [{concrete}] can not built");
            }
            service = FormatService(service);
            return Bind(service, WrapperTypeBuilder(service, concrete), isStatic);
        }

        /// <summary>
        /// 绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否静态化</param>
        /// <returns>服务绑定数据</returns>
        /// <exception cref="LogicException"><paramref name="service"/>绑定冲突</exception>
        /// <exception cref="ArgumentNullException"><paramref name="concrete"/>为<c>null</c></exception>
        public IBindData Bind(string service, Func<IContainer, object[], object> concrete, bool isStatic)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            Guard.NotNull(concrete, nameof(concrete));
            GuardServiceName(service);

            service = FormatService(service);
            lock (syncRoot)
            {
                GuardFlushing();

                if (binds.ContainsKey(service))
                {
                    throw new LogicException($"Bind [{service}] already exists.");
                }

                if (instances.ContainsKey(service))
                {
                    throw new LogicException($"Instances [{service}] is already exists.");
                }

                if (aliases.ContainsKey(service))
                {
                    throw new LogicException($"Aliase [{service}] is already exists.");
                }

                var bindData = new BindData(this, service, concrete, isStatic);
                binds.Add(service, bindData);

                if (!IsResolved(service))
                {
                    return bindData;
                }

                if (isStatic)
                {
                    // 如果为 静态的 那么直接解决这个服务
                    // 在服务静态化的过程会触发 TriggerOnRebound
                    Resolve(service);
                }
                else
                {
                    TriggerOnRebound(service);
                }

                return bindData;
            }
        }

        /// <summary>
        /// 绑定一个方法到容器
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="target">调用目标</param>
        /// <param name="call">调用方法</param>
        /// <returns>方法绑定数据</returns>
        public IMethodBind BindMethod(string method, object target, MethodInfo call)
        {
            GuardFlushing();
            GuardMethodName(method);
            return methodContainer.Bind(method, target, call);
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
        public void UnbindMethod(object target)
        {
            methodContainer.Unbind(target);
        }

        /// <summary>
        /// 调用一个已经被绑定的方法
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="userParams">用户提供的参数</param>
        /// <returns>调用结果</returns>
        public object Invoke(string method, params object[] userParams)
        {
            GuardConstruct(nameof(Invoke));
            return methodContainer.Invoke(method, userParams);
        }

        /// <summary>
        /// 以依赖注入形式调用一个方法
        /// </summary>
        /// <param name="target">方法对象</param>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法返回值</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/>,<paramref name="methodInfo"/>为<c>null</c></exception>
        public object Call(object target, MethodInfo methodInfo, params object[] userParams)
        {
            Guard.Requires<ArgumentNullException>(methodInfo != null);
            if (!methodInfo.IsStatic)
            {
                Guard.Requires<ArgumentNullException>(target != null);
            }
            GuardConstruct(nameof(Call));

            var parameter = methodInfo.GetParameters();

            lock (syncRoot)
            {
                var bindData = GetBindFillable(target != null ? Type2Service(target.GetType()) : null);
                userParams = GetDependencies(bindData, parameter, userParams) ?? new object[] { };
                return methodInfo.Invoke(target, userParams);
            }
        }

        /// <summary>
        /// 构造服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <param name="userParams">用户传入的构造参数</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>为<c>null</c>或者空字符串</exception>
        /// <exception cref="LogicException">出现循环依赖</exception>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        public object Make(string service, params object[] userParams)
        {
            return Resolve(service, userParams);
        }

        /// <summary>
        /// 构造服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        public object this[string service]
        {
            get => Make(service);
            set
            {
                lock (syncRoot)
                {
                    var bind = GetBind(service);
                    if (bind != null)
                    {
                        Unbind(bind);
                    }
                    Bind(service, (_, __) => value, false);
                }
            }
        }

        /// <summary>
        /// 扩展容器中的服务
        /// <para>允许在服务构建的过程中配置或者替换服务</para>
        /// <para>如果服务已经被构建，拓展会立即生效。</para>
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <param name="closure">闭包</param>
        public void Extend(string service, Func<object, IContainer, object> closure)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            Guard.Requires<ArgumentNullException>(closure != null);

            lock (syncRoot)
            {
                GuardFlushing();
                service = AliasToService(service);

                if (instances.TryGetValue(service, out object instance))
                {
                    // 如果实例已经存在那么，那么应用扩展。
                    // 扩展将不再被添加到永久扩展列表
                    var old = instance;
                    instances[service] = instance = closure(instance, this);

                    if (!old.Equals(instance))
                    {
                        instancesReverse.Remove(old);
                        instancesReverse.Add(instance, service);
                    }

                    TriggerOnRebound(service, instance);
                    return;
                }

                if (!extenders.TryGetValue(service, out List<Func<object, IContainer, object>> extender))
                {
                    extenders[service] = extender = new List<Func<object, IContainer, object>>();
                }

                extender.Add(closure);

                if (IsResolved(service))
                {
                    TriggerOnRebound(service);
                }
            }
        }

        /// <summary>
        /// 移除指定服务的全部扩展
        /// </summary>
        /// <param name="service">服务名或别名</param>
        public void ClearExtenders(string service)
        {
            lock (syncRoot)
            {
                GuardFlushing();
                service = AliasToService(service);
                extenders.Remove(service);

                if (!IsResolved(service))
                {
                    return;
                }

                Release(service);
                TriggerOnRebound(service);
            }
        }

        /// <summary>
        /// 静态化一个服务,实例值会经过解决修饰器
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <param name="instance">服务实例，<c>null</c>也是合法的实例值</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>为<c>null</c>或者空字符串</exception>
        /// <exception cref="LogicException"><paramref name="service"/>的服务在绑定设置中不是静态的</exception>
        /// <returns>被修饰器处理后的新的实例</returns>
        public object Instance(string service, object instance)
        {
            Guard.NotEmptyOrNull(service, nameof(service));

            lock (syncRoot)
            {
                GuardFlushing();
                GuardServiceName(service);

                service = AliasToService(service);

                var bindData = GetBind(service);
                if (bindData != null)
                {
                    if (!bindData.IsStatic)
                    {
                        throw new LogicException($"Service [{service}] is not Singleton(Static) Bind.");
                    }
                }
                else
                {
                    bindData = MakeEmptyBindData(service);
                }

                instance = TriggerOnResolving((BindData)bindData, instance);

                if (instance != null
                    && instancesReverse.TryGetValue(instance, out string realService)
                    && realService != service)
                {
                    throw new CodeStandardException($"The instance has been registered as a singleton in {realService}");
                }

                var isResolved = IsResolved(service);
                Release(service);

                instances.Add(service, instance);

                if (instance != null)
                {
                    instancesReverse.Add(instance, service);
                }

                if (!instanceTiming.Contains(service))
                {
                    instanceTiming.Add(service, instanceId++);
                }

                if (isResolved)
                {
                    TriggerOnRebound(service, instance);
                }

                return instance;
            }
        }

        /// <summary>
        /// 释放静态化实例
        /// </summary>
        /// <param name="mixed">服务名或别名或单例化的对象</param>
        /// <returns>是否完成了释放</returns>
        public bool Release(object mixed)
        {
            if (mixed == null)
            {
                return false;
            }

            lock (syncRoot)
            {
                string service;
                object instance = null;
                if (!(mixed is string))
                {
                    service = GetServiceWithInstanceObject(mixed);
                }
                else
                {
                    service = AliasToService(mixed.ToString());
                    if (!instances.TryGetValue(service, out instance))
                    {
                        // 防止将字符串作为服务名的情况
                        service = GetServiceWithInstanceObject(mixed);
                    }
                }

                if (instance == null && 
                    (string.IsNullOrEmpty(service) || !instances.TryGetValue(service, out instance)))
                {
                    return false;
                }

                var bindData = GetBindFillable(service);
                bindData.TriggerRelease(instance);
                TriggerOnRelease(bindData, instance);

                if (instance != null)
                {
                    DisposeInstance(instance);
                    instancesReverse.Remove(instance);
                }

                instances.Remove(service);

                if (!HasOnReboundCallbacks(service))
                {
                    instanceTiming.Remove(service);
                }

                return true;
            }
        }

        /// <summary>
        /// 当查找类型无法找到时会尝试去调用开发者提供的查找类型函数
        /// </summary>
        /// <param name="finder">查找类型的回调</param>
        /// <param name="priority">查询优先级(值越小越优先)</param>
        /// <returns>当前容器实例</returns>
        public IContainer OnFindType(Func<string, Type> finder, int priority = int.MaxValue)
        {
            Guard.NotNull(finder, nameof(finder));
            lock (syncRoot)
            {
                GuardFlushing();
                findType.Add(finder, priority);
            }
            return this;
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="closure">处理释放时的回调</param>
        /// <returns>当前容器实例</returns>
        public IContainer OnRelease(Action<IBindData, object> closure)
        {
            AddClosure(closure, release);
            return this;
        }

        /// <summary>
        /// 当服务被解决时，生成的服务会经过注册的回调函数
        /// </summary>
        /// <param name="closure">回调函数</param>
        /// <returns>当前容器对象</returns>
        public IContainer OnResolving(Action<IBindData, object> closure)
        {
            AddClosure(closure, resolving);
            return this;
        }

        /// <summary>
        /// 解决服务时事件之后的回调
        /// </summary>
        /// <param name="closure">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public IContainer OnAfterResolving(Action<IBindData, object> closure)
        {
            AddClosure(closure, afterResloving);
            return this;
        }

        /// <summary>
        /// 关注指定的服务，当服务触发重定义时调用指定对象的指定方法
        /// <para>调用是以依赖注入的形式进行的</para>
        /// <para>服务的新建（第一次解决服务）操作并不会触发重定义</para>
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="callback">回调</param>
        /// <returns>服务容器</returns>
        public IContainer OnRebound(string service, Action<object> callback)
        {
            Guard.NotNull(callback, nameof(callback));
            lock (syncRoot)
            {
                GuardFlushing();
                service = AliasToService(service);

                if (!IsResolved(service) && !CanMake(service))
                {
                    throw new CodeStandardException(
                        $"If you want use Rebound(Watch) , please {nameof(Bind)} or {nameof(Instance)} service first.");
                }

                if (!rebound.TryGetValue(service, out List<Action<object>> list))
                {
                    rebound[service] = list = new List<Action<object>>();
                }

                list.Add(callback);
            }
            return this;
        }

        /// <summary>
        /// 解除绑定服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        public void Unbind(string service)
        {
            service = AliasToService(service);
            var bind = GetBind(service);
            bind?.Unbind();
        }

        /// <summary>
        /// 清空容器的所有实例，绑定，别名，标签，解决器，方法容器, 扩展
        /// </summary>
        public virtual void Flush()
        {
            lock (syncRoot)
            {
                try
                {
                    flushing = true;
                    foreach (var service in instanceTiming)
                    {
                        Release(service);
                    }

                    Guard.Requires<AssertException>(instances.Count <= 0);

                    tags.Clear();
                    aliases.Clear();
                    aliasesReverse.Clear();
                    instances.Clear();
                    binds.Clear();
                    resolving.Clear();
                    release.Clear();
                    extenders.Clear();
                    resolved.Clear();
                    findType.Clear();
                    findTypeCache.Clear();
                    BuildStack.Clear();
                    UserParamsStack.Clear();
                    rebound.Clear();
                    methodContainer.Flush();
                    instanceTiming.Clear();
                    instanceId = 0;
                }
                finally
                {
                    flushing = false;
                }
            }
        }

        /// <summary>
        /// 将类型转为服务名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>服务名</returns>
        public string Type2Service(Type type)
        {
            return type.ToString();
        }

        /// <summary>
        /// 解除绑定服务
        /// </summary>
        /// <param name="bindable">绑定关系</param>
        internal void Unbind(IBindable bindable)
        {
            lock (syncRoot)
            {
                GuardFlushing();
                Release(bindable.Service);
                if (aliasesReverse.TryGetValue(bindable.Service, out List<string> serviceList))
                {
                    foreach (var alias in serviceList)
                    {
                        aliases.Remove(alias);
                    }
                    aliasesReverse.Remove(bindable.Service);
                }
                binds.Remove(bindable.Service);
            }
        }

        /// <summary>
        /// 在回调区间内暂时性的静态化服务实例
        /// </summary>
        /// <param name="callback">回调区间</param>
        /// <param name="services">服务映射</param>
        public void Flash(Action callback, params KeyValuePair<string, object>[] services)
        {
            lock (syncRoot)
            {
                if (services == null || services.Length <= 0)
                {
                    callback();
                    return;
                }

                Stack<KeyValuePair<string, object>> serviceStack = null;
                try
                {
                    foreach (var service in services)
                    {
                        try
                        {
                            // 如果服务被绑定过了，那么我们认为这不是一个Flash可用的服务
                            // 所以我们抛出一个异常来终止该操作。
                            if (HasBind(service.Key))
                            {
                                throw new LogicException(
                                    $"Flash service [{service.Key}] name has be used for {nameof(Bind)} or {nameof(Alias)}.");
                            }
                        }
                        catch
                        {
                            // 如果在 HasBind 执行过程中出现了异常，那么清空服务堆栈
                            // 因为服务还没替换也无需在执行还原操作。
                            serviceStack = null;
                            throw;
                        }

                        if (!HasInstance(service.Key))
                        {
                            continue;
                        }

                        // 如果服务已经存在那么，将旧的服务加入堆栈。
                        // 等待Flash操作完成后再恢复旧的服务实例。
                        serviceStack = serviceStack ?? new Stack<KeyValuePair<string, object>>(services.Length);
                        serviceStack.Push(new KeyValuePair<string, object>(service.Key, Make(service.Key)));
                    }

                    Arr.Flash(services,
                        service => Instance(service.Key, service.Value),
                        service => Release(service.Key),
                        callback);
                }
                finally
                {
                    while (serviceStack != null && serviceStack.Count > 0)
                    {
                        var service = serviceStack.Pop();
                        Instance(service.Key, service.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 是否是依赖注入容器默认的基础类型
        /// </summary>
        /// <param name="type">基础类型</param>
        /// <returns>是否是基础类型</returns>
        protected virtual bool IsBasicType(Type type)
        {
            return type == null || type.IsPrimitive || type == typeof(string);
        }

        /// <summary>
        /// 是否是无法被构建的类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否可以被构建</returns>
        protected virtual bool IsUnableType(Type type)
        {
            return type == null || type.IsAbstract || type.IsInterface || type.IsArray || type.IsEnum;
        }

        /// <summary>
        /// 包装一个类型，可以被用来生成服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">类型</param>
        /// <returns>根据类型生成的服务</returns>
        protected virtual Func<IContainer, object[], object> WrapperTypeBuilder(string service, Type concrete)
        {
            return (container, userParams) => ((Container) container).CreateInstance(GetBindFillable(service), concrete,
                userParams);
        }

        /// <summary>
        /// 从用户传入的参数中获取依赖
        /// </summary>
        /// <param name="baseParam">基础参数</param>
        /// <param name="userParams">用户传入参数</param>
        /// <returns>合适的注入参数</returns>
        protected virtual object GetDependenciesFromUserParams(ParameterInfo baseParam, ref object[] userParams)
        {
            if (userParams == null)
            {
                return null;
            }

            GuardUserParamsCount(userParams.Length);

            for (var n = 0; n < userParams.Length; n++)
            {
                var userParam = userParams[n];

                if (!ChangeType(ref userParam, baseParam.ParameterType))
                {
                    continue;
                }

                Arr.RemoveAt(ref userParams, n);
                return userParam;
            }

            return null;
        }

        /// <summary>
        /// 转换参数类型
        /// </summary>
        /// <param name="result">需要转换的参数</param>
        /// <param name="conversionType">转换到的类型</param>
        /// <returns>是否转换成功</returns>
        protected virtual bool ChangeType(ref object result, Type conversionType)
        {
            try
            {
                if (result == null || conversionType.IsInstanceOfType(result))
                {
                    return true;
                }

                if (IsBasicType(result.GetType()) && typeof(IVariant).IsAssignableFrom(conversionType))
                {
                    try
                    {
                        result = Make(Type2Service(conversionType), result);
                        return true;
                    }
                    catch (Exception)
                    {
                        // ignored
                        // when throw exception then stop inject
                    }
                }

                if (result is IConvertible && typeof(IConvertible).IsAssignableFrom(conversionType))
                {
                    result = Convert.ChangeType(result, conversionType);
                    return true;
                }
            }
            catch (Exception)
            {
                // ignored
                // when throw exception then stop inject
            }

            return false;
        }

        /// <summary>
        /// 获取字段需求服务
        /// </summary>
        /// <param name="property">字段</param>
        /// <returns>需求的服务名</returns>
        protected virtual string GetPropertyNeedsService(PropertyInfo property)
        {
            return Type2Service(property.PropertyType);
        }

        /// <summary>
        /// 获取参数需求服务
        /// </summary>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>需求的服务名</returns>
        protected virtual string GetParamNeedsService(ParameterInfo baseParam)
        {
            return Type2Service(baseParam.ParameterType);
        }

        /// <summary>
        /// 根据上下文获取相关的构建闭包
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">构建的服务名</param>
        /// <param name="paramName">目标参数的名字</param>
        /// <returns>构建闭包</returns>
        protected virtual Func<object> GetContextualClosure(Bindable makeServiceBindData, string service,
            string paramName)
        {
            return makeServiceBindData.GetContextualClosure(service) ??
                   makeServiceBindData.GetContextualClosure($"{GetVariableTag()}{paramName}");
        }

        /// <summary>
        /// 根据上下文获取相关的需求服务
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">构建的服务名</param>
        /// <param name="paramName">目标参数的名字</param>
        /// <returns>需求的服务名</returns>
        protected virtual string GetContextualService(Bindable makeServiceBindData, string service, string paramName)
        {
            return makeServiceBindData.GetContextual(service) ??
                   makeServiceBindData.GetContextual($"{GetVariableTag()}{paramName}") ??
                   service;
        }

        /// <summary>
        /// 从上下文闭包中进行构建获得实例
        /// </summary>
        /// <param name="closure">上下文闭包</param>
        /// <param name="needType">参数需求的类型</param>
        /// <param name="ouput">构建的实例</param>
        /// <returns>是否成功构建</returns>
        protected virtual bool MakeFromContextualClosure(Func<object> closure, Type needType, out object ouput)
        {
            ouput = null;
            if (closure == null)
            {
                return false;
            }

            ouput = closure();
            return ChangeType(ref ouput, needType);
        }

        /// <summary>
        /// 从上下文关系的服务名获取服务实现
        /// </summary>
        /// <param name="service">上下文关系的服务名</param>
        /// <param name="needType">参数需求类型</param>
        /// <param name="output">构建的实例</param>
        /// <returns>是否成功构建</returns>
        protected virtual bool MakeFromContextualService(string service, Type needType, out object output)
        {
            output = null;
            if (!CanMake(service))
            {
                return false;
            }

            output = Make(service);
            return ChangeType(ref output, needType);
        }

        /// <summary>
        /// 根据上下文来解决指定需求的服务
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">构建的服务名字</param>
        /// <param name="paramName">目标参数的名字</param>
        /// <param name="paramType">目标参数的类型</param>
        /// <param name="output">构建的实例</param>
        /// <returns>是否成功通过上下文解决</returns>
        protected virtual bool ResloveFromContextual(Bindable makeServiceBindData, string service, string paramName,
            Type paramType, out object output)
        {
            if (MakeFromContextualClosure(GetContextualClosure(makeServiceBindData, service, paramName),
                paramType, out output))
            {
                return true;
            }

            return MakeFromContextualService(GetContextualService(makeServiceBindData, service, paramName),
                paramType, out output);
        }

        /// <summary>
        /// 解决基本类型
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">希望解决的服务名或者别名</param>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>解决结果</returns>
        protected virtual object ResolveAttrPrimitive(Bindable makeServiceBindData, string service, PropertyInfo baseParam)
        {
            return ResloveAttrClass(makeServiceBindData, service, baseParam);
        }

        /// <summary>
        /// 解决类类型
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">希望解决的服务名或者别名</param>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>解决结果</returns>
        protected virtual object ResloveAttrClass(Bindable makeServiceBindData, string service, PropertyInfo baseParam)
        {
            if (ResloveFromContextual(makeServiceBindData, service, baseParam.Name, baseParam.PropertyType,
                out object instance))
            {
                return instance;
            }

            throw MakeUnresolvablePrimitiveException(baseParam.Name, baseParam.DeclaringType);
        }

        /// <summary>
        /// 解决基本类型
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">希望解决的服务名或者别名</param>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>解决结果</returns>
        protected virtual object ResolvePrimitive(Bindable makeServiceBindData, string service, ParameterInfo baseParam)
        {
            return ResloveClass(makeServiceBindData, service, baseParam);
        }

        /// <summary>
        /// 解决类类型
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">希望解决的服务名或者别名</param>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>解决结果</returns>
        protected virtual object ResloveClass(Bindable makeServiceBindData, string service, ParameterInfo baseParam)
        {
            if (ResloveFromContextual(makeServiceBindData, service, baseParam.Name, baseParam.ParameterType,
                out object instance))
            {
                return instance;
            }

            if (baseParam.IsOptional)
            {
                return baseParam.DefaultValue;
            }

            throw MakeUnresolvablePrimitiveException(baseParam.Name, baseParam.Member.DeclaringType);
        }

        /// <summary>
        /// 获取变量标签
        /// </summary>
        /// <returns>变量标签</returns>
        protected virtual char GetVariableTag()
        {
            return '$';
        }

        /// <summary>
        /// 获取编译堆栈调试消息
        /// </summary>
        /// <returns></returns>
        protected virtual string GetBuildStackDebugMessage()
        {
            var previous = string.Join(", ", BuildStack.ToArray());
            return $" While building stack [{previous}].";
        }

        /// <summary>
        /// 生成一个编译失败异常
        /// </summary>
        /// <param name="makeService">构造的服务名字</param>
        /// <param name="makeServiceType">构造的服务类型</param>
        /// <param name="innerException">内部异常</param>
        /// <returns>运行时异常</returns>
        protected virtual UnresolvableException MakeBuildFaildException(string makeService, Type makeServiceType, Exception innerException)
        {
            var message = makeServiceType != null
                ? $"Class [{makeServiceType}] build faild. Service is [{makeService}]."
                : $"Service [{makeService}] is not exists.";

            message += GetBuildStackDebugMessage();
            message += GetInnerExceptionMessage(innerException);
            return new UnresolvableException(message, innerException);
        }

        /// <summary>
        /// 获取内部异常提示消息
        /// </summary>
        /// <param name="innerException">内部异常</param>
        /// <returns>提示消息内容</returns>
        protected virtual string GetInnerExceptionMessage(Exception innerException)
        {
            if (innerException == null)
            {
                return string.Empty;
            }

            var stack = new StringBuilder();
            do
            {
                if (stack.Length > 0)
                {
                    stack.Append(", ");
                }
                stack.Append(innerException);
            } while ((innerException = innerException.InnerException) != null);
            return $" InnerException message stack: [{stack}]";
        }

        /// <summary>
        /// 生成一个未能解决基本类型的异常
        /// </summary>
        /// <param name="name">变量名</param>
        /// <param name="declaringClass">变量所属类</param>
        /// <returns>运行时异常</returns>
        protected virtual UnresolvableException MakeUnresolvablePrimitiveException(string name, Type declaringClass)
        {
            return new UnresolvableException(
                $"Unresolvable primitive dependency , resolving [{name}] in class [{declaringClass}]");
        }

        /// <summary>
        /// 生成一个出现循环依赖的异常
        /// </summary>
        /// <param name="service">当前服务名</param>
        /// <returns>运行时异常</returns>
        protected virtual LogicException MakeCircularDependencyException(string service)
        {
            var message = $"Circular dependency detected while for [{service}].";
            message += GetBuildStackDebugMessage();
            return new LogicException(message);
        }

        /// <summary>
        /// 格式化服务名
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>格式化后的服务名</returns>
        protected virtual string FormatService(string service)
        {
            return service.Trim();
        }

        /// <summary>
        /// 检查实例是否实现自某种类型
        /// </summary>
        /// <param name="type">需要实现自的类型</param>
        /// <param name="instance">生成的实例</param>
        /// <returns>是否符合类型</returns>
        protected virtual bool CanInject(Type type, object instance)
        {
            return instance == null || type.IsInstanceOfType(instance);
        }

        /// <summary>
        /// 保证用户传入参数必须小于指定值
        /// </summary>
        /// <param name="count">传入参数数量</param>
        protected virtual void GuardUserParamsCount(int count)
        {
            if (count > 255)
            {
                throw new LogicException("Too many parameters , must be less or equal than 255");
            }
        }

        /// <summary>
        /// 守卫解决实例状态
        /// </summary>
        /// <param name="instance">服务实例</param>
        /// <param name="makeService">服务名</param>
        protected virtual void GuardResolveInstance(object instance, string makeService)
        {
            if (instance == null)
            {
                throw MakeBuildFaildException(makeService, SpeculatedServiceType(makeService), null);
            }
        }

        /// <summary>
        /// 根据服务名推测服务的类型
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>服务类型</returns>
        protected virtual Type SpeculatedServiceType(string service)
        {
            if (findTypeCache.TryGetValue(service, out Type result))
            {
                return result;
            }

            foreach (var finder in findType)
            {
                var type = finder.Invoke(service);
                if (type != null)
                {
                    return findTypeCache[service] = type;
                }
            }

            return findTypeCache[service] = null;
        }

        /// <summary>
        /// 属性注入
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="makeServiceInstance">服务实例</param>
        /// <returns>服务实例</returns>
        /// <exception cref="LogicException">属性是必须的或者注入类型和需求类型不一致</exception>
        protected virtual void AttributeInject(Bindable makeServiceBindData, object makeServiceInstance)
        {
            if (makeServiceInstance == null)
            {
                return;
            }

            foreach (var property in makeServiceInstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite
                    || !property.IsDefined(injectTarget, false))
                {
                    continue;
                }

                var needService = GetPropertyNeedsService(property);

                object instance;
                if (property.PropertyType.IsClass
                    || property.PropertyType.IsInterface)
                {
                    instance = ResloveAttrClass(makeServiceBindData, needService, property);
                }
                else
                {
                    instance = ResolveAttrPrimitive(makeServiceBindData, needService, property);
                }

                if (!CanInject(property.PropertyType, instance))
                {
                    throw new UnresolvableException(
                        $"[{makeServiceBindData.Service}]({makeServiceInstance.GetType()}) Attr inject type must be [{property.PropertyType}] , But instance is [{instance?.GetType()}] , Make service is [{needService}].");
                }

                property.SetValue(makeServiceInstance, instance, null);
            }
        }

        /// <summary>
        /// 检查是否可以紧缩注入用户传入的参数
        /// </summary>
        /// <param name="baseParam">服务实例的参数信息</param>
        /// <param name="userParams">输入的构造参数列表</param>
        /// <returns>是否可以紧缩注入</returns>
        protected virtual bool CheckCompactInjectUserParams(ParameterInfo baseParam, object[] userParams)
        {
            if (userParams == null || userParams.Length <= 0)
            {
                return false;
            }

            return baseParam.ParameterType == typeof(object[])
                   || baseParam.ParameterType == typeof(object);
        }

        /// <summary>
        /// 获取通过紧缩注入的参数
        /// </summary>
        /// <param name="baseParam">服务实例的参数信息</param>
        /// <param name="userParams">输入的构造参数列表</param>
        /// <returns>紧缩注入的参数</returns>
        protected virtual object GetCompactInjectUserParams(ParameterInfo baseParam, ref object[] userParams)
        {
            if (!CheckCompactInjectUserParams(baseParam, userParams))
            {
                return null;
            }

            var result = userParams;
            userParams = null;

            if (baseParam.ParameterType == typeof(object)
                && result != null && result.Length == 1)
            {
                return result[0];
            }

            return result;
        }

        /// <summary>
        /// 获取参数(<see cref="IParams"/>)匹配器
        /// <para>开发者重写后可以实现自己的匹配器</para>
        /// <para>如果调用获取到的匹配器后返回结果为null则表示没有匹配到参数</para>
        /// </summary>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>匹配器，如果返回null则表示没有匹配器</returns>
        protected virtual Func<ParameterInfo, object> GetParamsMatcher(ref object[] userParams)
        {
            if (userParams == null || userParams.Length <= 0)
            {
                return null;
            }

            var tables = GetParamsTypeInUserParams(ref userParams);
            return tables.Length <= 0 ? null : MakeParamsMatcher(tables);
        }

        /// <summary>
        /// 获取依赖解决结果
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="baseParams">服务实例的参数信息</param>
        /// <param name="userParams">输入的构造参数列表</param>
        /// <returns>服务所需参数的解决结果</returns>
        /// <exception cref="LogicException">生成的实例类型和需求类型不一致</exception>
        protected virtual object[] GetDependencies(Bindable makeServiceBindData, ParameterInfo[] baseParams, object[] userParams)
        {
            if (baseParams.Length <= 0)
            {
                return null;
            }

            var results = new object[baseParams.Length];

            // 获取一个参数匹配器用于筛选参数
            var matcher = GetParamsMatcher(ref userParams);

            for (var i = 0; i < baseParams.Length; i++)
            {
                var baseParam = baseParams[i];

                // 使用参数匹配器对参数进行匹配，参数匹配器是最先进行的，因为他们的匹配精度是最准确的
                var param = matcher?.Invoke(baseParam);

                // 当容器发现开发者使用 object 或者 object[] 作为参数类型时
                // 我们尝试将所有用户传入的用户参数紧缩注入
                param = param ?? GetCompactInjectUserParams(baseParam, ref userParams);

                // 从用户传入的参数中挑选合适的参数，按照相对顺序依次注入
                param = param ?? GetDependenciesFromUserParams(baseParam, ref userParams);

                string needService = null;

                if (param == null)
                {
                    // 尝试通过依赖注入容器来生成所需求的参数
                    needService = GetParamNeedsService(baseParam);

                    if (baseParam.ParameterType.IsClass
                        || baseParam.ParameterType.IsInterface)
                    {
                        param = ResloveClass(makeServiceBindData, needService, baseParam);
                    }
                    else
                    {
                        param = ResolvePrimitive(makeServiceBindData, needService, baseParam);
                    }
                }

                // 对筛选到的参数进行注入检查
                if (!CanInject(baseParam.ParameterType, param))
                {
                    var error =
                        $"[{makeServiceBindData.Service}] Params inject type must be [{baseParam.ParameterType}] , But instance is [{param?.GetType()}]";
                    if (needService == null)
                    {
                        error += " Inject params from user incoming parameters.";
                    }
                    else
                    {
                        error += $" Make service is [{needService}].";
                    }

                    throw new UnresolvableException(error);
                }

                results[i] = param;
            }

            return results;
        }

        /// <summary>
        /// 获取构造函数参数
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="makeServiceType">服务类型</param>
        /// <param name="userParams">用户传入的构造参数</param>
        /// <returns>构造函数参数</returns>
        protected virtual object[] GetConstructorsInjectParams(Bindable makeServiceBindData, Type makeServiceType, object[] userParams)
        {
            var constructors = makeServiceType.GetConstructors();
            if (constructors.Length <= 0)
            {
                return null;
            }

            Exception exception = null;
            foreach (var constructor in constructors)
            {
                try
                {
                    return GetDependencies(makeServiceBindData, constructor.GetParameters(), userParams);
                }
                catch (Exception ex)
                {
                    if (exception == null)
                    {
                        exception = ex;
                    }
                }
            }

            Guard.Requires<AssertException>(exception != null);
            throw exception;
        }

        /// <summary>
        /// 通过对象反向获取服务名
        /// </summary>
        /// <param name="instance">对象</param>
        /// <returns>服务名</returns>
        protected string GetServiceWithInstanceObject(object instance)
        {
            return instancesReverse.TryGetValue(instance, out string origin)
                ? origin
                : string.Empty;
        }

        /// <summary>
        /// 验证构建状态
        /// </summary>
        /// <param name="method">函数名</param>
        protected virtual void GuardConstruct(string method)
        {
        }

        /// <summary>
        /// 验证服务名有效性
        /// </summary>
        /// <param name="service">服务名</param>
        protected virtual void GuardServiceName(string service)
        {
            foreach (var c in ServiceBanChars)
            {
                if (service.IndexOf(c) >= 0)
                {
                    throw new CodeStandardException(
                        $"Service name {service} contains disabled characters : {c}. please use Alias replacement");
                }
            }
        }

        /// <summary>
        /// 验证函数名有效性
        /// </summary>
        /// <param name="method">函数名</param>
        protected virtual void GuardMethodName(string method)
        {

        }

        /// <summary>
        /// 验证重置状态
        /// </summary>
        private void GuardFlushing()
        {
            if (flushing)
            {
                throw new CodeStandardException("Container is flushing can not do it");
            }
        }

        /// <summary>
        /// 获取别名最终对应的服务名
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>最终映射的服务名</returns>
        private string AliasToService(string service)
        {
            service = FormatService(service);
            return aliases.TryGetValue(service, out string alias) ? alias : service;
        }

        /// <summary>
        /// 触发全局解决修饰器
        /// </summary>
        /// <param name="bindData">服务绑定数据</param>
        /// <param name="instance">服务实例</param>
        /// <returns>被修饰器修饰后的服务实例</returns>
        private object TriggerOnResolving(BindData bindData, object instance)
        {
            instance = bindData.TriggerResolving(instance);
            instance = Trigger(bindData, instance, resolving);
            return TriggerOnAfterResolving(bindData, instance);
        }

        /// <summary>
        /// 触发全局解决修饰器之后的修饰器回调
        /// </summary>
        /// <param name="bindData">服务绑定数据</param>
        /// <param name="instance">服务实例</param>
        /// <returns>被修饰器修饰后的服务实例</returns>
        private object TriggerOnAfterResolving(BindData bindData, object instance)
        {
            instance = bindData.TriggerAfterResolving(instance);
            return Trigger(bindData, instance, afterResloving);
        }

        /// <summary>
        /// 触发全局释放修饰器
        /// </summary>
        /// <param name="bindData">服务绑定数据</param>
        /// <param name="instance">服务实例</param>
        /// <returns>被修饰器修饰后的服务实例</returns>
        private object TriggerOnRelease(IBindData bindData, object instance)
        {
            return Trigger(bindData, instance, release);
        }

        /// <summary>
        /// 触发指定的事件列表
        /// </summary>
        /// <param name="bindData">服务绑定数据</param>
        /// <param name="instance">服务实例</param>
        /// <param name="list">事件列表</param>
        /// <returns>服务实例</returns>
        internal object Trigger(IBindData bindData, object instance, List<Action<IBindData, object>> list)
        {
            if (list == null)
            {
                return instance;
            }

            foreach (var closure in list)
            {
                closure(bindData, instance);
            }

            return instance;
        }

        /// <summary>
        /// 触发服务重定义事件
        /// </summary>
        /// <param name="service">发生重定义的服务</param>
        /// <param name="instance">服务实例（如果为空将会从容器请求）</param>
        private void TriggerOnRebound(string service, object instance = null)
        {
            var callbacks = GetOnReboundCallbacks(service);
            if (callbacks == null || callbacks.Count <= 0)
            {
                return;
            }

            var bind = GetBind(service);
            instance = instance ?? Make(service);
            Flash(() =>
            {
                for (var index = 0; index < callbacks.Count; index++)
                {
                    callbacks[index](instance);
                    // 如果是非实例绑定那么每个 callback 给定单独的实例
                    if (index + 1 < callbacks.Count && (bind == null || !bind.IsStatic))
                    {
                        instance = Make(service);
                    }
                }
            }, Pair(typeof(IBindData), bind));
        }

        /// <summary>
        /// 释放实例
        /// </summary>
        /// <param name="obj">实例</param>
        private void DisposeInstance(object obj)
        {
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// 类型配实例配偶
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="instance">实例</param>
        /// <returns>键值对</returns>
        private KeyValuePair<string, object> Pair(Type type, object instance)
        {
            return new KeyValuePair<string, object>(Type2Service(type), instance);
        }

        /// <summary>
        /// 获取重定义的服务所对应的回调
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>回调列表</returns>
        private IList<Action<object>> GetOnReboundCallbacks(string service)
        {
            return !rebound.TryGetValue(service, out List<Action<object>> result) ? null : result;
        }

        /// <summary>
        /// 是否拥有重定义的服务所对应的回调
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>是否存在回调</returns>
        private bool HasOnReboundCallbacks(string service)
        {
            var result = GetOnReboundCallbacks(service);
            return result != null && result.Count > 0;
        }

        /// <summary>
        /// 制作一个空的绑定数据
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>空绑定数据</returns>
        private BindData MakeEmptyBindData(string service)
        {
            return new BindData(this, service, null, false);
        }

        /// <summary>
        /// 解决服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <param name="userParams">用户传入的构造参数</param>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>为<c>null</c>或者空字符串</exception>
        /// <exception cref="LogicException">出现循环依赖</exception>
        /// <exception cref="UnresolvableException">无法解决服务</exception>
        /// <returns>服务实例</returns>
        private object Resolve(string service, params object[] userParams)
        {
            GuardConstruct(nameof(Make));
            Guard.NotEmptyOrNull(service, nameof(service));
            lock (syncRoot)
            {
                service = AliasToService(service);

                if (instances.TryGetValue(service, out object instance))
                {
                    return instance;
                }

                if (BuildStack.Contains(service))
                {
                    throw MakeCircularDependencyException(service);
                }

                BuildStack.Push(service);
                UserParamsStack.Push(userParams);
                try
                {
                    var bindData = GetBindFillable(service);

                    // 我们将要开始构建服务实例，
                    // 对于构建的服务我们会尝试进行依赖注入。
                    instance = Build(bindData, userParams);

                    // 如果我们为指定的服务定义了扩展器，那么我们需要依次执行扩展器，
                    // 并允许扩展器来修改或者覆盖原始的服务。
                    instance = Extend(service, instance);

                    instance = bindData.IsStatic
                        ? Instance(bindData.Service, instance)
                        : TriggerOnResolving(bindData, instance);

                    resolved.Add(bindData.Service);
                    return instance;
                }
                finally
                {
                    UserParamsStack.Pop();
                    BuildStack.Pop();
                }
            }
        }

        /// <summary>
        /// 为服务进行扩展
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="instance">服务实例</param>
        /// <returns>扩展后的服务</returns>
        private object Extend(string service, object instance)
        {
            if (!extenders.TryGetValue(service, out List<Func<object, IContainer, object>> list))
            {
                return instance;
            }

            foreach (var extender in list)
            {
                instance = extender(instance, this);
            }

            return instance;
        }

        /// <summary>
        /// 为对象进行依赖注入
        /// </summary>
        /// <param name="bindData">绑定数据</param>
        /// <param name="instance">对象实例</param>
        /// <returns>注入完成的对象</returns>
        private object Inject(Bindable bindData, object instance)
        {
            GuardResolveInstance(instance, bindData.Service);

            AttributeInject(bindData, instance);

            return instance;
        }

        /// <summary>
        /// 编译服务
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="userParams">用户传入的构造参数</param>
        /// <returns>服务实例</returns>
        private object Build(BindData makeServiceBindData, object[] userParams)
        {
            var instance = makeServiceBindData.Concrete != null
                ? makeServiceBindData.Concrete(this, userParams)
                : CreateInstance(makeServiceBindData, SpeculatedServiceType(makeServiceBindData.Service),
                    userParams);

            return Inject(makeServiceBindData, instance);
        }

        /// <summary>
        /// 构造服务实现（准备需要注入的参数）
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="makeServiceType">服务类型</param>
        /// <param name="userParams">用户传入的构造参数</param>
        /// <returns>服务实例</returns>
        protected virtual object CreateInstance(Bindable makeServiceBindData, Type makeServiceType, object[] userParams)
        {
            if (IsUnableType(makeServiceType))
            {
                return null;
            }

            userParams = GetConstructorsInjectParams(makeServiceBindData, makeServiceType, userParams);

            try
            {
                return CreateInstance(makeServiceType, userParams);
            }
            catch (Exception ex)
            {
                throw MakeBuildFaildException(makeServiceBindData.Service, makeServiceType, ex);
            }
        }

        /// <summary>
        /// 通过指定的类型构建服务实现
        /// </summary>
        /// <param name="makeServiceType">指定的服务类型</param>
        /// <param name="userParams">用户自定义参数</param>
        /// <returns>构建的服务实现</returns>
        protected virtual object CreateInstance(Type makeServiceType, object[] userParams)
        {
            // 如果参数不存在那么在反射时不写入参数可以获得更好的性能
            if (userParams == null || userParams.Length <= 0)
            {
                return Activator.CreateInstance(makeServiceType);
            }
            return Activator.CreateInstance(makeServiceType, userParams);
        }

        /// <summary>
        /// 获取服务绑定数据,如果数据为null则填充数据
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>服务绑定数据</returns>
        private BindData GetBindFillable(string service)
        {
            return service != null && binds.TryGetValue(service, out BindData bindData)
                ? bindData
                : MakeEmptyBindData(service);
        }

        /// <summary>
        /// 从<paramref name="userParams"/>中获取<see cref="IParams"/>类型的变量
        /// </summary>
        /// <param name="userParams">用户传入参数</param>
        /// <returns>获取到的参数</returns>
        private IParams[] GetParamsTypeInUserParams(ref object[] userParams)
        {
            // 这里使用了Filter而没有使用Remove由于筛选器也是可能希望注入的类型之一
            var elements = Arr.Filter(userParams, value => value is IParams);
            var results = new IParams[elements.Length];
            for (var i = 0; i < elements.Length; i++)
            {
                results[i] = (IParams)elements[i];
            }
            return results;
        }

        /// <summary>
        /// 生成一个默认的参数<see cref="IParams" />匹配器
        /// </summary>
        /// <param name="tables">参数表</param>
        /// <returns>匹配器</returns>
        private Func<ParameterInfo, object> MakeParamsMatcher(IParams[] tables)
        {
            // 默认匹配器策略将会将参数名和参数表的参数名进行匹配
            // 最先匹配到的有效参数值将作为返回值返回
            return parameterInfo =>
            {
                foreach (var table in tables)
                {
                    if (!table.TryGetValue(parameterInfo.Name, out object result))
                    {
                        continue;
                    }

                    if (ChangeType(ref result, parameterInfo.ParameterType))
                    {
                        return result;
                    }
                }

                return null;
            };
        }

        /// <summary>
        /// 增加一个闭包到指定的列表
        /// </summary>
        /// <param name="closure">闭包</param>
        /// <param name="list">指定的列表</param>
        private void AddClosure(Action<IBindData, object> closure, List<Action<IBindData, object>> list)
        {
            Guard.NotNull(closure, nameof(closure));

            lock (syncRoot)
            {
                GuardFlushing();
                list.Add(closure);
            }
        }
    }
}