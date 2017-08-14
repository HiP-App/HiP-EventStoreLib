using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public interface IEventStream
    {
        event EventAppendedEventHandler Appended;
        string Name { get; }
        Task AppendAsync(IEvent ev);
        Task AppendManyAsync(IEnumerable<IEvent> events);
        Task DeleteAsync();
        EventStreamTransaction BeginTransaction();
        Task<(T value, bool isSuccessful)> TryGetMetadataAsync<T>(string key);
        Task SetMetadataAsync(string key, object value);
        IEventStreamEnumerator GetEnumerator();
        IEventStreamSubscription SubscribeCatchUp();
    }

    public interface IEventStreamSubscription : IDisposable
    {
        IObservable<IEvent> EventAppeared { get; }

        event EventHandler<EventParsingFailedArgs> EventParsingFailed;
    }
}
