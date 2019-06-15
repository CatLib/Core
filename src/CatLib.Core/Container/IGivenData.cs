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

namespace CatLib.Container
{
    /// <summary>
    /// Indicates the given relationship in the context.
    /// </summary>
    /// <typeparam name="TReturn">The type of the <see cref="IBindable"/>.</typeparam>
    public interface IGivenData<TReturn>
        where TReturn : IBindable
    {
        /// <summary>
        /// Give the specified service.
        /// </summary>
        /// <param name="service">The service name or alias.</param>
        /// <returns>The instance of the <see cref="IBindData"/>.</returns>
        TReturn Given(string service);

        /// <inheritdoc cref="Given(string)"/>
        TReturn Given<TService>();

        /// <inheritdoc cref="Given(string)"/>
        /// <param name="closure">The closure returns the given instance.</param>
        TReturn Given(Func<object> closure);
    }
}
