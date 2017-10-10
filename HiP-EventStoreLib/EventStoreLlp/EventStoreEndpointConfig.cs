namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    /// <summary>
    /// Specifies the configuration fields that are required for <see cref="EventStoreService"/> to work.
    /// </summary>
    public class EventStoreEndpointConfig
    {
        public string EventStoreHost { get; set; }

        public string EventStoreStream { get; set; }
    }
}
