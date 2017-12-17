using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    public static class DocRefExtensions
    {
        /// <summary>
        /// Resolves a reference to a document.
        /// Collection name of the referenced document must be specified, database name is ignored.
        /// </summary>
        public static T Load<T>(this DocRef<T> docRef, IMongoDbContext db) where T : IEntity<int>
        {
            if (string.IsNullOrEmpty(docRef.Collection))
                throw new InvalidOperationException($"The DocRef does not specify the collection of the referenced document");

            return db.Get<T>((ResourceType.Parse(docRef.Collection), (int)docRef.Id));
        }

        /// <summary>
        /// Resolves a collection of document references.
        /// Collection name of the referenced documents must be specified, database name is ignored.
        /// </summary>
        public static IReadOnlyCollection<T> LoadAll<T>(this DocRefList<T> docRef, IMongoDbContext db) where T : IEntity<int>
        {
            if (string.IsNullOrEmpty(docRef.Collection))
                throw new InvalidOperationException($"The DocRef does not specify the collection of the referenced document");

            // Less efficient approach, preserves ordering
            return docRef.Ids
                .Select(id => db.Get<T>((ResourceType.Parse(docRef.Collection), (int)id)))
                .ToList();

            // Simple, efficient approach (unfortunately doesn't preserve ordering):
            // return collection.Find(Builders<T>.Filter.In("_id", docRef.Ids)).ToList();
        }
    }
}
