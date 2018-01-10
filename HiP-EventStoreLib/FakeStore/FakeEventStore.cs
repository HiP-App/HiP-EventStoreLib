using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.FakeStore
{
    /// <summary>
    /// A simple in-memory "event store" for testing purposes.
    /// </summary>
    public class FakeEventStore : IEventStore, IEventStreamCollection
    {
        private readonly Dictionary<string, FakeEventStream> _streams = new Dictionary<string, FakeEventStream>();

        public IEventStreamCollection Streams => this;

        public IEventStream this[string name] => GetOrCreateStream(name);

        internal void DeleteStream(string name) => _streams.Remove(name);

        private FakeEventStream GetOrCreateStream(string name)
        {
            if (_streams.TryGetValue(name, out var stream))
                return stream;

            var newStream = new FakeEventStream(this, name);
            _streams[name] = newStream;
            return newStream;
        }
    }
}
