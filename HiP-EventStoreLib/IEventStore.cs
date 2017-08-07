using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public interface IEventStore
    {
        IEventStreamCollection Streams { get; }
    }

    public interface IEventStreamCollection
    {
        IEventStream this[string name] { get; }
    }

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
        IAsyncEnumerator<IEvent> GetEnumerator();
    }

    public delegate void EventAppendedEventHandler(IEventStream sender, IEvent ev);
}
