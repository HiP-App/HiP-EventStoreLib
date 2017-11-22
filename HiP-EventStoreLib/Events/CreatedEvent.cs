namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    public class CreatedEvent : EventBase
    {
        public CreatedEvent(string resourceTypeName, int id, string userId) : base(resourceTypeName, id, userId)
        {
        }        
    }
}
