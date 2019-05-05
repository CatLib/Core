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

namespace CatLib
{
    /// <summary>
    /// All parameter tables must implement this interface.
    /// </summary>
    public interface IParams
    {
        /// <summary>
        /// Get a parameter.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>True if the parameter exist.</returns>
        bool TryGetValue(string key, out object value);
    }
}
