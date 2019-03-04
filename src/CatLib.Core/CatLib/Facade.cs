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
    /// <see cref="Facade{TService}"/> is the abstract implemented by all facade classes.
    /// </summary>
    /// <remarks>
    /// <code>public class FileSystem : Facade&gt;IFileSystem&lt;{ }</code>
    /// </remarks>
    public abstract class Facade<TService>
    {
        /// <summary>
        /// The resolved object instance.
        /// </summary>
        private static TService instance;

        /// <summary>
        /// The resolved object bind data.
        /// </summary>
        private static IBindData binder;

        /// <summary>
        /// Whether the facade has been initialized.
        /// </summary>
        private static bool inited;

        /// <summary>
        /// The service name.
        /// </summary>
        private static string service;

        /// <summary>
        /// Whether the resolved object has been released.
        /// </summary>
        private static bool released;

        /// <summary>
        /// The facade static constructor .
        /// </summary>
        static Facade()
        {
            service = App.Type2Service(typeof(TService));
            App.OnNewApplication += app =>
            {
                instance = default(TService);
                binder = null;
                inited = false;
                released = false;
            };
        }

        /// <inheritdoc cref="instance"/>
        public static TService Instance => HasInstance ? instance : Resolve();

        /// <summary>
        /// Whether the resolved instance is exists in the facade.
        /// <para>If it is a non-static binding then return forever <code>false</code></para>
        /// </summary>
        internal static bool HasInstance => binder != null && binder.IsStatic && !released && instance != null;

        /// <summary>
        /// Resolve the object instance.
        /// </summary>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The resolved object.</returns>
        internal static TService Make(params object[] userParams)
        {
            return HasInstance ? instance : Resolve(userParams);
        }

        /// <inheritdoc cref="Make"/>
        private static TService Resolve(params object[] userParams)
        {
            released = false;

            if (!inited && (App.IsResolved(service) || App.CanMake(service)))
            {
                App.Watch<TService>(ServiceRebound);
                inited = true;
            }
            else if (binder != null && !binder.IsStatic)
            {
                // If it has been initialized, the binder has been initialized.
                // Then judging in advance can optimize performance without 
                // going through a hash lookup.
                return Build(userParams);
            }

            var newBinder = App.GetBind(service);
            if (newBinder == null || !newBinder.IsStatic)
            {
                binder = newBinder;
                return Build(userParams);
            }

            Rebind(newBinder);
            return instance = Build(userParams);
        }

        /// <summary>
        /// When the resolved object is released
        /// </summary>
        /// <param name="oldBinder">The old bind data with resolved object.</param>
        /// <param name="_">The ignored parameter.</param>
        private static void OnRelease(IBindData oldBinder, object _)
        {
            if (oldBinder != binder)
            {
                return;
            }

            instance = default(TService);
            released = true;
        }

        /// <summary>
        /// When the resolved object is rebound.
        /// </summary>
        /// <param name="newService">The new resolved object.</param>
        private static void ServiceRebound(TService newService)
        {
            var newBinder = App.GetBind(service);
            Rebind(newBinder);
            instance = (newBinder == null || !newBinder.IsStatic) ? default(TService) : newService;
        }

        /// <summary>
        /// Rebinding the bound data to given binder. 
        /// </summary>
        /// <param name="newBinder">The new binder.</param>
        private static void Rebind(IBindData newBinder)
        {
            if (newBinder != null && binder != newBinder && newBinder.IsStatic)
            {
                newBinder.OnRelease(OnRelease);
            }

            binder = newBinder;
        }

        /// <summary>
        /// Resolve facade object from the container.
        /// </summary>
        /// <param name="userParams">The user parameters.</param>
        /// <returns>The resolved object.</returns>
        private static TService Build(params object[] userParams)
        {
            return (TService)App.Make(service, userParams);
        }
    }
}