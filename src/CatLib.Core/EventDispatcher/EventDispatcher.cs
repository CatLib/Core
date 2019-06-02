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
    /// <inheritdoc />
    public sealed class EventDispatcher : IEventDispatcher
    {
        private readonly bool inheritancePropagation;
        private readonly IDictionary<Type, SortSet<WrappedListener, int>> listeners;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventDispatcher"/> class.
        /// </summary>
        /// <param name="inheritancePropagation">Whether to trigger an event based on the inheritance chain.</param>
        public EventDispatcher(bool inheritancePropagation = true)
        {
            this.inheritancePropagation = inheritancePropagation;
            listeners = new Dictionary<Type, SortSet<WrappedListener, int>>();
        }

        /// <inheritdoc />
        public void AddListener<T>(Action<T> listener, int priority = 0)
            where T : EventArgs
        {
            Guard.Requires<ArgumentNullException>(listener != null);
            DoAddListener(typeof(T), new WrappedListener<T>(listener), priority);
        }

        /// <inheritdoc />
        public T Dispatch<T>(T eventArgs)
            where T : EventArgs
        {
            DoDispatch(eventArgs);
            return eventArgs;
        }

        /// <inheritdoc />
        public IEnumerable<Action<T>> GetListeners<T>()
            where T : EventArgs
        {
            if (!listeners.TryGetValue(typeof(T), out SortSet<WrappedListener, int> collection))
            {
                return Array.Empty<Action<T>>();
            }

            return Arr.Map(collection, (wrappedListener) => ((WrappedListener<T>)wrappedListener).GetAction());
        }

        /// <inheritdoc />
        public bool HasListeners<T>()
             where T : EventArgs
        {
            return listeners.ContainsKey(typeof(T));
        }

        /// <inheritdoc />
        public void RemoveListener<T>(Action<T> listener = null)
             where T : EventArgs
        {
            DoRemoveListener(typeof(T), listener == null ?
                null : new WrappedListener<T>(listener));
        }

        private void DoDispatch(EventArgs eventArgs)
        {
            var eventType = eventArgs.GetType();
            do
            {
                CallListener(eventType, eventArgs);
                eventType = eventType.BaseType;
            }
            while (inheritancePropagation && eventType.BaseType != null);
        }

        private void DoAddListener(Type eventType, WrappedListener wrappedListener, int priority)
        {
            if (!listeners.TryGetValue(eventType, out SortSet<WrappedListener, int> collection))
            {
                listeners[eventType] = collection = new SortSet<WrappedListener, int>();
            }
            else if (collection.Contains(wrappedListener))
            {
                throw new RuntimeException($"Unable to add multiple times to the same listener: \"{wrappedListener}\"");
            }

            collection.Add(wrappedListener, priority);
        }

        private void DoRemoveListener(Type eventType, WrappedListener wrappedListener)
        {
            if (wrappedListener == null)
            {
                listeners.Remove(eventType);
                return;
            }

            if (!listeners.TryGetValue(eventType, out SortSet<WrappedListener, int> collection))
            {
                return;
            }

            collection.Remove(wrappedListener);

            if (collection.Count <= 0)
            {
                listeners.Remove(eventType);
            }
        }

        private void CallListener(Type eventType, EventArgs eventArgs)
        {
            if (!listeners.TryGetValue(eventType, out SortSet<WrappedListener, int> collection))
            {
                return;
            }

            foreach (var listener in collection)
            {
                if (eventArgs is IStoppableEvent stoppableEvent
                    && stoppableEvent.IsPropagationStopped())
                {
                    break;
                }

                listener.Invoke(eventArgs);
            }
        }

        private abstract class WrappedListener
        {
            public abstract void Invoke(EventArgs eventArgs);
        }

        private sealed class WrappedListener<T> : WrappedListener
            where T : EventArgs
        {
            private readonly Action<T> listener;

            public WrappedListener(Action<T> listener)
            {
                this.listener = listener;
            }

            public Action<T> GetAction()
            {
                return listener;
            }

            public override void Invoke(EventArgs eventArgs)
            {
                listener.Invoke((T)eventArgs);
            }

            public override int GetHashCode()
            {
                return listener.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is WrappedListener<T> wrappedListener)
                {
                    return listener.Equals(wrappedListener.listener);
                }

                return false;
            }

            public override string ToString()
            {
                return listener.ToString();
            }
        }
    }
}
