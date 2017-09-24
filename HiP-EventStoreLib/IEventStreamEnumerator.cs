using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// The exception that is thrown when raw event data cannot be deserialized into a corresponding CLR object.
    /// </summary>
    public interface IEventStreamEnumerator : IAsyncEnumerator<IEvent>
    {
        event EventHandler<EventParsingFailedArgs> EventParsingFailed;
    }

    public class EventParsingFailedArgs
    {
        /// <summary>
        /// The raw event data that could not be deserialized.
        /// The actual type depends on the backing event store technology.
        /// </summary>
        public object RawEventData { get; }

        /// <summary>
        /// The exception (if any) that was raised when trying to deserialize the event.
        /// </summary>
        public Exception Exception { get; }

        public EventParsingFailedArgs(object rawEventData, Exception exception = null)
        {
            RawEventData = rawEventData;
            Exception = exception;
        }
    }
}
