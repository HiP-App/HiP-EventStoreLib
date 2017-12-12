namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    /// <summary>
    /// Specifies the configuration fields that are required for <see cref="EventStoreService"/> to work.
    /// </summary>
    public sealed class EventStoreConfig
    {
        /// <summary>
        /// Endpoint of the Event Store.
        /// Default value: "tcp://localhost:1113"
        /// 
        /// Examples:
        /// "tcp://localhost:1113",
        /// "tcp://user:password@myserver:11234",
        /// "discover://user:password@myserver:1234"
        /// 
        /// See also: http://docs.geteventstore.com/dotnet-api/4.0.0/connecting-to-a-server/
        /// </summary>
        public string Host { get; set; } = "tcp://localhost:1113";

        /// <summary>
        /// Name of the event stream to read from and write to.
        /// For example, you can use different streams for develop and production environments.
        /// </summary>
        public string Stream { get; set; }
    }
}
