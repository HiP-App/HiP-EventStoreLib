using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Migrations
{
    public class StreamMigrationArgs : IStreamMigrationArgs
    {
        private readonly IEventStream _stream;
        private readonly List<IEvent> _eventsToAppend = new List<IEvent>();

        public IReadOnlyList<IEvent> EventsToAppend => _eventsToAppend;

        public StreamMigrationArgs(IEventStream stream)
        {
            _stream = stream;
        }

        public void AppendEvent(IEvent ev) => _eventsToAppend.Add(ev);

        public IAsyncEnumerator<IEvent> GetExistingEvents() => _stream.GetEnumerator();
    }
}
