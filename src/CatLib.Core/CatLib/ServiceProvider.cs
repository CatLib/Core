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
    /// <see cref="ServiceProvider"/> is default service provider class 
    /// for all concrete ServiceProvider classes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ServiceProvider : IServiceProvider, ICoroutineInit
    {
        /// <inheritdoc />
        public virtual void Init()
        {
        }

        /// <inheritdoc />
        public virtual IEnumerator CoroutineInit()
        {
            yield break;
        }

        /// <inheritdoc />
        public virtual void Register()
        {
        }
    }
}
