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
using System.Collections.Generic;

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
        /// <typeparam name="T">The specified events type.</typeparam>
        /// <param name="listener">The listener.</param>
        /// <param name="priority">The smaller this value, the earlier an event. listener will be triggered in the chain.</param>
        void AddListener<T>(Action<T> listener, int priority = 0)
            where T : EventArgs;

        /// <summary>
        /// Removes an event listener from the specified events.
        /// </summary>
        /// <typeparam name="T">The specified events type.</typeparam>
        /// <param name="listener">Remove the specified listener, otherwise remove all listeners under the event.</param>
        void RemoveListener<T>(Action<T> listener = null)
             where T : EventArgs;

        /// <summary>
        /// Gets the listeners of a specific event or all listeners sorted by descending priority. Will not return listeners in the inheritance chain.
        /// </summary>
        /// <typeparam name="T">The specified events type.</typeparam>
        /// <returns>The event listeners for the specified event.</returns>
        IEnumerable<Action<T>> GetListeners<T>()
             where T : EventArgs;

        /// <summary>
        /// Whether an event has any registered listeners. Will not return listeners in the inheritance chain.
        /// </summary>
        /// <typeparam name="T">The specified events type.</typeparam>
        /// <returns>True if the event has any registered listeners.</returns>
        bool HasListeners<T>()
             where T : EventArgs;

        /// <summary>
        /// Provide all relevant listeners with an event to process.
        /// </summary>
        /// <typeparam name="T">The specified events type.</typeparam>
        /// <param name="eventArgs">The event object to process.</param>
        /// <returns>The event that was passed, now modified by listeners.</returns>
        T Dispatch<T>(T eventArgs)
            where T : EventArgs;
    }
}
