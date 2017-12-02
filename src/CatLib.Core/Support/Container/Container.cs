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
        private readonly List<Func<IBindData, object, object>> resolving;

        /// <summary>
        /// 静态服务释放时的修饰器
        /// </summary>
        private readonly List<Action<IBindData, object>> release;

        /// <summary>
        /// 类型查询回调
        /// 当类型无法被解决时会尝试去开发者提供的查询器中查询类型
        /// </summary>
        private readonly SortSet<Func<string, Type>, int> findType;

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
        private readonly Stack<string> buildStack;

        /// <summary>
        /// 用户参数堆栈
        /// </summary>
        private readonly Stack<object[]> userParamsStack;

        /// <summary>
        /// 构造一个容器
        /// </summary>
        public Container()
        {
            tags = new Dictionary<string, List<string>>();
            aliases = new Dictionary<string, string>();
            aliasesReverse = new Dictionary<string, List<string>>();
            instances = new Dictionary<string, object>();
            binds = new Dictionary<string, BindData>();
            resolving = new List<Func<IBindData, object, object>>();
            release = new List<Action<IBindData, object>>();
            findType = new SortSet<Func<string, Type>, int>();
            injectTarget = typeof(InjectAttribute);
            buildStack = new Stack<string>();
            userParamsStack = new Stack<object[]>();
            methodContainer = new MethodContainer(this);
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
            Guard.NotEmptyOrNull(tag, "tag");
            Guard.NotNull(service, "service");
            Guard.CountGreaterZero(service, "service");
            Guard.ElementNotEmptyOrNull(service, "service");

            lock (syncRoot)
            {
                List<string> list;
                if (tags.TryGetValue(tag, out list))
                {
                    list.AddRange(service);
                }
                else
                {
                    list = new List<string>(service);
                    tags.Add(tag, list);
                }
            }
        }

        /// <summary>
        /// 根据标记名生成标记所对应的所有服务实例
        /// </summary>
        /// <param name="tag">标记名</param>
        /// <returns>将会返回标记所对应的所有服务实例</returns>
        /// <exception cref="RuntimeException"><paramref name="tag"/>不存在</exception>
        /// <exception cref="ArgumentNullException"><paramref name="tag"/>为<c>null</c>或者空字符串</exception>
        public object[] Tagged(string tag)
        {
            Guard.NotEmptyOrNull(tag, "tag");
            lock (syncRoot)
            {
                List<string> serviceList;
                if (!tags.TryGetValue(tag, out serviceList))
                {
                    throw new RuntimeException("Tag [" + tag + "] is not exist.");
                }

                var result = new List<object>();

                foreach (var tagService in serviceList)
                {
                    result.Add(Make(tagService));
                }

                return result.ToArray();
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
            Guard.NotEmptyOrNull(service, "service");
            lock (syncRoot)
            {
                service = NormalizeService(service);
                service = AliasToService(service);
                BindData bindData;
                return binds.TryGetValue(service, out bindData) ? bindData : null;
            }
        }

        /// <summary>
        /// 是否已经绑定了服务（只有进行过bind才视作绑定）
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>服务是否被绑定</returns>
        public bool HasBind(string service)
        {
            return GetBind(service) != null;
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
            return aliases.ContainsKey(name);
        }

        /// <summary>
        /// 为服务设定一个别名
        /// </summary>
        /// <param name="alias">别名</param>
        /// <param name="service">映射到的服务名</param>
        /// <returns>当前容器对象</returns>
        /// <exception cref="RuntimeException"><paramref name="alias"/>别名冲突或者<paramref name="service"/>的绑定与实例都不存在</exception>
        /// <exception cref="ArgumentNullException"><paramref name="alias"/>,<paramref name="service"/>为<c>null</c>或者空字符串</exception>
        public IContainer Alias(string alias, string service)
        {
            Guard.NotEmptyOrNull(alias, "alias");
            Guard.NotEmptyOrNull(service, "service");

            if (alias == service)
            {
                throw new RuntimeException("Alias is Same as Service Name: [" + alias + "].");
            }

            alias = NormalizeService(alias);
            service = NormalizeService(service);

            lock (syncRoot)
            {
                if (aliases.ContainsKey(alias))
                {
                    throw new RuntimeException("Alias [" + alias + "] is already exists.");
                }
                if (!binds.ContainsKey(service) && !instances.ContainsKey(service))
                {
                    throw new RuntimeException("You must Bind() or Instance() serivce before you can call Alias().");
                }

                aliases.Add(alias, service);

                List<string> serviceList;
                if (aliasesReverse.TryGetValue(service, out serviceList))
                {
                    serviceList.Add(alias);
                }
                else
                {
                    aliasesReverse.Add(service, new List<string> { alias });
                }
            }

            return this;
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        /// <returns>服务绑定数据</returns>
        public IBindData BindIf(string service, Func<IContainer, object[], object> concrete, bool isStatic)
        {
            var bind = GetBind(service);
            return bind ?? Bind(service, concrete, isStatic);
        }

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        /// <returns>服务绑定数据</returns>
        public IBindData BindIf(string service, Type concrete, bool isStatic)
        {
            var bind = GetBind(service);
            return bind ?? Bind(service, concrete, isStatic);
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
            Guard.NotNull(concrete, "concrete");
            return Bind(service, (c, param) =>
            {
                var container = (Container)c;
                return container.Resolve(service, concrete, false, param);
            }, isStatic);
        }

        /// <summary>
        /// 绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否静态化</param>
        /// <returns>服务绑定数据</returns>
        /// <exception cref="RuntimeException"><paramref name="service"/>绑定冲突</exception>
        /// <exception cref="ArgumentNullException"><paramref name="concrete"/>为<c>null</c></exception>
        public IBindData Bind(string service, Func<IContainer, object[], object> concrete, bool isStatic)
        {
            Guard.NotEmptyOrNull(service, "service");
            Guard.NotNull(concrete, "concrete");
            service = NormalizeService(service);
            lock (syncRoot)
            {
                if (binds.ContainsKey(service))
                {
                    throw new RuntimeException("Bind [" + service + "] already exists.");
                }

                if (instances.ContainsKey(service))
                {
                    throw new RuntimeException("Instances [" + service + "] is already exists.");
                }

                if (aliases.ContainsKey(service))
                {
                    throw new RuntimeException("Aliase [" + service + "] is already exists.");
                }

                var bindData = new BindData(this, service, concrete, isStatic);
                binds.Add(service, bindData);

                return bindData;
            }
        }

        /// <summary>
        /// 以依赖注入形式调用一个方法
        /// </summary>
        /// <param name="instance">方法对象</param>
        /// <param name="method">方法名</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法返回值</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/>,<paramref name="method"/>为<c>null</c>或者空字符串</exception>
        public object Call(object instance, string method, params object[] userParams)
        {
            Guard.NotNull(instance, "instance");
            Guard.NotEmptyOrNull(method, "method");

            var methodInfo = instance.GetType().GetMethod(method);
            return Call(instance, methodInfo, userParams);
        }

        /// <summary>
        /// 以依赖注入形式调用一个方法
        /// </summary>
        /// <param name="instance">方法对象</param>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法返回值</returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/>,<paramref name="methodInfo"/>为<c>null</c></exception>
        public object Call(object instance, MethodInfo methodInfo, params object[] userParams)
        {
            Guard.NotNull(instance, "instance");
            Guard.NotNull(methodInfo, "methodInfo");

            var type = instance.GetType();
            var parameter = new List<ParameterInfo>(methodInfo.GetParameters());

            lock (syncRoot)
            {
                var bindData = GetBindFillable(Type2Service(type));
                userParams = parameter.Count > 0 ? GetDependencies(bindData, parameter, userParams) : new object[] { };
                return methodInfo.Invoke(instance, userParams);
            }
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="method">方法</param>
        public void Call<T1>(Action<T1> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Call(method.Target, method.Method);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="method">方法</param>
        public void Call<T1, T2>(Action<T1, T2> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Call(method.Target, method.Method);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="method">方法</param>
        public void Call<T1, T2, T3>(Action<T1, T2, T3> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Call(method.Target, method.Method);
        }

        /// <summary>
        /// 以依赖注入的形式调用一个方法
        /// </summary>
        /// <param name="method">方法</param>
        public void Call<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method)
        {
            Guard.Requires<ArgumentNullException>(method != null);
            Call(method.Target, method.Method);
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
        public Action Wrap<T1>(Action<T1> method, params object[] userParams)
        {
            return () =>
            {
                if (method != null)
                {
                    Call(method.Target, method.Method, userParams);
                }
            };
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
        public Action Wrap<T1, T2>(Action<T1, T2> method, params object[] userParams)
        {
            return () =>
            {
                if (method != null)
                {
                    Call(method.Target, method.Method, userParams);
                }
            };
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
        public Action Wrap<T1, T2, T3>(Action<T1, T2, T3> method, params object[] userParams)
        {
            return () =>
            {
                if (method != null)
                {
                    Call(method.Target, method.Method, userParams);
                }
            };
        }

        /// <summary>
        /// 包装一个依赖注入形式调用的一个方法
        /// </summary>
        /// <param name="method">方法</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>包装方法</returns>
        public Action Wrap<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, params object[] userParams)
        {
            return () =>
            {
                if (method != null)
                {
                    Call(method.Target, method.Method, userParams);
                }
            };
        }

        /// <summary>
        /// 构造服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <param name="userParams">用户传入的构造参数</param>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>为<c>null</c>或者空字符串</exception>
        /// <exception cref="RuntimeException">出现循环依赖</exception>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        public object MakeWith(string service, params object[] userParams)
        {
            Guard.NotEmptyOrNull(service, "service");
            lock (syncRoot)
            {
                service = NormalizeService(service);
                service = AliasToService(service);

                object instance;
                if (instances.TryGetValue(service, out instance))
                {
                    return instance;
                }

                if (buildStack.Contains(service))
                {
                    throw new RuntimeException("Circular dependency detected while for [" + service + "].");
                }

                buildStack.Push(service);
                userParamsStack.Push(userParams);
                try
                {
                    return Resolve(service, null, true, userParams);
                }
                finally
                {
                    userParamsStack.Pop();
                    buildStack.Pop();
                }
            }
        }

        /// <summary>
        /// 构造服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>为<c>null</c>或者空字符串</exception>
        /// <exception cref="RuntimeException">出现循环依赖</exception>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        public object Make(string service)
        {
            return MakeWith(service);
        }

        /// <summary>
        /// 构造服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        public object this[string service]
        {
            get { return Make(service); }
        }

        /// <summary>
        /// 获取一个回调，当执行回调可以生成指定的服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>回调方案</returns>
        public Func<object> Factory(string service)
        {
            return () => Make(service);
        }

        /// <summary>
        /// 静态化一个服务,实例值会经过解决修饰器
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <param name="instance">服务实例，<c>null</c>也是合法的实例值</param>
        /// <exception cref="ArgumentNullException"><paramref name="service"/>为<c>null</c>或者空字符串</exception>
        /// <exception cref="RuntimeException"><paramref name="service"/>的服务在绑定设置中不是静态的</exception>
        public void Instance(string service, object instance)
        {
            Guard.NotEmptyOrNull(service, "service");
            lock (syncRoot)
            {
                service = NormalizeService(service);
                service = AliasToService(service);

                var bindData = GetBind(service);
                if (bindData != null)
                {
                    if (!bindData.IsStatic)
                    {
                        throw new RuntimeException("Service [" + service + "] is not Singleton(Static) Bind.");
                    }
                    instance = ((BindData)bindData).TriggerResolving(instance);
                }
                else
                {
                    bindData = MakeEmptyBindData(service);
                }

                Release(service);

                instance = TriggerOnResolving(bindData, instance);
                instances.Add(service, instance);
            }
        }

        /// <summary>
        /// 释放静态化实例
        /// </summary>
        /// <param name="service">服务名或别名</param>
        public void Release(string service)
        {
            Guard.NotEmptyOrNull(service, "service");
            lock (syncRoot)
            {
                service = NormalizeService(service);
                service = AliasToService(service);

                object instance;
                if (!instances.TryGetValue(service, out instance))
                {
                    return;
                }

                var bindData = GetBindFillable(service);
                bindData.TriggerRelease(instance);
                TriggerOnRelease(bindData, instance);
                instances.Remove(service);
            }
        }

        /// <summary>
        /// 清空容器的所有实例，绑定，别名，标签，解决器
        /// </summary>
        public void Flush()
        {
            lock (syncRoot)
            {
                var releaseList = new string[instances.Count];
                var i = 0;
                foreach (var instance in instances)
                {
                    releaseList[i++] = instance.Key;
                }
                foreach (var service in releaseList)
                {
                    Release(service);
                }

                binds.Clear();
                instances.Clear();
                aliases.Clear();
                aliasesReverse.Clear();
                tags.Clear();
                resolving.Clear();
                release.Clear();
                findType.Clear();
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
            Guard.NotNull(finder, "finder");
            lock (syncRoot)
            {
                findType.Add(finder, priority);
            }
            return this;
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="action">处理释放时的回调</param>
        /// <returns>当前容器实例</returns>
        public IContainer OnRelease(Action<IBindData, object> action)
        {
            Guard.NotNull(action, "action");
            lock (syncRoot)
            {
                release.Add(action);
            }
            return this;
        }

        /// <summary>
        /// 当服务被解决时，生成的服务会经过注册的回调函数
        /// </summary>
        /// <param name="func">回调函数</param>
        /// <returns>当前容器对象</returns>
        public IContainer OnResolving(Func<IBindData, object, object> func)
        {
            Guard.NotNull(func, "func");
            lock (syncRoot)
            {
                resolving.Add(func);

                var result = new Dictionary<string, object>();
                foreach (var data in instances)
                {
                    var bindData = GetBindFillable(data.Key);
                    result[data.Key] = func.Invoke(bindData, data.Value);
                }
                foreach (var data in result)
                {
                    instances[data.Key] = data.Value;
                }
                result.Clear();
            }
            return this;
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
        /// <param name="service">服务名或者别名</param>
        public void UnBind(string service)
        {
            lock (syncRoot)
            {
                service = NormalizeService(service);
                service = AliasToService(service);

                Release(service);
                List<string> serviceList;
                if (aliasesReverse.TryGetValue(service, out serviceList))
                {
                    foreach (var alias in serviceList)
                    {
                        aliases.Remove(alias);
                    }
                    aliasesReverse.Remove(service);
                }
                binds.Remove(service);
            }
        }

        /// <summary>
        /// 构造服务实现
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="makeServiceType">服务类型</param>
        /// <param name="userParams">用户传入的构造参数</param>
        /// <returns>服务实例</returns>
        protected object Build(BindData makeServiceBindData, Type makeServiceType, object[] userParams)
        {
            userParams = userParams ?? new object[] { };

            if (makeServiceType == null 
                || makeServiceType.IsAbstract 
                || makeServiceType.IsInterface)
            {
                return null;
            }

            var constructor = makeServiceType.GetConstructors();
            if (constructor.Length <= 0)
            {
                try
                {
                    return Activator.CreateInstance(makeServiceType);
                }
                catch (Exception ex)
                {
                    throw MakeBuildFaildException(makeServiceType, ex);
                }
            }

            var parameter = new List<ParameterInfo>(constructor[constructor.Length - 1].GetParameters());

            if (parameter.Count > 0)
            {
                userParams = GetDependencies(makeServiceBindData, parameter, userParams);
            }

            try
            {
                return Activator.CreateInstance(makeServiceType, userParams);
            }
            catch (Exception ex)
            {
                throw MakeBuildFaildException(makeServiceType, ex);
            }
        }

        /// <summary>
        /// 属性注入
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="makeServiceInstance">服务实例</param>
        /// <returns>服务实例</returns>
        /// <exception cref="RuntimeException">属性是必须的或者注入类型和需求类型不一致</exception>
        protected void AttributeInject<T>(Bindable<T> makeServiceBindData, object makeServiceInstance) where T : class , IBindable<T>
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

                InjectAttribute injectAttr = null;
                var needService = GetPropertyNeedsService(property, ref injectAttr);

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

                if (!CheckInjectAttrRequired(injectAttr, instance))
                {
                    throw new RuntimeException("[" + makeServiceBindData.Service + "] Attr [" + property.PropertyType + "] Required [" + needService + "] Service.");
                }

                if (!CheckInstanceIsInstanceOfType(property.PropertyType, instance))
                {
                    throw new RuntimeException("[" + makeServiceBindData.Service + "] Attr inject type must be [" + property.PropertyType + "] , But instance is [" + instance.GetType() + "] , Make service is [" + needService + "].");
                }

                property.SetValue(makeServiceInstance, instance, null);
            }
        }

        /// <summary>
        /// 获取依赖解决结果
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="baseParams">服务实例的参数信息</param>
        /// <param name="userParams">输入的构造参数列表</param>
        /// <returns>服务所需参数的解决结果</returns>
        /// <exception cref="RuntimeException">生成的实例类型和需求类型不一致</exception>
        protected object[] GetDependencies<T>(Bindable<T> makeServiceBindData, IList<ParameterInfo> baseParams, object[] userParams) where T : class, IBindable<T>
        {
            var results = new List<object>();

            foreach (var baseParam in baseParams)
            {
                var instance = GetDependenciesFromUserParams(baseParam, ref userParams);
                if (instance != null)
                {
                    results.Add(instance);
                    continue;
                }

                InjectAttribute injectAttr = null;
                var needService = GetParamNeedsService(baseParam, ref injectAttr);
            
                if (baseParam.ParameterType.IsClass 
                    || baseParam.ParameterType.IsInterface)
                {
                    instance = ResloveClass(makeServiceBindData, needService, baseParam);
                }
                else
                {
                    instance = ResolvePrimitive(makeServiceBindData, needService, baseParam);
                }

                if (!CheckInjectAttrRequired(injectAttr, instance))
                {
                    throw new RuntimeException("[" + makeServiceBindData.Service + "] Required [" + baseParam.ParameterType + "] Service.");
                }

                if (!CheckInstanceIsInstanceOfType(baseParam.ParameterType, instance))
                {
                    throw new RuntimeException("[" + makeServiceBindData.Service + "] Params inject type must be [" + baseParam.ParameterType + "] , But instance is [" + instance.GetType() + "] Make service is [" + needService + "].");
                }

                results.Add(instance);
            }

            return results.ToArray();
        }

        /// <summary>
        /// 从用户传入的参数中获取依赖
        /// </summary>
        /// <param name="baseParam">基础参数</param>
        /// <param name="userParams">用户传入参数</param>
        /// <returns>合适的注入参数</returns>
        protected object GetDependenciesFromUserParams(ParameterInfo baseParam, ref object[] userParams)
        {
            if (userParams == null)
            {
                return null;
            }

            GuardUserParamsCount(userParams.Length);

            for (var n = 0; n < userParams.Length; n++)
            {
                var userParam = userParams[n];
                if (baseParam.ParameterType.IsInstanceOfType(userParam))
                {
                    return Arr.RemoveAt(ref userParams, n);
                }

                try
                {
                    if (baseParam.ParameterType.IsPrimitive && userParam is IConvertible)
                    {
                        var result = Convert.ChangeType(userParam, baseParam.ParameterType);
                        Arr.RemoveAt(ref userParams, n);
                        return result;
                    }
                }
                catch (Exception)
                {
                    /*
                    throw new RuntimeException(
                        string.Format("Params [{0}({1})] can not convert to [{2}] , Service [{3}]",
                            baseParam.Name, userParam, baseParam.ParameterType, service
                        ), ex);*/
                }
            }

            return null;
        }

        /// <summary>
        /// 获取字段需求服务
        /// </summary>
        /// <param name="property">字段</param>
        /// <param name="injectAttr">依赖注入标记</param>
        /// <returns>需求的服务名</returns>
        protected string GetPropertyNeedsService(PropertyInfo property, ref InjectAttribute injectAttr)
        {
            injectAttr = (InjectAttribute)property.GetCustomAttributes(injectTarget, false)[0];
            return string.IsNullOrEmpty(injectAttr.Alias)
                ? Type2Service(property.PropertyType)
                : injectAttr.Alias;
        }

        /// <summary>
        /// 获取参数需求服务
        /// </summary>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <param name="injectAttr">依赖注入标记</param>
        /// <returns>需求的服务名</returns>
        protected string GetParamNeedsService(ParameterInfo baseParam, ref InjectAttribute injectAttr)
        {
            var needService = Type2Service(baseParam.ParameterType);
            if (baseParam.IsDefined(injectTarget, false))
            {
                var propertyAttrs = baseParam.GetCustomAttributes(injectTarget, false);
                if (propertyAttrs.Length > 0)
                {
                    injectAttr = (InjectAttribute)propertyAttrs[0];
                    if (!string.IsNullOrEmpty(injectAttr.Alias))
                    {
                        needService = injectAttr.Alias;
                    }
                }
            }
            return needService;
        }

        /// <summary>
        /// 解决基本类型
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">希望解决的服务名或者别名</param>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>解决结果</returns>
        protected object ResolveAttrPrimitive<T>(Bindable<T> makeServiceBindData, string service, PropertyInfo baseParam) where T : class, IBindable<T>
        {
            return Make(makeServiceBindData.GetContextual(service));
        }

        /// <summary>
        /// 解决类类型
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">希望解决的服务名或者别名</param>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>解决结果</returns>
        protected object ResloveAttrClass<T>(Bindable<T> makeServiceBindData, string service, PropertyInfo baseParam) where T : class, IBindable<T>
        {
            return Make(makeServiceBindData.GetContextual(service));
        }

        /// <summary>
        /// 解决基本类型
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">希望解决的服务名或者别名</param>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>解决结果</returns>
        protected object ResolvePrimitive<T>(Bindable<T> makeServiceBindData, string service , ParameterInfo baseParam) where T : class, IBindable<T>
        {
            var newServiceBind = GetBind(makeServiceBindData.GetContextual("@" + baseParam.Name));
            if (newServiceBind != null)
            {
                return Make(newServiceBind.Service);
            }

            if (baseParam.DefaultValue != DBNull.Value)
            {
                return baseParam.DefaultValue;
            }

            return Make(makeServiceBindData.GetContextual(service));
        }

        /// <summary>
        /// 解决类类型
        /// </summary>
        /// <param name="makeServiceBindData">请求注入操作的服务绑定数据</param>
        /// <param name="service">希望解决的服务名或者别名</param>
        /// <param name="baseParam">当前正在解决的变量</param>
        /// <returns>解决结果</returns>
        protected object ResloveClass<T>(Bindable<T> makeServiceBindData, string service, ParameterInfo baseParam) where T : class, IBindable<T>
        {
            return Make(makeServiceBindData.GetContextual(service));
        }

        /// <summary>
        /// 触发全局解决修饰器
        /// </summary>
        /// <param name="bindData">服务绑定数据</param>
        /// <param name="obj">服务实例</param>
        /// <returns>被修饰器修饰后的服务实例</returns>
        protected object TriggerOnResolving(IBindData bindData, object obj)
        {
            foreach (var func in resolving)
            {
                obj = func(bindData, obj);
            }
            return obj;
        }

        /// <summary>
        /// 触发全局释放修饰器
        /// </summary>
        /// <param name="bindData">服务绑定数据</param>
        /// <param name="obj">服务实例</param>
        /// <returns>被修饰器修饰后的服务实例</returns>
        protected void TriggerOnRelease(IBindData bindData, object obj)
        {
            foreach (var action in release)
            {
                action.Invoke(bindData, obj);
            }
        }

        /// <summary>
        /// 制作一个空的绑定数据
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>空绑定数据</returns>
        protected BindData MakeEmptyBindData(string service)
        {
            return new BindData(this, service, null, false);
        }

        /// <summary>
        /// 生成一个编译失败异常
        /// </summary>
        /// <param name="makeServiceType">构造的服务类型</param>
        /// <param name="innerException">内部异常</param>
        /// <returns>运行时异常</returns>
        protected RuntimeException MakeBuildFaildException(Type makeServiceType, Exception innerException)
        {
            var message = "Target [" + makeServiceType + "] build faild.";
            if (buildStack.Count > 0)
            {
                var previous = string.Join(", ", buildStack.ToArray());
                message = "Target [" + makeServiceType + "] build faild while building [" + previous + "].";
            }
            return new RuntimeException(message, innerException);
        }

        /// <summary>
        /// 标准化服务名
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>标准化后的服务名</returns>
        protected string NormalizeService(string service)
        {
            return service.Trim();
        }

        /// <summary>
        /// 检查实例是否实现自某种类型
        /// </summary>
        /// <param name="type">需要实现自的类型</param>
        /// <param name="instance">生成的实例</param>
        /// <returns>是否符合类型</returns>
        protected bool CheckInstanceIsInstanceOfType(Type type, object instance)
        {
            return instance == null || type.IsInstanceOfType(instance);
        }

        /// <summary>
        /// 检查依赖注入的必须标记
        /// </summary>
        /// <param name="injectAttr">依赖注入标记</param>
        /// <param name="instance">生成的实例</param>
        /// <returns>是否通过检查</returns>
        protected bool CheckInjectAttrRequired(InjectAttribute injectAttr, object instance)
        {
            return injectAttr == null || !injectAttr.Required || instance != null;
        }

        /// <summary>
        /// 保证用户传入参数必须小于指定值
        /// </summary>
        /// <param name="count">传入参数数量</param>
        protected void GuardUserParamsCount(int count)
        {
            if (count > 255)
            {
                throw new RuntimeException("Too many parameters , must be less than 255");
            }
        }

        /// <summary>
        /// 获取通过服务名获取服务的类型
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>服务类型</returns>
        protected Type GetServiceType(string service)
        {
            foreach (var finder in findType)
            {
                var type = finder.Invoke(service);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        /// <summary>
        /// 解决服务
        /// </summary>
        /// <param name="makeService">服务名</param>
        /// <param name="makeServiceType">服务类型</param>
        /// <param name="isFromMake">是否直接调用自Make函数</param>
        /// <param name="userParams">用户传入的构造参数</param>
        /// <returns>服务实例</returns>
        private object Resolve(string makeService, Type makeServiceType, bool isFromMake, params object[] userParams)
        {
            var bindData = GetBindFillable(makeService);
            var buildInstance = isFromMake ? BuildUseConcrete(bindData, makeServiceType, userParams)
                : Build(bindData, makeServiceType ?? GetServiceType(bindData.Service), userParams);

            //只有是来自于make函数的调用时才执行di，包装，以及修饰
            if (!isFromMake)
            {
                return buildInstance;
            }

            AttributeInject(bindData, buildInstance);

            if (bindData.IsStatic)
            {
                Instance(makeService, buildInstance);
            }
            else
            {
                buildInstance = TriggerOnResolving(bindData, bindData.TriggerResolving(buildInstance));
            }

            return buildInstance;
        }

        /// <summary>
        /// 常规编译一个服务
        /// </summary>
        /// <param name="makeServiceBindData">服务绑定数据</param>
        /// <param name="makeServiceType">服务类型</param>
        /// <param name="param">构造参数</param>
        /// <returns>服务实例</returns>
        private object BuildUseConcrete(BindData makeServiceBindData, Type makeServiceType, object[] param)
        {
            return makeServiceBindData.Concrete != null ?
                makeServiceBindData.Concrete(this, param) :
                Resolve(makeServiceBindData.Service, makeServiceType, false, param);
        }

        /// <summary>
        /// 获取别名最终对应的服务名
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>最终映射的服务名</returns>
        private string AliasToService(string service)
        {
            string alias;
            return aliases.TryGetValue(service, out alias) ? alias : service;
        }

        /// <summary>
        /// 获取服务绑定数据,如果数据为null则填充数据
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>服务绑定数据</returns>
        private BindData GetBindFillable(string service)
        {
            BindData bindData;
            return binds.TryGetValue(service, out bindData) ? bindData : MakeEmptyBindData(service);
        }
    }
}