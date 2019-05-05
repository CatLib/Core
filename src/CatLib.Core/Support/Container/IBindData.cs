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
    /// The bind data indicates relational data related to the specified service.
    /// </summary>
    public interface IBindData : IBindable<IBindData>
    {
        /// <summary>
        /// The delegate return service concrete.
        /// </summary>
        Func<IContainer, object[], object> Concrete { get; }

        /// <summary>
        /// True if the service is singleton(static).
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// Alias service to a different name.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns>The current instance.</returns>
        IBindData Alias(string alias);

        /// <summary>
        /// Assign a tag to a given service.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <returns>The current instance.</returns>
        IBindData Tag(string tag);

        /// <summary>
        /// Register a new resolving callback.
        /// </summary>
        /// <param name="closure">The resolving callback.</param>
        /// <returns>The current instance.</returns>
        IBindData OnResolving(Action<IBindData, object> closure);

        /// <summary>
        /// Register a new after resolving callback.
        /// </summary>
        /// <param name="closure">The after resolving callback.</param>
        /// <returns>The current instance.</returns>
        IBindData OnAfterResolving(Action<IBindData, object> closure);

        /// <summary>
        /// Register a new release callback.
        /// </summary>
        /// <param name="closure">The release callback.</param>
        /// <returns>The current instance.</returns>
        IBindData OnRelease(Action<IBindData, object> closure);
    }
}
