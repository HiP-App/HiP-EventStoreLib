namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// An "index" caches specific parts of the domain model for efficient access during validation.
    /// </summary>
    public interface IDomainIndex
    {
        void ApplyEvent(IEvent e);
    }
}
