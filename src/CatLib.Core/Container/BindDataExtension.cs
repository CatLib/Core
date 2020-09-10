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

using CatLib.Container;
using CatLib.Util;
using System;

namespace CatLib
{
    /// <summary>
    /// An extension function for <see cref="BindData"/>.
    /// </summary>
    public static class BindDataExtension
    {
        /// <inheritdoc cref="IBindData.Alias"/>
        /// <typeparam name="TAlias">The type convert to alias name.</typeparam>
        public static IBindData Alias<TAlias>(this IBindData bindData)
        {
            return bindData.Alias(bindData.Container.Type2Service(typeof(TAlias)));
        }

        /// <inheritdoc cref="IBindData.OnResolving"/>
        public static IBindData OnResolving(this IBindData bindData, Action closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnResolving((_, instance) =>
            {
                closure();
            });
        }

        /// <inheritdoc cref="IBindData.OnResolving"/>
        public static IBindData OnResolving(this IBindData bindData, Action<object> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnResolving((_, instance) =>
            {
                closure(instance);
            });
        }

        /// <inheritdoc cref="IBindData.OnResolving"/>
        /// <typeparam name="T">The type of resolve instance.</typeparam>
        public static IBindData OnResolving<T>(this IBindData bindData, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnResolving((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <inheritdoc cref="OnResolving{T}(IBindData, Action{T})"/>
        public static IBindData OnResolving<T>(this IBindData bindData, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnResolving((bind, instance) =>
            {
                if (instance is T)
                {
                    closure(bind, (T)instance);
                }
            });
        }

        /// <inheritdoc cref="IBindData.OnAfterResolving"/>
        public static IBindData OnAfterResolving(this IBindData bindData, Action closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnAfterResolving((_, instance) =>
            {
                closure();
            });
        }

        /// <inheritdoc cref="IBindData.OnAfterResolving"/>
        public static IBindData OnAfterResolving(this IBindData bindData, Action<object> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnAfterResolving((_, instance) =>
            {
                closure(instance);
            });
        }

        /// <inheritdoc cref="IBindData.OnAfterResolving"/>
        /// <typeparam name="T">The type of resolve instance.</typeparam>
        public static IBindData OnAfterResolving<T>(this IBindData bindData, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnAfterResolving((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <inheritdoc cref="OnAfterResolving{T}(IBindData, Action{T})"/>
        public static IBindData OnAfterResolving<T>(this IBindData bindData, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnAfterResolving((bind, instance) =>
            {
                if (instance is T)
                {
                    closure(bind, (T)instance);
                }
            });
        }

        /// <inheritdoc cref="IBindData.OnRelease"/>
        public static IBindData OnRelease(this IBindData bindData, Action closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnRelease((_, __) =>
            {
                closure();
            });
        }

        /// <inheritdoc cref="IBindData.OnRelease"/>
        public static IBindData OnRelease(this IBindData bindData, Action<object> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnRelease((_, instance) =>
            {
                closure(instance);
            });
        }

        /// <inheritdoc cref="IBindData.OnRelease"/>
        /// <typeparam name="T">The type of release instance.</typeparam>
        public static IBindData OnRelease<T>(this IBindData bindData, Action<T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnRelease((_, instance) =>
            {
                if (instance is T)
                {
                    closure((T)instance);
                }
            });
        }

        /// <inheritdoc cref="OnRelease{T}(IBindData, Action{T})"/>
        public static IBindData OnRelease<T>(this IBindData bindData, Action<IBindData, T> closure)
        {
            Guard.Requires<ArgumentNullException>(closure != null);
            return bindData.OnRelease((bind, instance) =>
            {
                if (instance is T)
                {
                    closure(bind, (T)instance);
                }
            });
        }
    }
}
