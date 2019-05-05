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

using System.Collections;

namespace CatLib
{
    /// <summary>
    /// Allow coroutine to be used during the <see cref="Application.Init"/> process.
    /// </summary>
    public interface ICoroutineInit
    {
        /// <summary>
        /// Initialize the service provider with a coroutine.
        /// </summary>
        /// <remarks>The next coroutine will be executed only after the current coroutine is completed.</remarks>
        /// <returns>The iterator object.</returns>
        IEnumerator CoroutineInit();
    }
}
