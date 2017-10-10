using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public interface IEventStream
    {
        /// <summary>
        /// Is raised when an event is added to the event stream.
        /// </summary>
        IObservable<EventAppendedArgs> Appended { get; }

        /// <summary>
        /// The name of the event stream.
        /// </summary>
        string Name { get; }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task AppendAsync(IEvent ev);

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task AppendManyAsync(IEnumerable<IEvent> events);

        /// <summary>
        /// Deletes a stream. This also completes and disposes the <see cref="Appended"/>-observable.
        /// Further read/write operations on this stream cause a <see cref="StreamDeletedException"/>.
        /// To recreate a stream with the same name, obtain a new <see cref="IEventStream"/>-instance
        /// from the <see cref="IEventStore"/>.
        /// </summary>
        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task DeleteAsync();

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        EventStreamTransaction BeginTransaction();

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task<(T value, bool isSuccessful)> TryGetMetadataAsync<T>(string key);

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task SetMetadataAsync(string key, object value);

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        IEventStreamEnumerator GetEnumerator();

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        IEventStreamSubscription SubscribeCatchUp(Action<IEvent> handler);
    }

    public interface IEventStreamSubscription : IDisposable
    {
        event EventHandler<EventParsingFailedArgs> EventParsingFailed;
    }

    public class EventAppendedArgs
    {
        public IEventStream Stream { get; }

        public IEvent Event { get; }

        public EventAppendedArgs(IEventStream stream, IEvent ev)
        {
            Stream = stream;
            Event = ev;
        }
    }
}
