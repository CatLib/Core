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

using System.Collections.Generic;

namespace CatLib
{
    /// <summary>
    /// Default parameter table implementation.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class Params : IParams, IEnumerable<KeyValuePair<string, object>>
    {
        /// <summary>
        /// The parameter mapping.
        /// </summary>
        private readonly IDictionary<string, object> table;

        /// <summary>
        /// Gets the iterator.
        /// </summary>
        /// <returns>Return the iterator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return table.GetEnumerator();
        }

        /// <summary>
        /// Gets the iterator.
        /// </summary>
        /// <returns>Return the iterator.</returns>
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return table.GetEnumerator();
        }

        /// <summary>
        /// Initialize a new <see cref="Params"/> instance.
        /// </summary>
        public Params()
        {
            table = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initialize a new <see cref="Params"/> instance.
        /// </summary>
        /// <param name="args">The parameters mapping.</param>
        public Params(IDictionary<string, object> args)
        {
            table = args;
        }

        /// <summary>
        /// Get or set a parameter.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <returns>The parameter value.</returns>
        public object this[string key]
        {
            get => table[key];
            set => table[key] = value;
        }

        /// <summary>
        /// Add an parameter.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        public void Add(string key, object value)
        {
            table.Add(key, value);
        }

        /// <summary>
        /// Remove an parameter.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <returns>True if the removed.</returns>
        public bool Remove(string key)
        {
            return table.Remove(key);
        }

        /// <summary>
        /// Gets an parameter value.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>True if the parameter is exist.</returns>
        public bool TryGetValue(string key, out object value)
        {
            return table.TryGetValue(key, out value);
        }
    }
}
