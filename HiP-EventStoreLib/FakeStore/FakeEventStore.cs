using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.FakeStore
{
    /// <summary>
    /// A simple in-memory "event store" for testing purposes.
    /// </summary>
    public class FakeEventStore : IEventStore
    {
        /// <summary>
        /// Gets the latest instance of <see cref="FakeEventStore"/>.
        /// </summary>
        public static FakeEventStore Current { get; private set; }

        public FakeEventStreamCollection Streams { get; }

        IEventStreamCollection IEventStore.Streams => Streams;

        /// <summary>
        /// Initializes a new <see cref="FakeEventStore"/> and sets it as the
        /// current instance (see <see cref="Current"/>).
        /// </summary>
        public FakeEventStore()
        {
            Current = this;
            Streams = new FakeEventStreamCollection(this);
        }
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
