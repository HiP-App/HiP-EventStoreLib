using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo.Test
{
    /// <summary>
    /// Simulates a fake, in-memory Mongo database that can be used in testing scenarios.
    /// </summary>
    /// <remarks>
    /// In terms of runtime behavior, the fake implementation is not 100% equivalent to the
    /// original implementation - for example, the methods may throw different exceptions.
    /// </remarks>
    public class FakeMongoDbContext : IMongoDbContext
    {
        /// <summary>
        /// Gets the latest instance of <see cref="FakeMongoDbContext"/>.
        /// </summary>
        public static FakeMongoDbContext Current { get; private set; }

        private readonly Dictionary<EntityId, IEntity<int>> _entities = new Dictionary<EntityId, IEntity<int>>();
        private readonly Dictionary<EntityId, HashSet<EntityId>> _incomingRefs = new Dictionary<EntityId, HashSet<EntityId>>();
        private readonly Dictionary<EntityId, HashSet<EntityId>> _outgoingRefs = new Dictionary<EntityId, HashSet<EntityId>>();

        /// <summary>
        /// Initializes a new <see cref="FakeMongoDbContext"/> and sets it as the
        /// current instance (see <see cref="Current"/>).
        /// </summary>
        public FakeMongoDbContext()
        {
            Current = this;
        }

        private HashSet<EntityId> IncomingRefs(EntityId id) =>
            _incomingRefs.TryGetValue(id, out var list)
                ? list
                : _incomingRefs[id] = new HashSet<EntityId>();

        private HashSet<EntityId> OutgoingRefs(EntityId id) =>
            _outgoingRefs.TryGetValue(id, out var list)
                ? list
                : _outgoingRefs[id] = new HashSet<EntityId>();

        public void Add<T>(ResourceType resourceType, T entity) where T : IEntity<int>
        {
            _entities.Add((resourceType, entity.Id), entity);
        }

        public void AddReference(EntityId source, EntityId target)
        {
            if (!_entities.ContainsKey(source) || !_entities.ContainsKey(target))
                return;

            IncomingRefs(target).Add(source);
            OutgoingRefs(source).Add(target);
        }

        public void AddReferences(EntityId source, IEnumerable<EntityId> targets)
        {
            foreach (var target in targets)
                AddReference(source, target);
        }

        public void ClearIncomingReferences(EntityId entity)
        {
            foreach (var source in IncomingRefs(entity).ToList())
                RemoveReference(source, entity);
        }

        public void ClearOutgoingReferences(EntityId entity)
        {
            foreach (var target in OutgoingRefs(entity).ToList())
                RemoveReference(entity, target);
        }

        public bool Delete(EntityId entity)
        {
            if (_entities.Remove(entity))
            {
                _incomingRefs.Remove(entity);
                _outgoingRefs.Remove(entity);
                return true;
            }
            return false;
        }

        public T Get<T>(EntityId entity) where T : IEntity<int>
        {
            return _entities.TryGetValue(entity, out var o) ? (T)o : default(T);
        }

        public IReadOnlyList<T> GetMany<T>(ResourceType resourceType, IEnumerable<int> ids) where T : IEntity<int>
        {
            var idSet = ids.ToImmutableHashSet();
            return GetCollection<T>(resourceType).Where(x => idSet.Contains(x.Id)).ToImmutableList();
        }

        public IQueryable<T> GetCollection<T>(ResourceType resourceType) where T : IEntity<int>
        {
            return _entities
                .Where(pair => pair.Key.Type == resourceType)
                .Select(pair => pair.Value)
                .Cast<T>()
                .AsQueryable();
        }

        public bool RemoveReference(EntityId source, EntityId target)
        {
            if (_entities.ContainsKey(source) && _entities.ContainsKey(target))
            {
                OutgoingRefs(source).Remove(target);
                IncomingRefs(target).Remove(source);
                return true;
            }
            return false;
        }

        public bool Replace<T>(EntityId entity, T newEntity) where T : IEntity<int>
        {
            if (entity.Id != newEntity.Id)
            {
                throw new ArgumentException(
                    $"The ID of the entity to be replaced ('{entity.Id}') and the ID of the replacement " +
                    $"('{newEntity.Id}') do not match", nameof(newEntity));
            }

            if (_entities.ContainsKey(entity))
            {
                _entities[entity] = newEntity;
                return true;
            }

            return false;
        }

        public bool Update<T>(EntityId entityId, Action<IMongoUpdater<T>> updateFunc) where T : IEntity<int>
        {
            if (!_entities.TryGetValue(entityId, out var entity))
                return false;

            var updater = new FakeMongoUpdater<T>();
            updateFunc?.Invoke(updater);

            foreach (var action in updater.UpdateActions)
                action((T)entity);

            return true;
        }
    }
}
