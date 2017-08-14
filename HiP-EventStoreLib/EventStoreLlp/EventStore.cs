using System.Collections.Generic;
using EventStore.ClientAPI;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public class EventStore : IEventStore, IEventStreamCollection
    {
        private readonly Dictionary<string, IEventStream> _streamWrappers = new Dictionary<string, IEventStream>();
        private readonly SemaphoreSlim _sema = new SemaphoreSlim(1);

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
            await _sema.WaitAsync();

            try
            {
                await UnderlyingConnection.DeleteStreamAsync(name, ExpectedVersion.Any);
                _streamWrappers.Remove(name);
            }
            finally
            {
                _sema.Release();
            }
        }

        IEventStream IEventStreamCollection.this[string name]
        {
            get
            {
                _sema.Wait();

                try
                {
                    if (_streamWrappers.TryGetValue(name, out var existingStreamWrapper))
                        return existingStreamWrapper;

                    var streamWrapper = new EventStoreStream(this, name);
                    _streamWrappers[name] = streamWrapper;
                    return streamWrapper;
                }
                finally
                {
                    _sema.Release();
                }
            }
        }
    }
}
