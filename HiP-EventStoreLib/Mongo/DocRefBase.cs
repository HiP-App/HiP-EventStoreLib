﻿using MongoDB.Bson.Serialization.Attributes;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    /// <summary>
    /// Base class for <see cref="DocRef{T}"/> and <see cref="DocRefList{T}"/>.
    /// </summary>
    public abstract class DocRefBase
    {
        /// <summary>
        /// The name of the collection where the referenced document is. This property is optional:
        /// If not set, the referenced document is assumed to be in the same collection as the referencing document.
        /// </summary>
        [BsonElement]
        public string Collection { get; private set; }

        /// <summary>
        /// The name of the database where the referenced document is. This property is optional:
        /// If not set, the referenced document is assumed to be in the same database as the referencing document.
        /// </summary>
        [BsonElement]
        public string Database { get; private set; }

        protected DocRefBase()
        {
        }

        protected DocRefBase(string collection, string database)
        {
            Collection = collection;
            Database = database;
        }
    }
}
