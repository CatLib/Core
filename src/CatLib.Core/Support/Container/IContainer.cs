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
using System.Reflection;

namespace CatLib
{
    /// <summary>
    /// 依赖注入容器
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// 获取服务的绑定数据,如果绑定不存在则返回null
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>服务绑定数据或者null</returns>
        IBindData GetBind(string service);

        /// <summary>
        /// 是否已经绑定了服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>返回一个bool值代表服务是否被绑定</returns>
        bool HasBind(string service);

        /// <summary>
        /// 是否可以生成服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>是否可以生成服务</returns>
        bool CanMake(string service);

        /// <summary>
        /// 服务是否是静态化的,如果服务不存在也将返回false
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>是否是静态化的</returns>
        bool IsStatic(string service);

        /// <summary>
        /// 是否是别名
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>是否是别名</returns>
        bool IsAlias(string name);

        /// <summary>
        /// 绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实体</param>
        /// <param name="isStatic">服务是否静态化</param>
        /// <returns>服务绑定数据</returns>
        bool Bind(string service, Func<IContainer, object[], object> concrete, bool isStatic, out IBindData bindData);

        /// <summary>
        /// 绑定一个服务
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否静态化</param>
        /// <returns>服务绑定数据</returns>
        bool Bind(string service, Type concrete, bool isStatic, out IBindData bindData);

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// <para>如果返回值为null说明没有绑定服务</para>
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        /// <returns>服务绑定数据</returns>
        IBindData BindIf(string service, Func<IContainer, object[], object> concrete, bool isStatic);

        /// <summary>
        /// 如果服务不存在那么则绑定服务
        /// <para>如果返回值为null说明没有绑定服务</para>
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        /// <returns>服务绑定数据</returns>
        IBindData BindIf(string service, Type concrete, bool isStatic);

        /// <summary>
        /// 为一个及以上的服务定义一个标记
        /// </summary>
        /// <param name="tag">标记名</param>
        /// <param name="service">服务名</param>
        void Tag(string tag, params string[] service);

        /// <summary>
        /// 根据标记名生成标记所对应的所有服务实例
        /// </summary>
        /// <param name="tag">标记名</param>
        /// <returns>将会返回标记所对应的所有服务实例</returns>
        object[] Tagged(string tag);

        /// <summary>
        /// 静态化一个服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <param name="instance">服务实例</param>
        void Instance(string service, object instance);

        /// <summary>
        /// 释放某个静态化实例
        /// </summary>
        /// <param name="service">服务名或别名</param>
        void Release(string service);

        /// <summary>
        /// 清空容器的所有实例，绑定，别名，标签，解决器
        /// </summary>
        void Flush();

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="action">处理释放时的回调</param>
        IContainer OnRelease(Action<IBindData, object> action);

        /// <summary>
        /// 以依赖注入形式调用一个方法
        /// </summary>
        /// <param name="instance">方法对象</param>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>方法返回值</returns>
        object Call(object instance, MethodInfo methodInfo, params object[] userParams);

        /// <summary>
        /// 构造服务,允许传入参数来决定构造函数的值
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <param name="userParams">用户传入的参数</param>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        object MakeWith(string service, params object[] userParams);

        /// <summary>
        /// 构造服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
        object Make(string service);

        /// <summary>
        /// 构造服务
        /// </summary>
        /// <param name="service">服务名或者别名</param>
        /// <returns>服务实例，如果构造失败那么返回null</returns>
		object this[string service] { get; }

        /// <summary>
        /// 获取一个回调，当执行回调可以生成指定的服务
        /// </summary>
        /// <param name="service">服务名或别名</param>
        /// <returns>回调方案</returns>
        Func<object> Factory(string service);

        /// <summary>
        /// 为服务设定一个别名
        /// </summary>
        /// <param name="alias">别名</param>
        /// <param name="service">映射到的服务名</param>
        /// <returns>当前容器对象</returns>
        IContainer Alias(string alias, string service);

        /// <summary>
        /// 当服务被解决时触发的事件
        /// </summary>
        /// <param name="func">回调函数</param>
        /// <returns>当前容器实例</returns>
        IContainer OnResolving(Func<IBindData, object, object> func);

        /// <summary>
        /// 当查找类型无法找到时会尝试去调用开发者提供的查找类型函数
        /// </summary>
        /// <param name="func">查找类型的回调</param>
        /// <param name="priority">查询优先级(值越小越优先)</param>
        /// <returns>当前容器实例</returns>
        IContainer OnFindType(Func<string, Type> func, int priority = int.MaxValue);

        /// <summary>
        /// 类型转为服务名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>转换后的服务名</returns>
        string Type2Service(Type type);
    }
}