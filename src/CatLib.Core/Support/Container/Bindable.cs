/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
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
    /// 可绑定对象
    /// </summary>
    public abstract class Bindable : IBindable
    {
        /// <summary>
        /// 当前绑定的名字
        /// </summary>
        public string Service { get; }

        /// <summary>
        /// 父级容器
        /// </summary>
        protected readonly Container Container;

        /// <summary>
        /// 服务关系上下文
        /// 当前服务需求某个服务时可以指定给与什么服务
        /// </summary>
        private Dictionary<string, string> contextual;

        /// <summary>
        /// 服务上下文闭包
        /// 当前服务需求某个服务时给定的闭包
        /// </summary>
        private Dictionary<string, Func<object>> contextualClosure;

        /// <summary>
        /// 同步锁
        /// </summary>
        protected readonly object SyncRoot = new object();

        /// <summary>
        /// 是否被释放
        /// </summary>
        private bool isDestroy;

        /// <summary>
        /// 构建一个绑定数据
        /// </summary>
        /// <param name="container">依赖注入容器</param>
        /// <param name="service">服务名</param>
        protected Bindable(Container container, string service)
        {
            Container = container;
            Service = service;
            isDestroy = false;
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        public void Unbind()
        {
            lock (SyncRoot)
            {
                isDestroy = true;
                ReleaseBind();
            }
        }

        /// <summary>
        /// 为服务增加上下文
        /// </summary>
        /// <param name="needs">需求什么服务</param>
        /// <param name="given">给与什么服务</param>
        /// <returns>服务绑定数据</returns>
        internal void AddContextual(string needs, string given)
        {
            lock (SyncRoot)
            {
                GuardIsDestroy();
                if (contextual == null)
                {
                    contextual = new Dictionary<string, string>();
                }
                if (contextual.ContainsKey(needs)
                    || (contextualClosure != null && contextualClosure.ContainsKey(needs)))
                {
                    throw new LogicException($"Needs [{needs}] is already exist.");
                }
                contextual.Add(needs, given);
            }
        }

        /// <summary>
        /// 为服务增加上下文
        /// </summary>
        /// <param name="needs">需求什么服务</param>
        /// <param name="given">给与什么服务</param>
        internal void AddContextual(string needs, Func<object> given)
        {
            lock (SyncRoot)
            {
                GuardIsDestroy();
                if (contextualClosure == null)
                {
                    contextualClosure = new Dictionary<string, Func<object>>();
                }
                if (contextualClosure.ContainsKey(needs)
                    || (contextual != null && contextual.ContainsKey(needs)))
                {
                    throw new LogicException($"Needs [{needs}] is already exist.");
                }
                contextualClosure.Add(needs, given);
            }
        }

        /// <summary>
        /// 获取上下文的需求关系
        /// </summary>
        /// <param name="needs">需求的服务</param>
        /// <returns>给与的服务</returns>
        internal string GetContextual(string needs)
        {
            if (contextual == null)
            {
                return needs;
            }
            return contextual.TryGetValue(needs, out string contextualNeeds) ? contextualNeeds : needs;
        }

        /// <summary>
        /// 获取上下文关系闭包实现
        /// </summary>
        /// <param name="needs">需求的服务</param>
        /// <returns>给与的闭包</returns>
        internal Func<object> GetContextualClosure(string needs)
        {
            if (contextualClosure == null)
            {
                return null;
            }
            return contextualClosure.TryGetValue(needs, out Func<object> closure) ? closure : null;
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        protected abstract void ReleaseBind();

        /// <summary>
        /// 守卫是否被释放
        /// </summary>
        protected void GuardIsDestroy()
        {
            if (isDestroy)
            {
                throw new LogicException("Current bind has be mark Destroy.");
            }
        }
    }

    /// <summary>
    /// 可绑定对象
    /// </summary>
    internal abstract class Bindable<TReturn> : Bindable, IBindable<TReturn> where TReturn : class, IBindable<TReturn>
    {
        /// <summary>
        /// 给与数据
        /// </summary>
        private GivenData<TReturn> given;

        /// <summary>
        /// 构建一个绑定数据
        /// </summary>
        /// <param name="container">依赖注入容器</param>
        /// <param name="service">服务名</param>
        protected Bindable(Container container, string service)
            : base(container, service)
        {
        }

        /// <summary>
        /// 当需求某个服务                                                                                                                                                                                                                                                                                                                                                                                  
        /// </summary>
        /// <param name="service">服务名</param>
        /// <returns>绑定关系临时数据</returns>
        public IGivenData<TReturn> Needs(string service)
        {
            Guard.NotEmptyOrNull(service, nameof(service));
            lock (SyncRoot)
            {
                GuardIsDestroy();
                if (given == null)
                {
                    given = new GivenData<TReturn>(Container, this);
                }
                given.Needs(service);
            }
            return given;
        }

        /// <summary>
        /// 当需求某个服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>绑定关系临时数据</returns>
        public IGivenData<TReturn> Needs<T>()
        {
            return Needs(Container.Type2Service(typeof(T)));
        }
    }
}
