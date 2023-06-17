﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TBird.Core.Stateful
{
    internal class AnonymousPropertyChangedEventHandlerBag : IEnumerable<KeyValuePair<string, List<PropertyChangedEventHandler>>>
    {
        private readonly Dictionary<string, List<PropertyChangedEventHandler>> _handlerDictionary = new Dictionary<string, List<PropertyChangedEventHandler>>();
        private readonly WeakReference<INotifyPropertyChanged> _source;

        private readonly object _handlerDictionaryLockObject = new object();
        private readonly Dictionary<List<PropertyChangedEventHandler>, object> _lockObjectDictionary = new Dictionary<List<PropertyChangedEventHandler>, object>();

        internal AnonymousPropertyChangedEventHandlerBag(INotifyPropertyChanged source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _source = new WeakReference<INotifyPropertyChanged>(source);
        }

        internal AnonymousPropertyChangedEventHandlerBag(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
            : this(source)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            RegisterHandler(handler);
        }

        internal void RegisterHandler(PropertyChangedEventHandler handler) => RegisterHandler(string.Empty, handler);

        internal void RegisterHandler(string propertyName, PropertyChangedEventHandler handler)
        {
            lock (_handlerDictionaryLockObject)
            {
                List<PropertyChangedEventHandler> bag;
                if (!_handlerDictionary.TryGetValue(propertyName, out bag))
                {
                    bag = new List<PropertyChangedEventHandler>();
                    _lockObjectDictionary.Add(bag, new object());
                    _handlerDictionary[propertyName] = bag;
                }
                bag.Add(handler);
            }
        }

        internal void ExecuteHandler(PropertyChangedEventArgs e)
        {
            INotifyPropertyChanged sourceResult;
            var result = _source.TryGetTarget(out sourceResult);

            if (!result) return;

            if (e.PropertyName != null)
            {
                List<PropertyChangedEventHandler> list;
                lock (_handlerDictionaryLockObject)
                {
                    _handlerDictionary.TryGetValue(e.PropertyName, out list);
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
            }

            lock (_handlerDictionaryLockObject)
            {
                List<PropertyChangedEventHandler> allList;
                _handlerDictionary.TryGetValue(string.Empty, out allList);
                if (allList != null)
                {
                    lock (_lockObjectDictionary[allList])
                    {
                        foreach (var handler in allList)
                        {
                            handler(sourceResult, e);
                        }
                    }
                }
            }
        }

        IEnumerator<KeyValuePair<string, List<PropertyChangedEventHandler>>> IEnumerable<KeyValuePair<string, List<PropertyChangedEventHandler>>>.GetEnumerator()
            => _handlerDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _handlerDictionary.GetEnumerator();

        internal void Add(PropertyChangedEventHandler handler) => RegisterHandler(handler);

        internal void Add(string propertyName, PropertyChangedEventHandler handler) => RegisterHandler(propertyName, handler);

        internal void Add(string propertyName, params PropertyChangedEventHandler[] handlers)
        {
            foreach (var handler in handlers)
            {
                RegisterHandler(propertyName, handler);
            }
        }
    }
}
