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

namespace CatLib
{
    /// <summary>
    /// 服务绑定数据
    /// </summary>    
    internal sealed class BindData : Bindable<IBindData>, IBindData
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
            return Alias(Container.Type2Service(typeof(T)));
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
                Container.Alias(alias, Service);
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
                Container.Tag(tag, Service);
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
            Guard.NotNull(closure, nameof(closure));
            lock (SyncRoot)
            {
                GuardIsDestroy();

                if (resolving == null)
                {
                    resolving = new List<Action<IBindData, object>>();
                }

                resolving.Add(closure);
            }
            return this;
        }

        /// <summary>
        /// 当静态服务被释放时
        /// </summary>
        /// <param name="closure">处理事件</param>
        /// <returns>服务绑定数据</returns>
        public IBindData OnRelease(Action<IBindData, object> closure)
        {
            Guard.NotNull(closure, nameof(closure));
            if (!IsStatic)
            {
                throw new RuntimeException($"Service [{Service}] is not Singleton(Static) Bind , Can not call {nameof(OnRelease)}().");
            }
            lock (SyncRoot)
            {
                GuardIsDestroy();

                if (release == null)
                {
                    release = new List<Action<IBindData, object>>();
                }

                release.Add(closure);
            }
            return this;
        }

        /// <summary>
        /// 移除绑定服务 , 在解除绑定时如果是静态化物体将会触发释放
        /// </summary>
        protected override void ReleaseBind()
        {
            Container.Unbind(this);
        }

        /// <summary>
        /// 执行服务修饰器
        /// </summary>
        /// <param name="instance">服务实例</param>
        /// <returns>服务实例</returns>
        internal object TriggerResolving(object instance)
        {
            return Container.Trigger(this, instance, resolving);
        }

        /// <summary>
        /// 执行服务释放处理器
        /// </summary>
        /// <param name="instance">服务实例</param>
        /// <returns>服务实例</returns>
        internal object TriggerRelease(object instance)
        {
            return Container.Trigger(this, instance, release);
        }
    }
}