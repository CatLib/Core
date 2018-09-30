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
    /// 管理器模版 - 不可被外部访问到的拓展
    /// </summary>
    /// <typeparam name="TInterface">拓展接口</typeparam>
    public abstract class Managed<TInterface> : IManaged<TInterface>
    {
        /// <summary>
        /// 扩展解决器
        /// </summary>
        private readonly Dictionary<string, Func<TInterface>> extendBuilder;

        /// <summary>
        /// 当扩展被实现时
        /// </summary>
        public event Action<TInterface> OnResolving;

        /// <summary>
        /// 构建一个新的管理器模板
        /// </summary>
        protected Managed()
        {
            extendBuilder = new Dictionary<string, Func<TInterface>>();
        }

        /// <summary>
        /// 自定义一个扩展构建器
        /// </summary>
        /// <param name="builder">扩展构建器</param>
        /// <param name="name">扩展名</param>
        public void Extend(Func<TInterface> builder, string name = null)
        {
            Guard.Requires<ArgumentNullException>(builder != null);

            StandardName(ref name);

            if (extendBuilder.ContainsKey(name))
            {
                throw new RuntimeException("Extend [" + name + "](" + GetType() + ") is already exists.");
            }

            extendBuilder.Add(name, builder);
        }

        /// <summary>
        /// 释放指定扩展的构建器
        /// </summary>
        /// <param name="name">扩展名</param>
        [Obsolete("Please use RemoveExtend();")]
        public void ReleaseExtend(string name = null)
        {
            RemoveExtend(name);
        }

        /// <summary>
        /// 释放指定扩展的构建器
        /// </summary>
        /// <param name="name">扩展名</param>
        public void RemoveExtend(string name = null)
        {
            StandardName(ref name);

            if (!extendBuilder.ContainsKey(name))
            {
                return;
            }

            extendBuilder.Remove(name);
        }

        /// <summary>
        /// 是否包含指定扩展构建器
        /// </summary>
        /// <param name="name">扩展名</param>
        public bool ContainsExtend(string name = null)
        {
            StandardName(ref name);
            return extendBuilder.ContainsKey(name);
        }

        /// <summary>
        /// 使用指定的扩展构建器生成扩展实现
        /// </summary>
        /// <param name="name">扩展名</param>
        /// <returns>扩展实现</returns>
        protected virtual TInterface MakeExtend(string name)
        {
            var extend = GetExtend(name)();

            if (OnResolving != null)
            {
                OnResolving(extend);
            }

            return extend;
        }

        /// <summary>
        /// 获取默认的扩展名
        /// </summary>
        /// <returns>默认的扩展名</returns>
        protected virtual string GetDefaultName()
        {
            return "default";
        }

        /// <summary>
        /// 标准化扩展名
        /// </summary>
        /// <param name="name">扩展名</param>
        protected void StandardName(ref string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = GetDefaultName();
            }
        }

        /// <summary>
        /// 获取扩展的构建闭包
        /// </summary>
        /// <param name="name">名字</param>
        /// <returns>拓展</returns>
        private Func<TInterface> GetExtend(string name)
        {
            StandardName(ref name);

            Func<TInterface> result;
            if (!extendBuilder.TryGetValue(name, out result))
            {
                throw new RuntimeException("Can not find [" + name + "](" + GetType() + ") Extend.");
            }

            return result;
        }
    }
}
