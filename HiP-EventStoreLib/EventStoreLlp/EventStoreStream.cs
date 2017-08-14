using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using System.Linq;
using System.Threading;
using System.Reactive.Subjects;
using PaderbornUniversity.SILab.Hip.EventSourcing.Migrations;
using System.Reactive.Linq;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public class EventStoreStream : IEventStream
    {
        private readonly Subject<(IEventStream sender, IEvent ev)> _appended = new Subject<(IEventStream sender, IEvent ev)>();
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);
        private EventStore _connection;
        private bool _isDeleted;

        public IObservable<(IEventStream sender, IEvent ev)> Appended => _appended;

        public string Name { get; }

        public EventStoreStream(EventStore connection, string name)
        {
            _connection = connection;
            Name = name;
        }

        public IEventStreamEnumerator GetEnumerator()
        {
            _sema.Wait();

            if (_isDeleted)
                throw new StreamDeletedException();

            try
            {
                return new EventStoreStreamEnumerator(_connection.UnderlyingConnection, Name);
            }
            finally
            {
                _sema.Release();
            }
        }

        public async Task AppendAsync(IEvent ev) =>
            await AppendManyAsync(Enumerable.Repeat(ev, 1));

        public async Task AppendManyAsync(IEnumerable<IEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            await _sema.WaitAsync();

            if (_isDeleted)
                throw new StreamDeletedException();

            try
            {
                var eventsList = events.ToList();

                // persist events in Event Store
                var eventData = eventsList.Select(ev => ev.ToEventData(Guid.NewGuid()));
                var result = await _connection.UnderlyingConnection.AppendToStreamAsync(Name, ExpectedVersion.Any, eventData);

                // forward events to indices so they can update their state
                foreach (var ev in eventsList)
                    _appended.OnNext((this, ev));
            }
            finally
            {
                _sema.Release();
            }
        }

        public async Task DeleteAsync()
        {
            await _sema.WaitAsync();

            if (_isDeleted)
                throw new StreamDeletedException();

            try
            {
                _isDeleted = true;
                _appended.OnCompleted();
                _appended.Dispose();
                await _connection.DeleteStreamAsync(Name);
            }
            finally
            {
                _sema.Release();
            }
        }

        public EventStreamTransaction BeginTransaction()
        {
            return new EventStreamTransaction(this);
        }

        public async Task<(T value, bool isSuccessful)> TryGetMetadataAsync<T>(string key)
        {
            var meta = await _connection.UnderlyingConnection.GetStreamMetadataAsync(Name);
            return meta.StreamMetadata.TryGetValue<T>(key, out var value)
                ? (value, true)
                : (default(T), false);
        }

        public async Task SetMetadataAsync(string key, object value)
        {
            var meta = await _connection.UnderlyingConnection.GetStreamMetadataAsync(Name);
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

            await _connection.UnderlyingConnection.SetStreamMetadataAsync(Name, ExpectedVersion.Any, newMeta);
        }

        public IEventStreamSubscription SubscribeCatchUp()
        {
            return new EventStoreStreamCatchUpSubscription(_connection.UnderlyingConnection, Name);
        }
    }

    public class EventStoreStreamCatchUpSubscription : IEventStreamSubscription
    {
        private readonly ReplaySubject<IEvent> _eventAppeared = new ReplaySubject<IEvent>();
        private readonly EventStoreCatchUpSubscription _subscription;

        public IObservable<IEvent> EventAppeared => _eventAppeared;

        public event EventHandler<EventParsingFailedArgs> EventParsingFailed;

        public EventStoreStreamCatchUpSubscription(IEventStoreConnection connection, string streamName)
        {
            _subscription = connection.SubscribeToStreamFrom(
                streamName,
                null, // don't use StreamPosition.Start (see https://groups.google.com/forum/#!topic/event-store/8tpXJMNEMqI),
                CatchUpSubscriptionSettings.Default,
                OnEventAppeared);
        }

        private void OnEventAppeared(EventStoreCatchUpSubscription _, ResolvedEvent resolvedEvent)
        {
            try
            {
                // Note regarding migration:
                // Event types may change over time (properties get added/removed etc.)
                // Whenever an event has multiple versions, an event of an obsolete type should be transformed to an event
                // of the latest version, so that ApplyEvent(...) only has to deal with events of the current version.

                var ev = resolvedEvent.Event.ToIEvent().MigrateToLatestVersion();
                _eventAppeared.OnNext(ev);
            }
            catch (Exception e)
            {
                EventParsingFailed?.Invoke(this, new EventParsingFailedArgs(resolvedEvent, e));
            }
        }

        public void Dispose()
        {
            _subscription.Stop();
            _eventAppeared.OnCompleted();
            _eventAppeared.Dispose();
        }
    }
}
