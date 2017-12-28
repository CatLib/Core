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

using System.Collections.Generic;

namespace CatLib
{
    /// <summary>
    /// 参数名注入表
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class Params : IEnumerable<KeyValuePair<string, object>>
    {
        /// <summary>
        /// 参数表
        /// </summary>
        private readonly Dictionary<string, object> table = new Dictionary<string, object>();

        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns>迭代器</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return table.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return table.GetEnumerator();
        }

        /// <summary>
        /// 获取或者设定一个参数
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public object this[string key]
        {
            get
            {
                return table[key];
            }
            set
            {
                table[key] = value;
            }
        }

        /// <summary>
        /// 增加一个参数
        /// </summary>
        /// <param name="key">参数名</param>
        /// <param name="value">参数值</param>
        public void Add(string key, object value)
        {
            table.Add(key, value);
        }

        /// <summary>
        /// 移除参数
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return table.Remove(key);
        }

        /// <summary>
        /// 获取一个参数
        /// </summary>
        /// <param name="key">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetValue(string key, out object value)
        {
            return table.TryGetValue(key, out value);
        }
    }
}
