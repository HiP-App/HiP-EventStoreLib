﻿using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.FakeStore
{
    public class FakeEventStream : IEventStream
    {
        private readonly FakeEventStore _eventStore;
        private readonly List<IEvent> _events = new List<IEvent>();
        private readonly AsyncLock _mutex = new AsyncLock();
        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>();
        private readonly Subject<EventAppendedArgs> _appended = new Subject<EventAppendedArgs>();
        private bool _isDeleted;

        public IObservable<EventAppendedArgs> Appended => _appended;

        public IReadOnlyList<IEvent> Events => _events; // direct access to the events for testing purposes

        public string Name { get; }

        public FakeEventStream(FakeEventStore eventStore, string name)
        {
            _eventStore = eventStore;
            Name = name;
        }

        public async Task AppendAsync(IEvent ev) =>
            await AppendManyAsync(Enumerable.Repeat(ev ?? throw new ArgumentNullException(nameof(ev)), 1));

        public async Task AppendManyAsync(IEnumerable<IEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            using (await BeginCriticalSectionAsync())
                AppendEventsCore(events);
        }

        public EventStreamTransaction BeginTransaction()
        {
            var lockToken = BeginCriticalSectionAsync().Result;

            try
            {
                var transaction = new EventStreamTransaction();

                transaction.WhenCompleted.ContinueWith(task =>
                {
                    AppendEventsCore(task.Result);
                    lockToken.Dispose();
                });

                return transaction;
            }
            catch
            {
                lockToken.Dispose();
                throw;
            }
        }

        public async Task DeleteAsync()
        {
            using (await BeginCriticalSectionAsync())
            {
                _isDeleted = true;
                _eventStore.Streams.Delete(Name);
                _appended.OnCompleted();
                _appended.Dispose();
            }
        }

        public IEventStreamEnumerator GetEnumerator()
        {
            using (BeginCriticalSectionAsync().Result)
                return new FakeEventStoreStreamEnumerator(_events.GetEnumerator());
        }

        public IEventStreamSubscription SubscribeCatchUp(Action<IEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            using (BeginCriticalSectionAsync().Result)
                return new FakeEventStoreStreamCatchUpSubscription(_events, _appended.Select(e => e.Event), handler);
        }

        public async Task SetMetadataAsync(string key, object value)
        {
            using (await BeginCriticalSectionAsync())
                _metadata[key] = value;
        }

        public async Task<(T value, bool isSuccessful)> TryGetMetadataAsync<T>(string key)
        {
            using (await BeginCriticalSectionAsync())
            {
                return _metadata.TryGetValue(key, out var value)
                    ? ((T)value, true)
                    : (default(T), false);
            }
        }

        private void AppendEventsCore(IEnumerable<IEvent> events)
        {
            var eventsList = events.ToList();
            _events.AddRange(eventsList);
            foreach (var ev in eventsList)
                _appended.OnNext(new EventAppendedArgs(this, ev));
        }

        private async Task<IDisposable> BeginCriticalSectionAsync()
        {
            var lockToken = await _mutex.LockAsync();

            if (_isDeleted)
                throw new StreamDeletedException();

            return lockToken;
        }
    }
}
