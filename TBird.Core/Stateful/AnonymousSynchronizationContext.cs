﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TBird.Core.Stateful
{
    public class AnonymousSynchronizationContext : SynchronizationContext
    {
        private Action<SendOrPostCallback> _syncCallback;
        private Action<SendOrPostCallback> _asyncCallback;

        private AnonymousSynchronizationContext() { }

        public static AnonymousSynchronizationContext CreateForSync(Action<SendOrPostCallback> callback)
        {
            return Create(callback, null);
        }

        public static AnonymousSynchronizationContext CreateForAsync(Action<SendOrPostCallback> callback)
        {
            return Create(null, callback);
        }

        public static AnonymousSynchronizationContext Create(Action<SendOrPostCallback> syncCallback, Action<SendOrPostCallback> asyncCallback)
        {
            var result = new AnonymousSynchronizationContext
            {
                _syncCallback = syncCallback,
                _asyncCallback = asyncCallback
            };
            return result;
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (_syncCallback != null)
            {
                _syncCallback(d);
                return;
            }

            base.Send(d, state);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            if (_asyncCallback != null)
            {
                _asyncCallback(d);
                return;
            }

            base.Post(d, state);
        }
    }
}
