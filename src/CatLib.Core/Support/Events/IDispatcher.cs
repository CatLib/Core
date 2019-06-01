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
    /// <see cref="IDispatcher"/> is the interface implemented by all event listener systems.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Determine if a given event has listeners.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="strict">Indicates whether the event is matched by a wildcard.</param>
        /// <returns>true if a given event has listener.</returns>
        bool HasListeners(string eventName, bool strict = false);

        /// <summary>
        /// Fire an event and call the listeners.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="payloads">The object passed to the listener.</param>
        /// <returns>The listener return values.</returns>
        object[] Trigger(string eventName, params object[] payloads);

        /// <summary>
        /// Trigger an event, terminate after encountering the first event
        /// , and get the return result of the listener.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="payloads">The object passed to the listener.</param>
        /// <returns>The first return result of the listener.</returns>
        object TriggerHalt(string eventName, params object[] payloads);

        /// <summary>
        /// Register an event listener with the dispatcher.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="execution">The event listener.</param>
        /// <param name="group">The event group，If it is null, it will not be grouped.</param>
        /// <returns>事件对象.</returns>
#pragma warning disable CA1716
        IEvent On(string eventName, Func<string, object[], object> execution, object group = null);
#pragma warning restore CA1716

        /// <summary>
        /// Remove a set of listeners from the dispatcher.
        /// </summary>
        /// <param name="target">
        /// <para>If the <see cref="string"/> is passed in, all events corresponding
        /// to the event name will be removed.</para>
        /// <para>If the event object (IEvent) is passed, the corresponding event is released.</para>
        /// <para>If the incoming object is an object, all events under the group will be released.</para>
        /// </param>
        void Off(object target);
    }
}
