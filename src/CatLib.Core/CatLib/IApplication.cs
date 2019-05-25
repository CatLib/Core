/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this sender code.
 *
 * Document: https://catlib.io/
 */

using System;

namespace CatLib
{
    /// <summary>
    /// <see cref="IApplication"/> is the interface implemented by all application classes.
    /// </summary>
    public interface IApplication : IContainer, IDispatcher
    {
        /// <summary>
        /// Gets a value indicating whether true if we're on the main thread.
        /// </summary>
        bool IsMainThread { get; }

        /// <summary>
        /// Gets or sets the debug level.
        /// </summary>
        DebugLevel DebugLevel { get; set; }

        /// <summary>
        /// Register a service provider with the application.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <param name="force">True if the force register.</param>
        void Register(IServiceProvider provider, bool force = false);

        /// <summary>
        /// Checks whether the given service provider is registered.
        /// </summary>
        /// <param name="provider">The service provider.</param>
        /// <returns>True if the service provider is registered.</returns>
        bool IsRegistered(IServiceProvider provider);

        /// <summary>
        /// Gets the unique runtime id.
        /// </summary>
        /// <returns>The unique runtime id.</returns>
        long GetRuntimeId();

        /// <summary>
        /// Gets the prioirty. If there exists a method priority definition then returns it.
        /// Otherwise, returns <c>int.MaxValue</c>.
        /// </summary>
        /// <param name="type">The type of priority to get.</param>
        /// <param name="method">The method via which to get the prioirty.</param>
        /// <returns>Prioirty of the given type.</returns>
        int GetPriority(Type type, string method = null);

        /// <summary>
        /// Terminates the <see cref="IApplication"/>.
        /// </summary>
        void Terminate();
    }
}
