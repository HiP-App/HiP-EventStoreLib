using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.FakeStore
{
    /// <summary>
    /// A simple in-memory "event store" for testing purposes.
    /// </summary>
    public class FakeEventStore : IEventStore
    {
        public static readonly FakeEventStore Instance = new FakeEventStore();

        public FakeEventStreamCollection Streams { get; }

        IEventStreamCollection IEventStore.Streams => Streams;

        public FakeEventStore() => Streams = new FakeEventStreamCollection(this);
    }

    public class FakeEventStreamCollection : IEventStreamCollection
    {
        private readonly Dictionary<string, FakeEventStream> _streams = new Dictionary<string, FakeEventStream>();
        private readonly FakeEventStore _eventStore;

        public FakeEventStreamCollection(FakeEventStore eventStore)
        {
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        }

        public FakeEventStream this[string name] => GetOrCreateStream(name);

        IEventStream IEventStreamCollection.this[string name] => GetOrCreateStream(name);

        internal void Delete(string name) => _streams.Remove(name);

        private FakeEventStream GetOrCreateStream(string name)
        {
            if (_streams.TryGetValue(name, out var stream))
                return stream;

            var newStream = new FakeEventStream(_eventStore, name);
            _streams[name] = newStream;
            return newStream;
        }
    }
}
