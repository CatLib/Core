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

namespace CatLib
{
    /// <summary>
    /// 服务绑定数据
    /// </summary>    
    public sealed class BindData : Bindable<IBindData>, IBindData
    {
        /// <summary>
        /// 服务实现，执行这个委托将会获得服务实例
        /// </summary>
        public Func<IContainer, object[], object> Concrete { get; }

        /// <summary>
        /// 当前绑定的服务是否是静态服务
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// 服务构造修饰器
        /// </summary>
        private List<Action<IBindData, object>> resolving;

        /// <summary>
        /// 在服务构建修饰器之后的修饰器
        /// </summary>
        private List<Action<IBindData, object>> afterResolving;

        /// <summary>
        /// 服务构造修饰器
        /// </summary>
        private List<Action<IBindData, object>> release;

        /// <summary>
        /// 构建一个绑定数据
        /// </summary>
        /// <param name="container">服务父级容器</param>
        /// <param name="service">服务名</param>
        /// <param name="concrete">服务实现</param>
        /// <param name="isStatic">服务是否是静态的</param>
        public BindData(Container container, string service, Func<IContainer, object[], object> concrete, bool isStatic)
            : base(container, service)
        {
            Concrete = concrete;
            IsStatic = isStatic;
        }

        /// <summary>
        /// 为服务设定一个别名
        /// </summary>
        /// <typeparam name="T">别名</typeparam>
        /// <returns>服务绑定数据</returns>
        public IBindData Alias<T>()
        {
            return Alias(InternalContainer.Type2Service(typeof(T)));
        }

        /// <summary>
        /// 为服务设定一个别名
        /// </summary>
        /// <param name="alias">别名</param>
        /// <returns>服务绑定数据</returns>
        public IBindData Alias(string alias)
        {
            lock (SyncRoot)
            {
                GuardIsDestroy();
                Guard.NotEmptyOrNull(alias, nameof(alias));
                InternalContainer.Alias(alias, Service);
                return this;
            }
        }

        /// <summary>
        /// 为服务打上一个标签
        /// </summary>
        /// <param name="tag">标签名</param>
        /// <returns>服务绑定数据</returns>
        public IBindData Tag(string tag)
        {
            lock (SyncRoot)
            {
                GuardIsDestroy();
                Guard.NotEmptyOrNull(tag, nameof(tag));
                InternalContainer.Tag(tag, Service);
                return this;
            }
        }

        /// <summary>
        /// 解决服务时触发的回调
        /// </summary>
        /// <param name="closure">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public IBindData OnResolving(Action<IBindData, object> closure)
        {
            AddClosure(closure, ref resolving);
            return this;
        }

        /// <summary>
        /// 解决服务时事件之后的回调
        /// </summary>
        /// <param name="closure">解决事件</param>
        /// <returns>服务绑定数据</returns>
        public IBindData OnAfterResolving(Action<IBindData, object> closure)
        {
            AddClosure(closure, ref afterResolving);
            return this;
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="closure">处理事件</param>
        /// <returns>服务绑定数据</returns>
        public IBindData OnRelease(Action<IBindData, object> closure)
        {
            if (!IsStatic)
            {
                throw new LogicException(
                    $"Service [{Service}] is not Singleton(Static) Bind , Can not call {nameof(OnRelease)}().");
            }

            AddClosure(closure, ref release);
            return this;
        }

        /// <summary>
        /// 移除绑定服务 , 在解除绑定时如果是静态化物体将会触发释放
        /// </summary>
        protected override void ReleaseBind()
        {
            InternalContainer.Unbind(this);
        }

        /// <summary>
        /// 执行服务修饰器
        /// </summary>
        /// <param name="instance">服务实例</param>
        /// <returns>服务实例</returns>
        internal object TriggerResolving(object instance)
        {
            return InternalContainer.Trigger(this, instance, resolving);
        }

        /// <summary>
        /// 执行服务修饰器之后的回调
        /// </summary>
        /// <param name="instance">服务实例</param>
        /// <returns>服务实例</returns>
        internal object TriggerAfterResolving(object instance)
        {
            return InternalContainer.Trigger(this, instance, afterResolving);
        }

        /// <summary>
        /// 执行服务释放处理器
        /// </summary>
        /// <param name="instance">服务实例</param>
        /// <returns>服务实例</returns>
        internal object TriggerRelease(object instance)
        {
            return InternalContainer.Trigger(this, instance, release);
        }

        /// <summary>
        /// 增加一个事件
        /// </summary>
        /// <param name="closure">闭包</param>
        /// <param name="list">事件列表</param>
        private void AddClosure(Action<IBindData, object> closure, ref List<Action<IBindData, object>> list)
        {
            Guard.NotNull(closure, nameof(closure));

            lock (SyncRoot)
            {
                GuardIsDestroy();

                if (list == null)
                {
                    list = new List<Action<IBindData, object>>();
                }

                list.Add(closure);
            }
        }
    }
}