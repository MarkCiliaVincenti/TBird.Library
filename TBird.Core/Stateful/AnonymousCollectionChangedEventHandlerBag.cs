﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace TBird.Core.Stateful
{
    internal class AnonymousCollectionChangedEventHandlerBag : IEnumerable<KeyValuePair<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventHandler>>>
    {
        private readonly Dictionary<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventHandler>> _handlerDictionary = new Dictionary<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventHandler>>();
        private readonly WeakReference<INotifyCollectionChanged> _source;

        private readonly List<NotifyCollectionChangedEventHandler> _allHandlerList = new List<NotifyCollectionChangedEventHandler>();

        private readonly Dictionary<List<NotifyCollectionChangedEventHandler>, object> _lockObjectDictionary = new Dictionary<List<NotifyCollectionChangedEventHandler>, object>();

        private readonly object _handlerDictionaryLockObject = new object();
        private readonly object _allHandlerListLockObject = new object();

        internal AnonymousCollectionChangedEventHandlerBag(INotifyCollectionChanged source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _source = new WeakReference<INotifyCollectionChanged>(source);
        }

        internal AnonymousCollectionChangedEventHandlerBag(INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler)
            : this(source)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            RegisterHandler(handler);
        }

        internal void RegisterHandler(NotifyCollectionChangedEventHandler handler)
        {
            lock (_allHandlerListLockObject)
            {
                _allHandlerList.Add(handler);
            }
        }

        internal void RegisterHandler(NotifyCollectionChangedAction action, NotifyCollectionChangedEventHandler handler)
        {
            lock (_handlerDictionaryLockObject)
            {
                List<NotifyCollectionChangedEventHandler> bag;
                if (!_handlerDictionary.TryGetValue(action, out bag))
                {
                    bag = new List<NotifyCollectionChangedEventHandler>();
                    _lockObjectDictionary.Add(bag, new object());
                    _handlerDictionary[action] = bag;
                }
                bag.Add(handler);
            }
        }

        internal void ExecuteHandler(NotifyCollectionChangedEventArgs e)
        {
            INotifyCollectionChanged sourceResult;
            var result = _source.TryGetTarget(out sourceResult);

            if (!result) return;

            List<NotifyCollectionChangedEventHandler> list;
            lock (_handlerDictionaryLockObject)
            {
                _handlerDictionary.TryGetValue(e.Action, out list);
            }
            if (list != null)
            {
                lock (_lockObjectDictionary[list])
                {
                    foreach (var handler in list)
                    {
                        handler(sourceResult, e);
                    }
                }
            }

            lock (_allHandlerListLockObject)
            {
                if (_allHandlerList.Any())
                {
                    foreach (var handler in _allHandlerList)
                    {
                        handler(sourceResult, e);
                    }
                }
            }
        }

        IEnumerator<KeyValuePair<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventHandler>>> IEnumerable<KeyValuePair<NotifyCollectionChangedAction, List<NotifyCollectionChangedEventHandler>>>.GetEnumerator()
            => _handlerDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _handlerDictionary.GetEnumerator();

        internal void Add(NotifyCollectionChangedEventHandler handler) => RegisterHandler(handler);

        internal void Add(NotifyCollectionChangedAction action, NotifyCollectionChangedEventHandler handler) => RegisterHandler(action, handler);

        internal void Add(NotifyCollectionChangedAction action, params NotifyCollectionChangedEventHandler[] handlers)
        {
            foreach (var handler in handlers)
            {
                RegisterHandler(action, handler);
            }
        }
    }
}