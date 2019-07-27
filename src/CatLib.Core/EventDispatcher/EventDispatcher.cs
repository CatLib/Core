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

using CatLib.Exception;
using CatLib.Support;
using System;
using System.Collections.Generic;

namespace CatLib.EventDispatcher
{
    /// <inheritdoc />
    public sealed class EventDispatcher : IEventDispatcher
    {
        private readonly IDictionary<string, SortSet<Action<EventArgs>, int>> listeners;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDispatcher"/> class.
        /// </summary>
        public EventDispatcher()
        {
            listeners = new Dictionary<string, SortSet<Action<EventArgs>, int>>();
        }

        /// <inheritdoc />
        public bool AddListener(string eventName, Action<EventArgs> listener, int priority = 0)
        {
            if (string.IsNullOrEmpty(eventName) || listener == null)
            {
                return false;
            }

            if (!listeners.TryGetValue(eventName, out SortSet<Action<EventArgs>, int> collection))
            {
                listeners[eventName] = collection = new SortSet<Action<EventArgs>, int>();
            }
            else if (collection.Contains(listener))
            {
                return false;
            }

            collection.Add(listener, priority);
            return true;
        }

        /// <inheritdoc />
        public EventArgs Dispatch(string eventName, EventArgs eventArgs)
        {
            if (!listeners.TryGetValue(eventName, out SortSet<Action<EventArgs>, int> collection))
            {
                return eventArgs;
            }

            Guard.Requires<AssertException>(
                collection.Count > 0,
                "Assertion error: The number of listeners should be greater than 0.");

            foreach (var listener in collection)
            {
                if (eventArgs is IStoppableEvent stoppableEvent
                    && stoppableEvent.IsPropagationStopped())
                {
                    break;
                }

                listener.Invoke(eventArgs);
            }

            return eventArgs;
        }

        /// <inheritdoc />
        public Action<EventArgs>[] GetListeners(string eventName)
        {
            if (!listeners.TryGetValue(eventName, out SortSet<Action<EventArgs>, int> collection))
            {
                return Array.Empty<Action<EventArgs>>();
            }

            return collection.ToArray();
        }

        /// <inheritdoc />
        public bool HasListeners(string eventName)
        {
            return listeners.ContainsKey(eventName);
        }

        /// <inheritdoc />
        public bool RemoveListener(string eventName, Action<EventArgs> listener = null)
        {
            if (listener == null)
            {
                return listeners.Remove(eventName);
            }

            if (!listeners.TryGetValue(eventName, out SortSet<Action<EventArgs>, int> collection))
            {
                return false;
            }

            var status = collection.Remove(listener);

            if (collection.Count <= 0)
            {
                status = listeners.Remove(eventName);
            }

            return status;
        }
    }
}
