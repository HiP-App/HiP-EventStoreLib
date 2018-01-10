using System;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    /// <summary>
    /// Provides read/write access to a Mongo database.
    /// Each <see cref="ResourceType"/> corresponds to one collection in the database.
    /// </summary>
    public interface IMongoDbContext
    {
        IQueryable<T> GetCollection<T>(ResourceType resourceType) where T : IEntity<int>;

        /// <summary>
        /// Retrieves the entity with the specified ID from the database.
        /// If no entity with that ID exists, the default value of <typeparamref name="T"/>
        /// is returned (which is usually null).
        /// </summary>
        T Get<T>(EntityId entity) where T : IEntity<int>;

        /// <summary>
        /// Retrieves the entities with the specified IDs from the database.
        /// IDs for which no entity exists are ignored, so the number of returned entities may be
        /// smaller than the number of specified IDs.
        /// </summary>
        IReadOnlyList<T> GetMany<T>(ResourceType resourceType, IEnumerable<int> ids) where T : IEntity<int>;

        /// <summary>
        /// Inserts an entity with the specified ID into the database.
        /// Throws an exception if an entity of the same ID already exists.
        /// </summary>
        void Add<T>(ResourceType resourceType, T entity) where T : IEntity<int>;

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <returns>True if the entity was found and deleted</returns>
        bool Delete(EntityId entity);

        /// <summary>
        /// Replaces an entity with another one. The IDs of the existing entity
        /// and the replacement must match, otherwise an exception is thrown.
        /// </summary>
        /// <returns>True if the original entity was found and replaced</returns>
        bool Replace<T>(EntityId entity, T newEntity) where T : IEntity<int>;

        /// <summary>
        /// Updates specific properties of an entity.
        /// </summary>
        /// <returns>True if the entity was found and updated</returns>
        bool Update<T>(EntityId entity, Action<IMongoUpdater<T>> updateFunc) where T : IEntity<int>;

        /// <summary>
        /// Creates a reference from <paramref name="source"/> to <paramref name="target"/>.
        /// Duplicate references are not created: If the reference to be added already exists, the
        /// method does nothing.
        /// </summary>
        void AddReference(EntityId source, EntityId target);

        void AddReferences(EntityId source, IEnumerable<EntityId> targets);

        bool RemoveReference(EntityId source, EntityId target);

        /// <summary>
        /// Removes all references from other entities to the specified <paramref name="entity"/>.
        /// </summary>
        void ClearIncomingReferences(EntityId entity);

        /// <summary>
        /// Removes all references from the specified <paramref name="entity"/> to other entities.
        /// </summary>
        void ClearOutgoingReferences(EntityId entity);
    }
}
