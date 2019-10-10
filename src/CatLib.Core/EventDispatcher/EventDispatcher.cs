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
        public virtual bool AddListener(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
            {
                return false;
            }

            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                listeners[eventName] = handlers = new List<EventHandler>();
            }
            else if (handlers.Contains(handler))
            {
                return false;
            }

            handlers.Add(handler);
            return true;
        }

        /// <inheritdoc />
        public virtual void Raise(string eventName, object sender, EventArgs e = null)
        {
            Guard.Requires<LogicException>(!(sender is EventArgs), $"Passed event args for the parameter {sender}, Did you make a wrong method call?");

            e = e ?? EventArgs.Empty;
            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                return;
            }

            foreach (var listener in handlers)
            {
                if (e is IStoppableEvent stoppableEvent
                    && stoppableEvent.IsPropagationStopped)
                {
                    break;
                }

                listener(sender, e);
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
        public virtual bool RemoveListener(string eventName, EventHandler handler = null)
        {
            if (handler == null)
            {
                return listeners.Remove(eventName);
            }

            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                return false;
            }

            var status = handlers.Remove(handler);
            if (handlers.Count <= 0)
            {
                listeners.Remove(eventName);
            }

            return status;
        }
    }
}
