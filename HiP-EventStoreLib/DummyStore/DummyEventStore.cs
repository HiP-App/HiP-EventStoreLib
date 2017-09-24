using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.DummyStore
{
    /// <summary>
    /// A simple in-memory "event store" for testing purposes.
    /// </summary>
    public class DummyEventStore : IEventStore, IEventStreamCollection
    {
        private readonly Dictionary<string, DummyEventStream> _streams = new Dictionary<string, DummyEventStream>();

        public IEventStreamCollection Streams => this;

        public IEventStream this[string name] => GetOrCreateStream(name);

        internal void DeleteStream(string name) => _streams.Remove(name);

        private DummyEventStream GetOrCreateStream(string name)
        {
            if (_streams.TryGetValue(name, out var stream))
                return stream;

            var newStream = new DummyEventStream(this, name);
            _streams[name] = newStream;
            return newStream;
        }
    }
}
