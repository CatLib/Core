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
    /// <see cref="IServiceProvider"/> is the interface implemented by all service provider classes.
    /// </summary>
    public interface IServiceProvider
    {
        /// <summary>
        /// Initialize the application's service providers.
        /// </summary>
        void Init();

        /// <summary>
        /// Register any application services.
        /// </summary>
        void Register();
    }
}
