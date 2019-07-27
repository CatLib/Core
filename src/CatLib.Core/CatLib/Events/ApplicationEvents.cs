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

namespace CatLib.Events
{
    /// <summary>
    /// Contains all events dispatched by an Application.
    /// </summary>
    public static class ApplicationEvents
    {
        /// <summary>
        /// Before the <see cref="Application.Bootstrap"/> call.
        /// </summary>
        public static readonly string OnBeforeBoot = $"{BaseEventArgs}.OnBeforeBoot";

        /// <summary>
        /// When the <see cref="Application.Bootstrap"/> call is in progress.
        /// </summary>
        public static readonly string OnBooting = $"{BaseEventArgs}.BootingEventArgs";

        /// <summary>
        /// After the <see cref="Application.Bootstrap"/> called.
        /// </summary>
        public static readonly string OnAfterBoot = $"{BaseEventArgs}.AfterBootEventArgs";

        /// <summary>
        /// When registering for a service provider.
        /// </summary>
        public static readonly string OnRegisterProvider = $"{BaseEventArgs}.RegisterProviderEventArgs";

        /// <summary>
        /// Before the <see cref="Application.Init"/> call.
        /// </summary>
        public static readonly string OnBeforeInit = $"{BaseEventArgs}.BeforeInitEventArgs";

        /// <summary>
        /// Before the <see cref="IServiceProvider.Init"/> call.
        /// </summary>
        public static readonly string OnInitProvider = $"{BaseEventArgs}.InitProviderEventArgs";

        /// <summary>
        /// After the <see cref="Application.Init"/> called.
        /// </summary>
        public static readonly string OnAfterInit = $"{BaseEventArgs}.AfterInitEventArgs";

        /// <summary>
        /// When the framework is started.
        /// </summary>
        public static readonly string OnStartCompleted = $"{BaseEventArgs}.StartCompletedEventArgs";

        /// <summary>
        /// Before the <see cref="Application.Terminate"/> call.
        /// </summary>
        public static readonly string OnBeforeTerminate = $"{BaseEventArgs}.BeforeTerminateEventArgs";

        /// <summary>
        /// After the <see cref="Application.Terminate"/> called.
        /// </summary>
        public static readonly string OnAfterTerminate = $"{BaseEventArgs}.AfterTerminateEventArgs";

        private const string BaseEventArgs = "EventArgs.ApplicationEventArgs";
    }
}
