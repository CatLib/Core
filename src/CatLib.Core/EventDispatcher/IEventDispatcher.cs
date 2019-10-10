﻿/*
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

namespace CatLib.EventDispatcher
{
    /// <summary>
    /// Represents an event dispatcher.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Adds an event listener that listens on the specified events.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="listener">The listener.</param>
        /// <returns>True if the listener added. otherwise false if listener already exists.</returns>
        bool AddListener(string eventName, EventHandler listener);

        /// <summary>
        /// Removes an event listener from the specified events.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="listener">Remove the specified listener, otherwise remove all listeners under the event.</param>
        /// <returns>True if removed the listener.</returns>
        bool RemoveListener(string eventName, EventHandler listener = null);

        /// <summary>
        /// Gets the listeners of a specific event or all listeners sorted by descending priority. Will not return listeners in the inheritance chain.
        /// </summary>
        /// <typeparam name="T">The specified events type.</typeparam>
        /// <param name="eventName">The event name.</param>
        /// <returns>The event listeners for the specified event. Never return null.</returns>
        EventHandler[] GetListeners(string eventName);

        /// <summary>
        /// Whether an event has any registered listeners. Will not return listeners in the inheritance chain.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <returns>True if the event has any registered listeners.</returns>
        bool HasListener(string eventName);

        /// <summary>
        /// Provide all relevant listeners with an event to process.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The event object to process.</param>
#pragma warning disable CA1030
        void Raise(string eventName, object sender, EventArgs args = null);
#pragma warning restore CA1030
    }
}
