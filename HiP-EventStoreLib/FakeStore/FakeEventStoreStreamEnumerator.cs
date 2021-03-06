﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.FakeStore
{
    public class FakeEventStoreStreamEnumerator : IEventStreamEnumerator
    {
        private readonly IEnumerator<IEvent> _syncEnumerator;

        public FakeEventStoreStreamEnumerator(IEnumerator<IEvent> syncEnumerator)
        {
            _syncEnumerator = syncEnumerator;
        }

        public IEvent Current => _syncEnumerator.Current;

        public event EventHandler<EventParsingFailedArgs> EventParsingFailed;

        public Task<bool> MoveNextAsync() => Task.FromResult(_syncEnumerator.MoveNext());

        public void Reset() => _syncEnumerator.Reset();
    }
}
