namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public interface IEntity<TKey>
    {
        TKey Id { get; }
    }
}
