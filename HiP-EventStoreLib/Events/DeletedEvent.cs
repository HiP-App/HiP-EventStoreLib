namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    /// <summary>
    /// Event that is used to delete entities
    /// </summary>
    public class DeletedEvent : BaseEvent
    {
        public DeletedEvent(string resourceTypeName, int id, string userId) : base(resourceTypeName, id, userId)
        {
        }
    }
}
