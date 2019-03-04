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
    /// <see cref="IBootstrap"/> is the interface implemented by all bootstrap classes.
    /// </summary>
    public interface IBootstrap
    {
        /// <summary>
        /// Bootstrap the framework.
        /// </summary>
        void Bootstrap();
    }
}
