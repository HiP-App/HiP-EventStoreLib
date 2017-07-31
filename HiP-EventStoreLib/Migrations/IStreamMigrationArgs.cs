namespace PaderbornUniversity.SILab.Hip.EventSourcing.Migrations
{
    public interface IStreamMigrationArgs
    {
        IAsyncEnumerator<IEvent> GetExistingEvents();
        void AppendEvent(IEvent ev);
    }
}
