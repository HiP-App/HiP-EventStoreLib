using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using System.Linq;
using System.Reactive.Subjects;
// ReSharper disable once RedundantUsingDirective (R# is wrong, System.Reactive.Linq IS NOT redundant!)
using System.Reactive.Linq;
using Nito.AsyncEx;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public class EventStoreStream : IEventStream
    {
        private readonly Subject<EventAppendedArgs> _appended = new Subject<EventAppendedArgs>();
        private readonly AsyncLock _mutex = new AsyncLock();
        private readonly EventStore _eventStore;
        private bool _isDeleted;

        public IObservable<EventAppendedArgs> Appended => _appended;

        public string Name { get; }

        public EventStoreStream(EventStore eventStore, string name)
        {
            _eventStore = eventStore;
            Name = name;
        }

        public IEventStreamEnumerator GetEnumerator()
        {
            using (BeginCriticalSectionAsync().Result)
                return new EventStoreStreamEnumerator(_eventStore.UnderlyingConnection, Name);
        }

        public async Task AppendAsync(IEvent ev) =>
            await AppendManyAsync(Enumerable.Repeat(ev, 1));

        public async Task AppendManyAsync(IEnumerable<IEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            using (await BeginCriticalSectionAsync())
                await AppendEventsCore(events);
        }

        public async Task DeleteAsync()
        {
            using (await BeginCriticalSectionAsync())
            {
                _isDeleted = true;
                _appended.OnCompleted();
                _appended.Dispose();
                await _eventStore.DeleteStreamAsync(Name);
            }
        }

        public EventStreamTransaction BeginTransaction()
        {
            var lockToken = BeginCriticalSectionAsync().Result;

            try
            {
                var transaction = new EventStreamTransaction();

                transaction.WhenCompleted.ContinueWith(async task =>
                {
                    await AppendEventsCore(task.Result);
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

        public async Task<(T value, bool isSuccessful)> TryGetMetadataAsync<T>(string key)
        {
            using (await BeginCriticalSectionAsync())
            {
                var meta = await _eventStore.UnderlyingConnection.GetStreamMetadataAsync(Name);
                return meta.StreamMetadata.TryGetValue<T>(key, out var value)
                    ? (value, true)
                    : (default(T), false);
            }
        }

        public async Task SetMetadataAsync(string key, object value)
        {
            using (await BeginCriticalSectionAsync())
            {
                var meta = await _eventStore.UnderlyingConnection.GetStreamMetadataAsync(Name);
                var newMeta = meta.StreamMetadata.Copy();

                switch (value)
                {
                    case float v: newMeta.SetCustomProperty(key, v); break;
                    case long v: newMeta.SetCustomProperty(key, v); break;
                    case bool v: newMeta.SetCustomProperty(key, v); break;
                    case decimal v: newMeta.SetCustomProperty(key, v); break;
                    case double v: newMeta.SetCustomProperty(key, v); break;
                    case int v: newMeta.SetCustomProperty(key, v); break;
                    case string v: newMeta.SetCustomProperty(key, v); break;
                    default: throw new ArgumentException($"Values of type '{value.GetType().Name}' are not supported");
                }

                await _eventStore.UnderlyingConnection.SetStreamMetadataAsync(Name, ExpectedVersion.Any, newMeta);
            }
        }

        public IEventStreamSubscription SubscribeCatchUp(Action<IEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            using (BeginCriticalSectionAsync().Result)
                return new EventStoreStreamCatchUpSubscription(_eventStore.UnderlyingConnection, Name, handler);
        }

        private async Task AppendEventsCore(IEnumerable<IEvent> events)
        {
            var eventsList = events.ToList();

            // persist events in Event Store
            var eventData = eventsList.Select(ev => ev.ToEventData(Guid.NewGuid()));
            await _eventStore.UnderlyingConnection.AppendToStreamAsync(Name, ExpectedVersion.Any, eventData);

            // forward events to indices so they can update their state
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
