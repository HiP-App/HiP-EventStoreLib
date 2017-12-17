namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    public class MongoDbConfig
    {
        /// <summary>
        /// Connection string for the Mongo DB database.
        /// Default value: "mongodb://localhost:27017"
        /// </summary>
        public string MongoDbHost { get; set; } = "mongodb://localhost:27017";

        /// <summary>
        /// Name of the database to use.
        /// Default value: "main"
        /// </summary>
        public string MongoDbName { get; set; } = "main";

        /// <summary>
        /// Specifies whether the database should be deleted on each application startup.
        /// Default value: true
        /// </summary>
        public bool MongoDbDropOnInit { get; set; } = true;
    }

}
