namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    public class CreatedEvent : BaseEvent
    {
        public CreatedEvent(string resourceTypeName, int id, string userId) : base(resourceTypeName, id, userId)
        {
        }        
    }
}
