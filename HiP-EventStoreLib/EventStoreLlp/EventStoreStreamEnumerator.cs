using System.Collections.Generic;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using System;
using PaderbornUniversity.SILab.Hip.EventSourcing.Migrations;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    /// <summary>
    /// Provides an easy-to-consume async enumerator to iterate over all events in an Event Store stream.
    /// </summary>
    public class EventStoreStreamEnumerator : IEventStreamEnumerator
    {
        private const int PageSize = 4096; // only 4096 events can be retrieved in one call

        private readonly IEventStoreConnection _connection;
        private readonly string _streamName;
        private readonly Queue<IEvent> _buffer = new Queue<IEvent>();

        private long _startPosition = StreamPosition.Start;
        private bool _isEndOfStream;

        public IEvent Current => _buffer.Peek();

        public event EventHandler<EventParsingFailedArgs> EventParsingFailed;

        public EventStoreStreamEnumerator(IEventStoreConnection connection, string streamName)
        {
            _connection = connection;
            _streamName = streamName;
        }

        public async Task<bool> MoveNextAsync()
        {
            if (_buffer.Count > 0)
            {
                _buffer.Dequeue();

                if (_buffer.Count > 0)
                    return true;
            }

            if (_isEndOfStream)
                return false;

            // Why the 'while'? If the stream was soft-deleted in the past it starts at an event with number != 0.
            // So the first read operation returns 0 events, but reports the actual start position of the stream
            // (currentSlice.NextEventNumber). Then, a second read is required to actually read the first few events.
            while (_buffer.Count == 0 && !_isEndOfStream)
            {
                var currentSlice = await _connection.ReadStreamEventsForwardAsync(_streamName, _startPosition, PageSize, false);

                foreach (var eventData in currentSlice.Events)
                {
                    try
                    {
                        var ev = eventData.Event.ToIEvent().MigrateToLatestVersion();
                        _buffer.Enqueue(ev);
                    }
                    catch (ArgumentException e)
                    {
                        EventParsingFailed?.Invoke(this, new EventParsingFailedArgs(eventData, e));
                    }
                }

                _startPosition = currentSlice.NextEventNumber;
                _isEndOfStream = currentSlice.IsEndOfStream;
            }

            return _buffer.Count > 0;
        }

        public void Reset()
        {
            _startPosition = 0;
            _isEndOfStream = false;
            _buffer.Clear();
        }
    }
}
