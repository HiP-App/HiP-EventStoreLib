namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    /// <summary>
    /// The different modes of inserting data in a MongoDB collection.
    /// </summary>
    public enum CollectionSemantic
    {
        /// <summary>
        /// Items are always appended to the collection, duplicates are possible.
        /// </summary>
        Bag,

        /// <summary>
        /// Items are only appended to the collection if they do not already exist.
        /// </summary>
        Set
    }

}
