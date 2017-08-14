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

    public delegate void EventAppendedEventHandler(IEventStream sender, IEvent ev);
}
