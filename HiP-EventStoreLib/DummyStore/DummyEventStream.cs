using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.DummyStore
{
    public class DummyEventStream : IEventStream
    {
        private readonly DummyEventStore _eventStore;
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);
        private readonly List<IEvent> _events = new List<IEvent>();
        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>();
        private readonly Subject<(IEventStream sender, IEvent ev)> _appended = new Subject<(IEventStream sender, IEvent ev)>();
        private bool _isDeleted;

        public IObservable<(IEventStream sender, IEvent ev)> Appended => throw new NotImplementedException();

        public string Name { get; }

        public DummyEventStream(DummyEventStore eventStore, string name)
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
            if (_isDeleted)
                throw new StreamDeletedException();

            _sema.Wait();

            try
            {
                var transaction = new EventStreamTransaction(this);

                transaction.WhenCompleted.ContinueWith(task =>
                {
                    AppendEventsCore(task.Result);
                    _sema.Release();
                });

                return transaction;
            }
            finally
            {
                _sema.Release();
            }
        }

        public async Task DeleteAsync()
        {
            using (await BeginCriticalSectionAsync())
            {
                _isDeleted = true;
                _eventStore.DeleteStream(Name);
                _appended.OnCompleted();
                _appended.Dispose();
            }
        }

        public IEventStreamEnumerator GetEnumerator()
        {
            using (BeginCriticalSectionAsync().Result)
                return new DummyEventStoreStreamEnumerator(_events.GetEnumerator());
        }

        public IEventStreamSubscription SubscribeCatchUp()
        {
            throw new NotImplementedException();
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
                _appended.OnNext((this, ev));
        }

        private async Task<IDisposable> BeginCriticalSectionAsync()
        {
            if (_isDeleted)
                throw new StreamDeletedException();

            await _sema.WaitAsync();
            return Disposable.Create(() => _sema.Release());
        }
    }
}
