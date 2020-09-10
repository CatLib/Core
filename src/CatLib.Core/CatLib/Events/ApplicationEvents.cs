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
    public static class ApplicationEvents
    {
        /// <summary>
        /// When registering for a service provider.
        /// </summary>
        public static readonly string OnRegisterProvider = $"{BaseEventArgs}.RegisterProviderEventArgs";

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
