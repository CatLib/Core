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

using CatLib.Support;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace CatLib.Container
{
    /// <summary>
    /// The catlib ioc container implemented.
    /// </summary>
    public class Container : IContainer
    {
        /// <summary>
        /// Characters not allowed in the service name.
        /// </summary>
        private static readonly char[] ServiceBanChars = { '@', ':', '$' };

        /// <summary>
        /// The container's bindings.
        /// </summary>
        private readonly Dictionary<string, BindData> bindings;

        /// <summary>
        /// The container's singleton(static) instances.
        /// </summary>
        private readonly Dictionary<string, object> instances;

        /// <summary>
        /// The container's singleton instances inverse index mapping.
        /// </summary>
        private readonly Dictionary<object, string> instancesReverse;

        /// <summary>
        /// The registered aliases with service.
        /// </summary>
        private readonly Dictionary<string, string> aliases;

        /// <summary>
        /// The registered aliases with service. inverse index mapping.
        /// </summary>
        private readonly Dictionary<string, List<string>> aliasesReverse;

        /// <summary>
        /// All of the registered tags.
        /// </summary>
        private readonly Dictionary<string, List<string>> tags;

        /// <summary>
        /// All of the global resolving callbacks.
        /// </summary>
        private readonly List<Action<IBindData, object>> resolving;

        /// <summary>
        /// All of the global after resolving callbacks.
        /// </summary>
        private readonly List<Action<IBindData, object>> afterResloving;

        /// <summary>
        /// All of the global release callbacks.
        /// </summary>
        private readonly List<Action<IBindData, object>> release;

        /// <summary>
        /// The extension closures for services.
        /// </summary>
        private readonly Dictionary<string, List<Func<object, IContainer, object>>> extenders;

        /// <summary>
        /// The type finder convert a string to a service type.
        /// </summary>
        private readonly SortSet<Func<string, Type>, int> findType;

        /// <summary>
        /// The cache that has been type found.
        /// </summary>
        private readonly Dictionary<string, Type> findTypeCache;

        /// <summary>
        /// An hash set of the service that have been resolved.
        /// </summary>
        private readonly HashSet<string> resolved;

        /// <summary>
        /// The singleton service build timing.
        /// </summary>
        private readonly SortSet<string, int> instanceTiming;

        /// <summary>
        /// All of the registered rebound callbacks.
        /// </summary>
        private readonly Dictionary<string, List<Action<object>>> rebound;

        /// <summary>
        /// The method ioc container.
        /// </summary>
        private readonly MethodContainer methodContainer;

        /// <summary>
        /// Synchronous locking object.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// Represents a skipped object to skip some dependency injection.
        /// </summary>
        private readonly object skipped;

        /// <summary>
        /// Whether the container is flushing.
        /// </summary>
        private bool flushing;

        /// <summary>
        /// The unique Id is used to mark the global build order.
        /// </summary>
        private int instanceId;

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        /// <param name="prime">The estimated number of services.</param>
        public Container(int prime = 64)
        {
            prime = Math.Max(8, prime);
            tags = new Dictionary<string, List<string>>((int)(prime * 0.25));
            aliases = new Dictionary<string, string>(prime * 4);
            aliasesReverse = new Dictionary<string, List<string>>(prime * 4);
            instances = new Dictionary<string, object>(prime * 4);
            instancesReverse = new Dictionary<object, string>(prime * 4);
            bindings = new Dictionary<string, BindData>(prime * 4);
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
            skipped = new object();
            methodContainer = new MethodContainer(this);
            flushing = false;
            instanceId = 0;
        }

        /// <summary>
        /// Gets the stack of concretions currently being built.
        /// </summary>
        protected Stack<string> BuildStack { get; }

        /// <summary>
        /// Gets the stack of the user params being built.
        /// </summary>
        protected Stack<object[]> UserParamsStack { get; }

        /// <inheritdoc />
        public object this[string service]
        {
            get => Make(service);
            set
            {
                lock (syncRoot)
                {
                    GetBind(service)?.Unbind();
                    Bind(service, (container, args) => value, false);
                }
            }
        }

        /// <inheritdoc />
        public void Tag(string tag, params string[] services)
        {
            Guard.ParameterNotNull(tag, nameof(tag));

            services = services ?? Array.Empty<string>();

            lock (syncRoot)
            {
                GuardFlushing();

                if (!tags.TryGetValue(tag, out List<string> collection))
                {
                    tags[tag] = collection = new List<string>();
                }

                foreach (var service in services)
                {
                    if (string.IsNullOrEmpty(service))
                    {
                        continue;
                    }

                    collection.Add(service);
                }
            }
        }

        /// <inheritdoc />
        public object[] Tagged(string tag)
        {
            Guard.ParameterNotNull(tag, nameof(tag));

            lock (syncRoot)
            {
                if (!tags.TryGetValue(tag, out List<string> services))
                {
                    throw new LogicException($"Tag \"{tag}\" is not exist.");
                }

                return Arr.Map(services, (service) => Make(service));
            }
        }

        /// <inheritdoc />
        public IBindData GetBind(string service)
        {
            if (string.IsNullOrEmpty(service))
            {
                return null;
            }

            lock (syncRoot)
            {
                service = AliasToService(service);
                return bindings.TryGetValue(service, out BindData bindData)
                    ? bindData : null;
            }
        }

        /// <inheritdoc />
        public bool HasBind(string service)
        {
            return GetBind(service) != null;
        }

        /// <inheritdoc />
        public bool HasInstance(string service)
        {
            Guard.ParameterNotNull(service, nameof(service));

            lock (syncRoot)
            {
                service = AliasToService(service);
                return instances.ContainsKey(service);
            }
        }

        /// <inheritdoc />
        public bool IsResolved(string service)
        {
            Guard.ParameterNotNull(service, nameof(service));

            lock (syncRoot)
            {
                service = AliasToService(service);
                return resolved.Contains(service) || instances.ContainsKey(service);
            }
        }

        /// <inheritdoc />
        public bool CanMake(string service)
        {
            Guard.ParameterNotNull(service, nameof(service));

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

        /// <inheritdoc />
        public bool IsStatic(string service)
        {
            var bind = GetBind(service);
            return bind != null && bind.IsStatic;
        }

        /// <inheritdoc />
        public bool IsAlias(string name)
        {
            name = FormatService(name);
            return aliases.ContainsKey(name);
        }

        /// <inheritdoc />
        public IContainer Alias(string alias, string service)
        {
            Guard.ParameterNotNull(alias, nameof(alias));
            Guard.ParameterNotNull(service, nameof(service));

            if (alias == service)
            {
                throw new LogicException($"Alias is same as service: \"{alias}\".");
            }

            alias = FormatService(alias);
            service = AliasToService(service);

            lock (syncRoot)
            {
                GuardFlushing();

                if (aliases.ContainsKey(alias))
                {
                    throw new LogicException($"Alias \"{alias}\" is already exists.");
                }

                if (bindings.ContainsKey(alias))
                {
                    throw new LogicException($"Alias \"{alias}\" has been used for service name.");
                }

                if (!bindings.ContainsKey(service) && !instances.ContainsKey(service))
                {
                    throw new CodeStandardException(
                        $"You must {nameof(Bind)}() or {nameof(Instance)}() serivce before and you be able to called {nameof(Alias)}().");
                }

                aliases.Add(alias, service);

                if (!aliasesReverse.TryGetValue(service, out List<string> collection))
                {
                    aliasesReverse[service] = collection = new List<string>();
                }

                collection.Add(alias);
            }

            return this;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public IBindData Bind(string service, Type concrete, bool isStatic)
        {
            Guard.Requires<ArgumentNullException>(concrete != null, $"Parameter {nameof(concrete)} can not be null.");

            if (IsUnableType(concrete))
            {
                throw new LogicException($"Type \"{concrete}\" can not bind. please check if there is a list of types that cannot be built.");
            }

            service = FormatService(service);
            return Bind(service, WrapperTypeBuilder(service, concrete), isStatic);
        }

        /// <inheritdoc />
        public IBindData Bind(string service, Func<IContainer, object[], object> concrete, bool isStatic)
        {
            Guard.ParameterNotNull(service, nameof(service));
            Guard.ParameterNotNull(concrete, nameof(concrete));
            GuardServiceName(service);

            service = FormatService(service);
            lock (syncRoot)
            {
                GuardFlushing();

                if (bindings.ContainsKey(service))
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
                bindings.Add(service, bindData);

                if (!IsResolved(service))
                {
                    return bindData;
                }

                if (isStatic)
                {
                    // If it is "static" then solve this service directly
                    // The process of staticizing the service triggers TriggerOnRebound
                    Make(service);
                }
                else
                {
                    TriggerOnRebound(service);
                }

                return bindData;
            }
        }

        /// <inheritdoc />
        public IMethodBind BindMethod(string method, object target, MethodInfo called)
        {
            GuardFlushing();
            GuardMethodName(method);
            return methodContainer.Bind(method, target, called);
        }

        /// <inheritdoc />
        public void UnbindMethod(object target)
        {
            methodContainer.Unbind(target);
        }

        /// <inheritdoc />
        public object Invoke(string method, params object[] userParams)
        {
            GuardConstruct(nameof(Invoke));
            return methodContainer.Invoke(method, userParams);
        }

        /// <inheritdoc />
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
                userParams = GetDependencies(bindData, parameter, userParams) ?? Array.Empty<object>();
                return methodInfo.Invoke(target, userParams);
            }
        }

        /// <inheritdoc />
        public object Make(string service, params object[] userParams)
        {
            GuardConstruct(nameof(Make));
            return Resolve(service, userParams);
        }

        /// <inheritdoc />
        public void Extend(string service, Func<object, IContainer, object> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);

            lock (syncRoot)
            {
                GuardFlushing();

                service = string.IsNullOrEmpty(service) ? string.Empty : AliasToService(service);

                if (!string.IsNullOrEmpty(service) && instances.TryGetValue(service, out object instance))
                {
                    // If the instance already exists then apply the extension.
                    // Extensions will no longer be added to the permanent extension list.
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

                if (!string.IsNullOrEmpty(service) && IsResolved(service))
                {
                    TriggerOnRebound(service);
                }
            }
        }

        /// <summary>
        /// Remove all extensions for the specified service.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
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

        /// <inheritdoc />
        public object Instance(string service, object instance)
        {
            Guard.ParameterNotNull(service, nameof(service));

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

        /// <inheritdoc />
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
                        // Prevent the use of a string as a service name.
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

        /// <inheritdoc />
        public IContainer OnFindType(Func<string, Type> func, int priority = int.MaxValue)
        {
            Guard.Requires<ArgumentNullException>(func != null);

            lock (syncRoot)
            {
                GuardFlushing();
                findType.Add(func, priority);
            }

            return this;
        }

        /// <inheritdoc />
        public IContainer OnRelease(Action<IBindData, object> closure)
        {
            AddClosure(closure, release);
            return this;
        }

        /// <inheritdoc />
        public IContainer OnResolving(Action<IBindData, object> closure)
        {
            AddClosure(closure, resolving);
            return this;
        }

        /// <inheritdoc />
        public IContainer OnAfterResolving(Action<IBindData, object> closure)
        {
            AddClosure(closure, afterResloving);
            return this;
        }

        /// <inheritdoc />
        public IContainer OnRebound(string service, Action<object> callback)
        {
            Guard.Requires<ArgumentNullException>(callback != null);

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

        /// <inheritdoc />
        public void Unbind(string service)
        {
            service = AliasToService(service);
            var bind = GetBind(service);
            bind?.Unbind();
        }

        /// <inheritdoc />
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
                    bindings.Clear();
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

        /// <inheritdoc />
        public string Type2Service(Type type)
        {
            return type.ToString();
        }

        /// <summary>
        /// Trigger all callbacks in specified list.
        /// </summary>
        /// <param name="bindData">The bind data for <see cref="Make"/> service.</param>
        /// <param name="instance">The service instance.</param>
        /// <param name="list">The specified list.</param>
        /// <returns>The decorated service instance.</returns>
        internal static object Trigger(IBindData bindData, object instance, List<Action<IBindData, object>> list)
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
        /// Unbind the service from the container.
        /// </summary>
        /// <param name="bindable">The bindable instance.</param>
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

                bindings.Remove(bindable.Service);
            }
        }

        /// <summary>
        /// Gets an array of resolved instances for dependent parameters.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for <see cref="Make"/> service.</param>
        /// <param name="baseParams">The dependent parameters array for <see cref="Make"/> service.</param>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>An array of resolved instances for dependent parameters.</returns>
        protected internal virtual object[] GetDependencies(Bindable makeServiceBindData, ParameterInfo[] baseParams, object[] userParams)
        {
            if (baseParams.Length <= 0)
            {
                return Array.Empty<object>();
            }

            var results = new object[baseParams.Length];

            // Gets a parameter matcher for filtering parameters
            var matcher = GetParamsMatcher(ref userParams);

            for (var i = 0; i < baseParams.Length; i++)
            {
                var baseParam = baseParams[i];

                // Parameter matching is used to match the parameters.
                // The parameter matchers are the first to perform because their
                // matching accuracy is the most accurate.
                var param = matcher?.Invoke(baseParam);

                // When the container finds that the developer uses object or object[] as
                // the dependency parameter type, we try to compact inject the user parameters.
                param = param ?? GetCompactInjectUserParams(baseParam, ref userParams);

                // Select the appropriate parameters from the user parameters and inject
                // them in the relative order.
                param = param ?? GetDependenciesFromUserParams(baseParam, ref userParams);

                string needService = null;

                if (param == null)
                {
                    // Try to generate the required parameters through the dependency
                    // injection container.
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

                // Perform dependency injection checking on the obtained injection instance.
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
        /// Determine if specified type is the default base type of the container.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <returns>True if the specified type is the default base type. otherwise false.</returns>
        protected virtual bool IsBasicType(Type type)
        {
            return type == null || type.IsPrimitive || type == typeof(string);
        }

        /// <summary>
        /// Determine the specified type is cannot built.
        /// </summary>
        /// <param name="type">The specified type.</param>
        /// <returns>True if the specified type is cannot built. otherwise false.</returns>
        protected virtual bool IsUnableType(Type type)
        {
            return type == null || type.IsAbstract || type.IsInterface || type.IsArray || type.IsEnum
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Wrap a specified type.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="concrete">The service concrete type.</param>
        /// <returns>Return a closure, call it to get the service instance.</returns>
        protected virtual Func<IContainer, object[], object> WrapperTypeBuilder(string service, Type concrete)
        {
            return (container, userParams) => ((Container)container).CreateInstance(GetBindFillable(service), concrete,
                userParams);
        }

        /// <summary>
        /// Get dependencies from the user parameters.
        /// </summary>
        /// <param name="baseParam">The depend type.</param>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The instance that match the type of dependency.</returns>
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
        /// Convert instance to specified type.
        /// </summary>
        /// <param name="result">The instance.</param>
        /// <param name="conversionType">The specified type.</param>
        /// <returns>True if the conversion was successful, otherwise false.</returns>
        protected virtual bool ChangeType(ref object result, Type conversionType)
        {
            try
            {
                if (result == null || conversionType.IsInstanceOfType(result))
                {
                    return true;
                }

                if (IsBasicType(result.GetType()) && conversionType.IsDefined(typeof(VariantAttribute), false))
                {
                    try
                    {
                        result = Make(Type2Service(conversionType), result);
                        return true;
                    }
#pragma warning disable CA1031
                    catch (Exception)
#pragma warning restore CA1031
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
#pragma warning disable CA1031
            catch (Exception)
#pragma warning restore CA1031
            {
                // ignored
                // when throw exception then stop inject
            }

            return false;
        }

        /// <summary>
        /// Convert <see cref="PropertyInfo"/> to the service name.
        /// </summary>
        /// <param name="propertyInfo">The property.</param>
        /// <returns>The service name.</returns>
        protected virtual string GetPropertyNeedsService(PropertyInfo propertyInfo)
        {
            return Type2Service(propertyInfo.PropertyType);
        }

        /// <summary>
        /// Convert <see cref="ParameterInfo"/> to the service name.
        /// </summary>
        /// <param name="baseParam">The parameter.</param>
        /// <returns>The service name.</returns>
        protected virtual string GetParamNeedsService(ParameterInfo baseParam)
        {
            return Type2Service(baseParam.ParameterType);
        }

        /// <summary>
        /// Gets build closures based on context.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for the <see cref="Make"/> service.</param>
        /// <param name="service">The service name for dependent.</param>
        /// <param name="paramName">The parameter name for dependent.</param>
        /// <returns>The closure, call returned the dependency instance.</returns>
        protected virtual Func<object> GetContextualClosure(Bindable makeServiceBindData, string service,
            string paramName)
        {
            return makeServiceBindData.GetContextualClosure(service) ??
                   makeServiceBindData.GetContextualClosure($"{GetVariableTag()}{paramName}");
        }

        /// <inheritdoc cref="GetContextualClosure"/>
        /// <summary>
        /// Get build service based on context.
        /// </summary>
        /// <returns>The dependency service name.</returns>
        protected virtual string GetContextualService(Bindable makeServiceBindData, string service, string paramName)
        {
            return makeServiceBindData.GetContextual(service) ??
                   makeServiceBindData.GetContextual($"{GetVariableTag()}{paramName}") ??
                   service;
        }

        /// <summary>
        /// Gets the instance from closure.
        /// </summary>
        /// <param name="closure">The closure.</param>
        /// <param name="needType">The expected type.</param>
        /// <param name="ouput">The instance.</param>
        /// <returns>True if the build is successful and matches the expected type, otherwise false.</returns>
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

        /// <inheritdoc cref="MakeFromContextualClosure"/>
        /// <summary>
        /// Gets the instance from service name.
        /// </summary>
        /// <param name="service">The service name.</param>
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
        /// Resolve the specified service based on context.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for the <see cref="Make"/> service.</param>
        /// <param name="service">The service name for dependent.</param>
        /// <param name="paramName">The parameter or property name for dependent.</param>
        /// <param name="paramType">The parameter or property type for dependent.</param>
        /// <param name="output">The dependency instance.</param>
        /// <returns>True if build the dependency instance successful. otherwise false.</returns>
        protected virtual bool ResloveFromContextual(Bindable makeServiceBindData, string service, string paramName,
            Type paramType, out object output)
        {
            if (MakeFromContextualClosure(
                GetContextualClosure(makeServiceBindData, service, paramName),
                paramType, out output))
            {
                return true;
            }

            return MakeFromContextualService(
                GetContextualService(makeServiceBindData, service, paramName),
                paramType, out output);
        }

        /// <summary>
        /// Resolved the attribute selector's primitive type.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for the <see cref="Make"/> service.</param>
        /// <param name="service">The name or alias of the service dependent needs to resolved.</param>
        /// <param name="baseParam">The property for dependent.</param>
        /// <returns>The dependency instance.</returns>
        protected virtual object ResolveAttrPrimitive(Bindable makeServiceBindData, string service, PropertyInfo baseParam)
        {
            if (ResloveFromContextual(makeServiceBindData, service, baseParam.Name, baseParam.PropertyType,
                out object instance))
            {
                return instance;
            }

            if (baseParam.PropertyType.IsGenericType && baseParam.PropertyType.GetGenericTypeDefinition() ==
                typeof(Nullable<>))
            {
                return null;
            }

            var inject = (InjectAttribute)baseParam.GetCustomAttribute(typeof(InjectAttribute));
            if (inject != null && !inject.Required)
            {
                return skipped;
            }

            throw MakeUnresolvableException(baseParam.Name, baseParam.DeclaringType);
        }

        /// <inheritdoc cref="ResolveAttrPrimitive"/>
        /// <summary>
        /// Resolved the attribute selector's reference type.
        /// </summary>
        protected virtual object ResloveAttrClass(Bindable makeServiceBindData, string service, PropertyInfo baseParam)
        {
            if (ResloveFromContextual(makeServiceBindData, service, baseParam.Name, baseParam.PropertyType,
                out object instance))
            {
                return instance;
            }

            var inject = (InjectAttribute)baseParam.GetCustomAttribute(typeof(InjectAttribute));
            if (inject != null && !inject.Required)
            {
                return skipped;
            }

            throw MakeUnresolvableException(baseParam.Name, baseParam.DeclaringType);
        }

        /// <summary>
        /// Resolved the constructor's primitive type.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for the <see cref="Make"/> service.</param>
        /// <param name="service">The name or alias of the service dependent needs to resolved.</param>
        /// <param name="baseParam">The parameter for dependent.</param>
        /// <returns>The dependency instance.</returns>
        protected virtual object ResolvePrimitive(Bindable makeServiceBindData, string service, ParameterInfo baseParam)
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

            if (baseParam.ParameterType.IsGenericType && baseParam.ParameterType.GetGenericTypeDefinition() ==
                typeof(Nullable<>))
            {
                return null;
            }

            throw MakeUnresolvableException(
                baseParam.Name,
                baseParam.Member?.DeclaringType);
        }

        /// <inheritdoc cref="ResolvePrimitive"/>
        /// <summary>
        /// Resolved the constructor's reference type.
        /// </summary>
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

            // baseParam.Member maybe empty and may occur when some underlying
            // development overwrites ParameterInfo class.
            throw MakeUnresolvableException(
                baseParam.Name,
                baseParam.Member?.DeclaringType);
        }

        /// <summary>
        /// Gets variable characters.
        /// </summary>
        /// <returns>The variable characters.</returns>
        protected virtual char GetVariableTag()
        {
            return '$';
        }

        /// <summary>
        /// Gets the debug message of the build stack.
        /// </summary>
        /// <returns>The debug message of the build stack.</returns>
        protected virtual string GetBuildStackDebugMessage()
        {
            var previous = string.Join(", ", BuildStack.ToArray());
            return $" While building stack [{previous}].";
        }

        /// <summary>
        /// Build a reslove failure exception.
        /// </summary>
        /// <param name="makeService">The <see cref="Make"/> service name.</param>
        /// <param name="makeServiceType">The <see cref="Make"/> service type.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <returns>The resolve failure exception instance.</returns>
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
        /// Gets the inner exception debug message.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <returns>The debug message.</returns>
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
            }
            while ((innerException = innerException.InnerException) != null);
            return $" InnerException message stack: [{stack}]";
        }

        /// <summary>
        /// Build a unresolved exception.
        /// </summary>
        /// <param name="name">The parameter or property name for dependent.</param>
        /// <param name="declaringClass">Declaring class type for parameter or property.</param>
        /// <returns>The unresolved exception instance.</returns>
        protected virtual UnresolvableException MakeUnresolvableException(string name, Type declaringClass)
        {
            return new UnresolvableException(
                $"Unresolvable dependency , resolving [{name ?? "Unknow"}] in class [{declaringClass?.ToString() ?? "Unknow"}]");
        }

        /// <summary>
        /// Build a circular dependency exception.
        /// </summary>
        /// <param name="service">The name of the service that throws the exception.</param>
        /// <returns>The circular dependency exception.</returns>
        protected virtual LogicException MakeCircularDependencyException(string service)
        {
            var message = $"Circular dependency detected while for [{service}].";
            message += GetBuildStackDebugMessage();
            return new LogicException(message);
        }

        /// <summary>
        /// Format the service name.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <returns>The formatted service name.</returns>
        protected virtual string FormatService(string service)
        {
            return service.Trim();
        }

        /// <summary>
        /// Check if the specified instance can be injected.
        /// </summary>
        /// <param name="type">The expected type.</param>
        /// <param name="instance">The specified instance.</param>
        /// <returns>True if the instance can be injected. otherwise false.</returns>
        protected virtual bool CanInject(Type type, object instance)
        {
            return instance == null || type.IsInstanceOfType(instance);
        }

        /// <summary>
        /// Ensure that the number of parameters passed in by the user must be less than the specified value.
        /// </summary>
        /// <param name="count">The specified count.</param>
        protected virtual void GuardUserParamsCount(int count)
        {
            if (count > 255)
            {
                throw new LogicException($"Too many parameters , must be less or equal than 255 or override the {nameof(GuardUserParamsCount)} method.");
            }
        }

        /// <summary>
        /// Ensure that the specified instance is valid.
        /// </summary>
        /// <param name="instance">The specified instance.</param>
        /// <param name="makeService">The <see cref="Make"/> service name.</param>
        protected virtual void GuardResolveInstance(object instance, string makeService)
        {
            if (instance == null)
            {
                throw MakeBuildFaildException(makeService, SpeculatedServiceType(makeService), null);
            }
        }

        /// <summary>
        /// Speculative service type based on specified service name.
        /// </summary>
        /// <param name="service">The specified service name.</param>
        /// <returns>The speculative service type.</returns>
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
        /// Dependency injection on the property selector.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for <see cref="Make"/> service.</param>
        /// <param name="makeServiceInstance">The instance for <see cref="Make"/> service.</param>
        protected virtual void AttributeInject(Bindable makeServiceBindData, object makeServiceInstance)
        {
            if (makeServiceInstance == null)
            {
                return;
            }

            foreach (var property in makeServiceInstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.CanWrite
                    || !property.IsDefined(typeof(InjectAttribute), false))
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

                if (ReferenceEquals(instance, skipped))
                {
                    continue;
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
        /// Check if the parameter passed in by the user can be injected compact.
        /// </summary>
        /// <param name="baseParam">The parameter for dependent.</param>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>True if the parameter can be injected tightly. otherwise false.</returns>
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
        /// Gets the parameters injected through the compact.
        /// </summary>
        /// <param name="baseParam">The parameter for dependent.</param>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>Parameters that can be injected in a compact.</returns>
        protected virtual object GetCompactInjectUserParams(ParameterInfo baseParam, ref object[] userParams)
        {
            if (!CheckCompactInjectUserParams(baseParam, userParams))
            {
                return null;
            }

            try
            {
                if (baseParam.ParameterType == typeof(object)
                    && userParams != null && userParams.Length == 1)
                {
                    return userParams[0];
                }

                return userParams;
            }
            finally
            {
                userParams = null;
            }
        }

        /// <summary>
        /// Gets the parameter matcher.
        /// </summary>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>Returns the parameter matcher, if it is null, there is no matcher.</returns>
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
        /// Select the appropriate constructor and get the corresponding array of parameter instances.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for <see cref="Make"/> service.</param>
        /// <param name="makeServiceType">The type for <see cref="Make"/> service.</param>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>An array of resolved instances for dependent parameters.</returns>
        protected virtual object[] GetConstructorsInjectParams(Bindable makeServiceBindData, Type makeServiceType, object[] userParams)
        {
            var constructors = makeServiceType.GetConstructors();
            if (constructors.Length <= 0)
            {
                return Array.Empty<object>();
            }

            ExceptionDispatchInfo exceptionDispatchInfo = null;
            foreach (var constructor in constructors)
            {
                try
                {
                    return GetDependencies(makeServiceBindData, constructor.GetParameters(), userParams);
                }
#pragma warning disable CA1031
                catch (Exception ex)
                {
                    if (exceptionDispatchInfo == null)
                    {
                        exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
                    }
                }
#pragma warning restore CA1031
            }

            exceptionDispatchInfo?.Throw();
            throw new AssertException("Exception dispatch info is null.");
        }

        /// <summary>
        /// Get the service name of the specified instance.
        /// </summary>
        /// <param name="instance">The specified instance.</param>
        /// <returns>Returns the service name, or null if not found.</returns>
        protected string GetServiceWithInstanceObject(object instance)
        {
            return instancesReverse.TryGetValue(instance, out string origin)
                ? origin
                : null;
        }

        /// <summary>
        /// Verify that the current construct is valid.
        /// </summary>
        /// <param name="method">Called function name.</param>
        protected virtual void GuardConstruct(string method)
        {
        }

        /// <summary>
        /// Verify service name validity.
        /// </summary>
        /// <param name="service">The service name.</param>
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
        /// Verify function name validity.
        /// </summary>
        /// <param name="method">The method name.</param>
        protected virtual void GuardMethodName(string method)
        {
        }

        /// <summary>
        /// Build an empty bound data.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <returns>The bound data.</returns>
        protected virtual BindData MakeEmptyBindData(string service)
        {
            return new BindData(this, service, null, false);
        }

        /// <summary>
        /// Resolve the specified service(Will not perform <see cref="GuardConstruct"/> check).
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>The service instance.</returns>
        protected object Resolve(string service, params object[] userParams)
        {
            Guard.ParameterNotNull(service, nameof(service));

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

                    // We will start building a service instance，
                    // For the built service we will try to do dependency injection。
                    instance = Build(bindData, userParams);

                    // If we define an extender for the specified service, then we need
                    // to execute the expander in turn，And allow the extender to modify
                    // or overwrite the original service。
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
        /// Build the specified service.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for the <see cref="Make"/> service.</param>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>The service instance.</returns>
        protected virtual object Build(BindData makeServiceBindData, object[] userParams)
        {
            var instance = makeServiceBindData.Concrete != null
                ? makeServiceBindData.Concrete(this, userParams)
                : CreateInstance(makeServiceBindData, SpeculatedServiceType(makeServiceBindData.Service),
                    userParams);

            return Inject(makeServiceBindData, instance);
        }

        /// <summary>
        /// Create the specified service instance.
        /// </summary>
        /// <param name="makeServiceBindData">The bind data for the <see cref="Make"/> service.</param>
        /// <param name="makeServiceType">The type for the <see cref="Make"/> service.</param>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>The service instance.</returns>
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
#pragma warning disable CA1031
            catch (Exception ex)
            {
                throw MakeBuildFaildException(makeServiceBindData.Service, makeServiceType, ex);
            }
#pragma warning restore CA1031
        }

        /// <inheritdoc cref="CreateInstance(Bindable, Type, object[])"/>
        protected virtual object CreateInstance(Type makeServiceType, object[] userParams)
        {
            // If the parameter does not exist then you can get better
            // performance without writing parameters when reflecting.
            if (userParams == null || userParams.Length <= 0)
            {
                return Activator.CreateInstance(makeServiceType);
            }

            return Activator.CreateInstance(makeServiceType, userParams);
        }

        /// <summary>
        /// Get the service binding data, fill the data if the data is null.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <returns>The bind data for the service.</returns>
        protected BindData GetBindFillable(string service)
        {
            return service != null && bindings.TryGetValue(service, out BindData bindData)
                ? bindData
                : MakeEmptyBindData(service);
        }

        /// <summary>
        /// Guaranteed not to be flushing.
        /// </summary>
        private void GuardFlushing()
        {
            if (flushing)
            {
                throw new CodeStandardException("Container is flushing can not do it");
            }
        }

        /// <summary>
        /// Convert an alias to a service name.
        /// </summary>
        /// <param name="name">The service name or alias.</param>
        /// <returns>The service name.</returns>
        private string AliasToService(string name)
        {
            name = FormatService(name);
            return aliases.TryGetValue(name, out string alias) ? alias : name;
        }

        /// <summary>
        /// Trigger all of the resolving callbacks.
        /// </summary>
        /// <param name="bindData">The bind data for <see cref="Make"/> service.</param>
        /// <param name="instance">The service instance.</param>
        /// <returns>The decorated service instance.</returns>
        private object TriggerOnResolving(BindData bindData, object instance)
        {
            instance = bindData.TriggerResolving(instance);
            instance = Trigger(bindData, instance, resolving);
            return TriggerOnAfterResolving(bindData, instance);
        }

        /// <inheritdoc cref="TriggerOnResolving"/>
        /// <summary>
        /// Trigger all of the after resolving callbacks.
        /// </summary>
        private object TriggerOnAfterResolving(BindData bindData, object instance)
        {
            instance = bindData.TriggerAfterResolving(instance);
            return Trigger(bindData, instance, afterResloving);
        }

        /// <inheritdoc cref="TriggerOnResolving"/>
        /// <summary>
        /// Trigger all of the release callbacks.
        /// </summary>
        private void TriggerOnRelease(IBindData bindData, object instance)
        {
            Trigger(bindData, instance, release);
        }

        /// <summary>
        /// Trigger the rebound callbacks for specified service instance.
        /// </summary>
        /// <param name="service">The specified service name.</param>
        /// <param name="instance">
        /// The specified service instance.
        /// Build from the container by service name if a null value is passed in.
        /// </param>
        private void TriggerOnRebound(string service, object instance = null)
        {
            var callbacks = GetOnReboundCallbacks(service);
            if (callbacks == null || callbacks.Count <= 0)
            {
                return;
            }

            var bind = GetBind(service);
            instance = instance ?? Make(service);

            for (var index = 0; index < callbacks.Count; index++)
            {
                callbacks[index](instance);

                // If it is a not singleton(static) binding then each callback is given a separate instance.
                if (index + 1 < callbacks.Count && (bind == null || !bind.IsStatic))
                {
                    instance = Make(service);
                }
            }
        }

        /// <summary>
        /// Release the specified instance via <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="instance">The specified instance.</param>
        private void DisposeInstance(object instance)
        {
            if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// Gets the specified service all of the rebound callbacks.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <returns>The rebound callbacks list.</returns>
        private IList<Action<object>> GetOnReboundCallbacks(string service)
        {
            return !rebound.TryGetValue(service, out List<Action<object>> result) ? null : result;
        }

        /// <summary>
        /// Check if there is a callback for the rebound service.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <returns>True if the rebound callback exists. otherwise false.</returns>
        private bool HasOnReboundCallbacks(string service)
        {
            var result = GetOnReboundCallbacks(service);
            return result != null && result.Count > 0;
        }

        /// <summary>
        /// Trigger all of the extend callbacks.
        /// </summary>
        /// <param name="service">The service name.</param>
        /// <param name="instance">The service instance.</param>
        /// <returns>The decorated instance.</returns>
        private object Extend(string service, object instance)
        {
            if (extenders.TryGetValue(service, out List<Func<object, IContainer, object>> list))
            {
                foreach (var extender in list)
                {
                    instance = extender(instance, this);
                }
            }

            if (!extenders.TryGetValue(string.Empty, out list))
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
        /// Dependency injection for specified instance.
        /// </summary>
        /// <param name="bindable">The bindable for the instance.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>An instance of injection has been completed.</returns>
        private object Inject(Bindable bindable, object instance)
        {
            GuardResolveInstance(instance, bindable.Service);

            AttributeInject(bindable, instance);

            return instance;
        }

        /// <summary>
        /// Get the variable of type <see cref="IParams"/> from <paramref name="userParams"/>.
        /// </summary>
        /// <param name="userParams">An array for the user parameter.</param>
        /// <returns>An array of <see cref="IParams"/> parameters.</returns>
        private IParams[] GetParamsTypeInUserParams(ref object[] userParams)
        {
            // Filter is used here without using Remove because
            // the IParams is also one of the types that you might want to inject.
            var elements = Arr.Filter(userParams, value => value is IParams);
            var results = new IParams[elements.Length];
            for (var i = 0; i < elements.Length; i++)
            {
                results[i] = (IParams)elements[i];
            }

            return results;
        }

        /// <summary>
        /// Generate a default parameter <see cref="IParams" /> matcher.
        /// </summary>
        /// <param name="tables">An array of <see cref="IParams"/> parameters.</param>
        /// <returns>The default parameter matcher.</returns>
        private Func<ParameterInfo, object> MakeParamsMatcher(IParams[] tables)
        {
            // The default matcher policy will match the parameter name
            // with the parameter name of the parameter table.

            // The first valid valid parameter value will be returned
            // as the return value
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
        /// Register a new callback in specified list.
        /// </summary>
        /// <param name="closure">The callback.</param>
        /// <param name="list">The specified list.</param>
        private void AddClosure(Action<IBindData, object> closure, List<Action<IBindData, object>> list)
        {
            Guard.Requires<ArgumentNullException>(closure != null);

            lock (syncRoot)
            {
                GuardFlushing();
                list.Add(closure);
            }
        }
    }
}
