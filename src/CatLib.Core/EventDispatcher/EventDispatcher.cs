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
using CatLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CatLib.EventDispatcher
{
    /// <inheritdoc />
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IDictionary<string, IList<EventHandler>> listeners;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDispatcher"/> class.
        /// </summary>
        public EventDispatcher()
        {
            listeners = new Dictionary<string, IList<EventHandler>>();
        }

        /// <inheritdoc />
        public virtual bool AddListener(string eventName, EventHandler listener)
        {
            if (string.IsNullOrEmpty(eventName) || listener == null)
            {
                return false;
            }

            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                listeners[eventName] = handlers = new List<EventHandler>();
            }
            else if (handlers.Contains(listener))
            {
                return false;
            }

            handlers.Add(listener);
            return true;
        }

        /// <inheritdoc />
        public virtual void Raise(string eventName, object sender, EventArgs args = null)
        {
            Guard.Requires<LogicException>(!(sender is EventArgs), $"Passed event args for the parameter {sender}, Did you make a wrong method call?");

            args = args ?? EventArgs.Empty;
            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                return;
            }

            foreach (var listener in handlers)
            {
                if (args is IStoppableEvent stoppableEvent
                    && stoppableEvent.IsPropagationStopped)
                {
                    break;
                }

                listener(sender, args);
            }
        }

        /// <inheritdoc />
        public virtual EventHandler[] GetListeners(string eventName)
        {
            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                return Array.Empty<EventHandler>();
            }

            return handlers.ToArray();
        }

        /// <inheritdoc />
        public virtual bool HasListener(string eventName)
        {
            return listeners.ContainsKey(eventName);
        }

        /// <inheritdoc />
        public virtual bool RemoveListener(string eventName, EventHandler listener = null)
        {
            if (listener == null)
            {
                return listeners.Remove(eventName);
            }

            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                return false;
            }

            var status = handlers.Remove(listener);
            if (handlers.Count <= 0)
            {
                listeners.Remove(eventName);
            }

            return status;
        }
    }
}
