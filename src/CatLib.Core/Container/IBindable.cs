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

#pragma warning disable SA1402

namespace CatLib.Container
{
    /// <summary>
    /// <see cref="IBindable"/> is the interface implemented by all bindable data classes.
    /// </summary>
    public interface IBindable
    {
        /// <summary>
        /// Gets the service name.
        /// </summary>
        string Service { get; }

        /// <summary>
        /// Gets the container to which the service belongs.
        /// </summary>
        IContainer Container { get; }

        /// <summary>
        /// Unbind the service from the container.
        /// </summary>
        /// <remarks>
        /// If the service is a singletoned instance, then the singleton instance
        /// that has been built will be automatically released.
        /// </remarks>
        void Unbind();
    }

    /// <inheritdoc />
    public interface IBindable<TReturn> : IBindable
        where TReturn : IBindable
    {
        /// <summary>
        /// When the service specified by the demand.
        /// </summary>
        /// <param name="service">The specified service name.</param>
        /// <returns>The given relationship in the context.</returns>
        IGivenData<TReturn> Needs(string service);

        /// <inheritdoc cref="Needs(string)"/>
        /// <typeparam name="TService">The type convert to service name.</typeparam>
        IGivenData<TReturn> Needs<TService>();
    }
}
