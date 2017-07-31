using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using System.Linq;
using System.Threading;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public class EventStoreStream : IEventStream
    {
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);
        private EventStoreConnection _connection;
        private bool _isDeleted;

        public event EventAppendedEventHandler Appended;

        public string Name { get; }

        public EventStoreStream(EventStoreConnection connection, string name)
        {
            _connection = connection;
            Name = name;
        }

        public IAsyncEnumerator<IEvent> GetEnumerator()
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
                    Appended?.Invoke(this, ev);
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
                Appended = null;
                await _connection.DeleteStreamAsync(Name);
            }
            finally
            {
                _sema.Release();
            }
        }
    }
}
