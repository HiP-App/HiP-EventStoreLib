namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    /// <summary>
    /// Event that is used to create entities
    /// </summary>
    public class CreatedEvent : BaseEvent
    {
        public CreatedEvent(string resourceTypeName, int id, string userId) : base(resourceTypeName, id, userId)
        {
        }        
    }
}
