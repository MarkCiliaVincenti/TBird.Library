﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TBird.Core.Stateful
{
    public class AnonymousDisposable : IDisposable
    {
        private readonly Action _releaseAction;
        private bool _disposed;

        public AnonymousDisposable(Action releaseAction)
        {
            _releaseAction = releaseAction;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _releaseAction();
            }
            _disposed = true;
        }
    }
}
