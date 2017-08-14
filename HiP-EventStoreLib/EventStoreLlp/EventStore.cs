using System.Collections.Generic;
using EventStore.ClientAPI;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public class EventStore : IEventStore, IEventStreamCollection
    {
        private readonly Dictionary<string, IEventStream> _streamWrappers = new Dictionary<string, IEventStream>();
        private readonly AsyncLock _mutex = new AsyncLock();

        internal IEventStoreConnection UnderlyingConnection { get; }

        public IEventStreamCollection Streams { get; }

        /// <summary>
        /// Initializes an <see cref="EventStore"/> from an already connected <see cref="IEventStoreConnection"/>.
        /// </summary>
        /// <param name="underlyingConnection"></param>
        public EventStore(IEventStoreConnection underlyingConnection)
        {
            UnderlyingConnection = underlyingConnection;
        }

        internal async Task DeleteStreamAsync(string name)
        {
            using (await _mutex.LockAsync())
            {
                await UnderlyingConnection.DeleteStreamAsync(name, ExpectedVersion.Any);
                _streamWrappers.Remove(name);
            }
        }

        IEventStream IEventStreamCollection.this[string name]
        {
            get
            {
                using (_mutex.Lock())
                {
                    if (_streamWrappers.TryGetValue(name, out var existingStreamWrapper))
                        return existingStreamWrapper;

                    var streamWrapper = new EventStoreStream(this, name);
                    _streamWrappers[name] = streamWrapper;
                    return streamWrapper;
                }
            }
        }
    }
}
