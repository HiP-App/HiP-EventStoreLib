using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.DummyStore
{
    public class DummyEventStoreStreamEnumerator : IEventStreamEnumerator
    {
        private readonly IEnumerator<IEvent> _syncEnumerator;

        public DummyEventStoreStreamEnumerator(IEnumerator<IEvent> syncEnumerator)
        {
            _syncEnumerator = syncEnumerator;
        }

        public IEvent Current => _syncEnumerator.Current;

        public event EventHandler<EventParsingFailedArgs> EventParsingFailed;

        public Task<bool> MoveNextAsync() => Task.FromResult(_syncEnumerator.MoveNext());

        public void Reset() => _syncEnumerator.Reset();
    }
}
