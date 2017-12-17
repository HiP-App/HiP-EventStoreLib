using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    /// <summary>
    /// Provides methods to construct a Mongo DB update definition
    /// (for updating documents in the database).
    /// </summary>
    public interface IMongoUpdater
    {
        /// <summary>
        /// Sets a field to a specific value.
        /// </summary>
        void Set(string field, object value);

        /// <summary>
        /// Adds a value to a collection.
        /// </summary>
        void Add(string field, object value, CollectionSemantic semantic = CollectionSemantic.Bag);

        /// <summary>
        /// Adds multiple values to a collection.
        /// </summary>
        void AddRange(string field, IEnumerable<object> values, CollectionSemantic semantic = CollectionSemantic.Bag);

        /// <summary>
        /// Removes all occurrences of a value from a collection.
        /// </summary>
        void Remove(string field, object value);

        /// <summary>
        /// Removes all occurrences of the specified values from a collection.
        /// </summary>
        void RemoveRange(string field, IEnumerable<object> values);
    }

    /// <summary>
    /// Provides generic methods to construct a Mongo DB update definition
    /// (for updating documents in the database).
    /// </summary>
    public interface IMongoUpdater<T> : IMongoUpdater
    {
        /// <summary>
        /// Sets a field to a specific value.
        /// </summary>
        void Set<TField>(Expression<Func<T, TField>> field, TField value);

        /// <summary>
        /// Adds a value to a collection.
        /// </summary>
        void Add<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value, CollectionSemantic semantic = CollectionSemantic.Bag);

        /// <summary>
        /// Adds multiple values to a collection.
        /// </summary>
        void AddRange<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, IEnumerable<TItem> values, CollectionSemantic semantic = CollectionSemantic.Bag);

        /// <summary>
        /// Removes all occurrences of a value from a collection.
        /// </summary>
        void Remove<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value);

        /// <summary>
        /// Removes all occurrences of the specified values from a collection.
        /// </summary>
        void RemoveRange<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, IEnumerable<TItem> values);
    }
}
