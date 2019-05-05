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
    /// Contains all events dispatched by an Application.
    /// </summary>
    public sealed class ApplicationEvents
    {
        /// <summary>
        /// Before the <see cref="Application.Bootstrap"/> call.
        /// </summary>
        public static readonly string OnBootstrap = "CatLib.ApplicationEvents.OnBootstrap";

        /// <summary>
        /// When the <see cref="Application.Bootstrap"/> call is in progress.
        /// </summary>
        public static readonly string Bootstrapping = "CatLib.ApplicationEvents.Bootstrapping";

        /// <summary>
        /// After the <see cref="Application.Bootstrap"/> called.
        /// </summary>
        public static readonly string OnBootstraped = "CatLib.ApplicationEvents.OnBootstraped";

        /// <summary>
        /// When registering for a service provider.
        /// </summary>
        public static readonly string OnRegisterProvider = "CatLib.ApplicationEvents.OnRegisterProvider";

        /// <summary>
        /// Before the <see cref="Application.Init"/> call.
        /// </summary>
        public static readonly string OnInit = "CatLib.ApplicationEvents.OnInit";

        /// <summary>
        /// Before the <see cref="IServiceProvider.Init"/> call.
        /// </summary>
        public static readonly string OnProviderInit = "CatLib.ApplicationEvents.OnProviderInit";

        /// <summary>
        /// After the <see cref="IServiceProvider.Init"/> called.
        /// </summary>
        public static readonly string OnProviderInited = "CatLib.ApplicationEvents.OnProviderInited";

        /// <summary>
        /// After the <see cref="Application.Init"/> called.
        /// </summary>
        public static readonly string OnInited = "CatLib.ApplicationEvents.OnInited";

        /// <summary>
        /// When the framework is started.
        /// </summary>
        public static readonly string OnStartCompleted = "CatLib.ApplicationEvents.OnStartCompleted";

        /// <summary>
        /// Before the <see cref="Application.Terminate"/> call.
        /// </summary>
        public static readonly string OnTerminate = "CatLib.ApplicationEvents.OnTerminate";

        /// <summary>
        /// After the <see cref="Application.Terminate"/> called.
        /// </summary>
        public static readonly string OnTerminated = "CatLib.ApplicationEvents.OnTerminated";
    }
}