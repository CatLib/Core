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

#pragma warning disable CA1000
#pragma warning disable S3963
#pragma warning disable S2743
#pragma warning disable S1118

namespace CatLib
{
    /// <summary>
    /// <see cref="Facade{TService}"/> is the abstract implemented by all facade classes.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <remarks>
    /// <code>public class FileSystem : Facade&gt;IFileSystem&lt;{ }</code>
    /// </remarks>
    public abstract class Facade<TService>
    {
        /// <summary>
        /// The service name.
        /// </summary>
        private static readonly string Service;

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
        /// Whether the resolved object has been released.
        /// </summary>
        private static bool released;

        /// <summary>
        /// Initializes static members of the <see cref="Facade{TService}"/> class.
        /// </summary>
        static Facade()
        {
            Service = App.Type2Service(typeof(TService));
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
        /// Gets a value indicating whether the resolved instance is exists in the facade.
        /// <para>If it is a non-static binding then return forever false.</para>
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

            if (!inited && (App.IsResolved(Service) || App.CanMake(Service)))
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

            var newBinder = App.GetBind(Service);
            if (newBinder == null || !newBinder.IsStatic)
            {
                binder = newBinder;
                return Build(userParams);
            }

            Rebind(newBinder);
            return instance = Build(userParams);
        }

        /// <summary>
        /// When the resolved object is released.
        /// </summary>
        /// <param name="oldBinder">The old bind data with resolved object.</param>
        /// <param name="instance">The ignored parameter.</param>
        private static void OnRelease(IBindData oldBinder, object instance)
        {
            if (oldBinder != binder)
            {
                return;
            }

            Facade<TService>.instance = default(TService);
            released = true;
        }

        /// <summary>
        /// When the resolved object is rebound.
        /// </summary>
        /// <param name="newService">The new resolved object.</param>
        private static void ServiceRebound(TService newService)
        {
            var newBinder = App.GetBind(Service);
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
            return (TService)App.Make(Service, userParams);
        }
    }
}
