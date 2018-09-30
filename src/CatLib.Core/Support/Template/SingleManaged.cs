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
    /// 管理器模版（拓展解决方案为单例）- 扩展内容不对外可见
    /// </summary>
    public abstract class SingleManaged<TInterface> : Managed<TInterface>, ISingleManaged<TInterface>, IDisposable
    {
        /// <summary>
        /// 扩展实现
        /// </summary>
        private readonly Dictionary<string, TInterface> instances;

        /// <summary>
        /// 当释放时
        /// </summary>
        public event Action<TInterface> OnRelease;

        /// <summary>
        /// 构建一个新的管理器模版
        /// </summary>
        protected SingleManaged()
        {
            instances = new Dictionary<string, TInterface>();
        }

        /// <summary>
        /// 释放指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        public void Release(string name = null)
        {
            StandardName(ref name);

            TInterface extend;
            if (!instances.TryGetValue(name, out extend))
            {
                return;
            }

            InternalRelease(extend);
            instances.Remove(name);
        }

        /// <summary>
        /// 是否包含指定的扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>是否包含扩展实现</returns>
        public bool Contains(string name = null)
        {
            StandardName(ref name);
            return instances.ContainsKey(name);
        }

        /// <summary>
        /// 释放管理器时
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var instance in instances)
            {
                InternalRelease(instance.Value);
            }
            instances.Clear();
        }

        /// <summary>
        /// 释放扩展
        /// </summary>
        /// <param name="extend">扩展实现</param>
        private void InternalRelease(TInterface extend)
        {
            if (OnRelease != null)
            {
                OnRelease(extend);
            }

            var dispose = extend as IDisposable;
            if (dispose != null)
            {
                dispose.Dispose();
            }
        }

        /// <summary>
        /// 生成扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>扩展实现</returns>
        protected override TInterface MakeExtend(string name)
        {
            StandardName(ref name);

            TInterface extend;
            if (instances.TryGetValue(name, out extend))
            {
                return extend;
            }

            return instances[name] = base.MakeExtend(name);
        }
    }
}
