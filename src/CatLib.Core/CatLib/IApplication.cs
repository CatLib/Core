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
using CatLib.Container;

namespace CatLib
{
    /// <summary>
    /// <see cref="IApplication"/> is the interface implemented by all application classes.
    /// </summary>
    public interface IApplication : IContainer
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
        /// Register a new boot listener.
        /// </summary>
        void Booting(Action<IApplication> callback);

        /// <summary>
        /// Register a new "booted" listener.
        /// </summary>
        void Booted(Action<IApplication> callback);

        /// <summary>
        /// Register a terminating callback with the application.
        /// </summary>
        void Terminating(Action<IApplication> callback);

        /// <summary>
        /// Gets the unique runtime id.
        /// </summary>
        /// <returns>The unique runtime id.</returns>
        long GetRuntimeId();

        /// <summary>
        /// Terminates the <see cref="IApplication"/>.
        /// </summary>
        void Terminate();
    }
}
