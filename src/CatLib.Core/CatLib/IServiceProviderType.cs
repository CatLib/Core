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

using System;

namespace CatLib
{
    /// <summary>
    /// Allow current service providers to be treated as specified service providers.
    /// </summary>
    public interface IServiceProviderType
    {
        /// <summary>
        /// The Type To be treated as specified service providers.
        /// </summary>
        Type BaseType { get; }
    }
}
