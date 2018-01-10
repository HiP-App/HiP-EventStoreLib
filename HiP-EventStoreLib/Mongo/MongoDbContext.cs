using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    /// <summary>
    /// Provides read/write access to a Mongo database.
    /// 
    /// Can be used with ASP.NET Core dependency injection (requires
    /// <see cref="MongoDbConfig"/> options and an <see cref="ILogger"/>).
    /// </summary>
    public class MongoDbContext : IMongoDbContext
    {
        private readonly MongoDbConfig _config;
        private readonly ILogger<MongoDbContext> _logger;
        private readonly IMongoDatabase _db;

        public MongoDbContext(IOptions<MongoDbConfig> config, ILogger<MongoDbContext> logger)
        {
            _config = config.Value;
            _logger = logger;

            BsonSerializer.RegisterSerializer(new EntityIdSerializer());

            var mongo = new MongoClient(config.Value.MongoDbHost);

            if (_config.MongoDbDropOnInit)
                mongo.DropDatabase(config.Value.MongoDbName);

            _db = mongo.GetDatabase(config.Value.MongoDbName);

            var uri = new Uri(config.Value.MongoDbHost);
            logger.LogInformation($"Connected to MongoDB cache database on '{uri.Host}', using database '{config.Value.MongoDbName}'");
        }

        public IQueryable<T> GetCollection<T>(ResourceType resourceType) where T : IEntity<int> =>
            _db.GetCollection<T>(resourceType.Name).AsQueryable();

        public T Get<T>(EntityId entity) where T : IEntity<int> =>
            GetCollection<T>(entity.Type).FirstOrDefault(x => x.Id == entity.Id);

        public IReadOnlyList<T> GetMany<T>(ResourceType resourceType, IEnumerable<int> ids) where T: IEntity<int>
        {
            var idSet = ids.ToImmutableHashSet();
            return GetCollection<T>(resourceType).Where(x => idSet.Contains(x.Id)).ToImmutableList();
        }

        public void Add<T>(ResourceType resourceType, T entity) where T : IEntity<int>
        {
            _db.GetCollection<T>(resourceType.Name).InsertOne(entity);
        }

        public bool Delete(EntityId entity)
        {
            var filter = Builders<dynamic>.Filter.Eq("_id", entity.Id);
            var result = _db.GetCollection<dynamic>(entity.Type.Name).DeleteOne(filter);
            return result.DeletedCount == 1;
        }

        public bool Replace<T>(EntityId entity, T newEntity) where T : IEntity<int>
        {
            if (entity.Id != newEntity.Id)
            {
                throw new ArgumentException(
                    $"The ID of the entity to be replaced ('{entity.Id}') and the ID of the replacement " +
                    $"('{newEntity.Id}') do not match", nameof(newEntity));
            }

            var result = _db.GetCollection<T>(entity.Type.Name).ReplaceOne(x => x.Id == entity.Id, newEntity);
            return result.ModifiedCount == 1;
        }

        public bool Update<T>(EntityId entity, Action<IMongoUpdater<T>> updateFunc) where T : IEntity<int>
        {
            var updater = new MongoUpdater<T>();
            updateFunc?.Invoke(updater);

            var result = _db.GetCollection<T>(entity.Type.Name).UpdateOne(x => x.Id == entity.Id, updater.Update);
            return result.ModifiedCount == 1;
        }

        public void AddReference(EntityId source, EntityId target)
        {
            AddReferences(source, Enumerable.Repeat(target, 1));
        }

        private const string OutgoingReferencesKey = "References";
        private const string IncomingReferencesKey = "Referencers";

        public void AddReferences(EntityId source, IEnumerable<EntityId> targets)
        {
            // for each reference (source -> target)...

            // 1) create a new DocRef pointing to the target and add it to the source's references list
            var update = Builders<IEntity<int>>.Update.PushEach(OutgoingReferencesKey, targets);
            var result = _db.GetCollection<IEntity<int>>(source.Type.Name).UpdateOne(x => x.Id == source.Id, update);
            Debug.Assert(result.ModifiedCount == 1);

            // 2) create a new DocRef pointing to the source and add it to the target's referencers list
            var update2 = Builders<IEntity<int>>.Update.Push(IncomingReferencesKey, source);
            foreach (var target in targets)
            {
                result = _db.GetCollection<IEntity<int>>(target.Type.Name).UpdateOne(x => x.Id == target.Id, update2);
                Debug.Assert(result.ModifiedCount == 1);
            }
        }

        public bool RemoveReference(EntityId source, EntityId target)
        {
            // 1) delete the DocRef pointing to the target from the source's references list
            var update = Builders<dynamic>.Update.Pull(OutgoingReferencesKey, target);
            var result = _db.GetCollection<dynamic>(source.Type.Name).UpdateOne(
                Builders<dynamic>.Filter.Eq("_id", source.Id), update);

            Debug.Assert(result.ModifiedCount == 1);

            // 2) delete the DocRef pointing to the source from the target's referencers list
            var update2 = Builders<dynamic>.Update.Pull(IncomingReferencesKey, source);
            var result2 = _db.GetCollection<dynamic>(target.Type.Name).UpdateOne(
                Builders<dynamic>.Filter.Eq("_id", target.Id), update2);

            Debug.Assert(result2.ModifiedCount == 1);

            return result.ModifiedCount == 1 || result2.ModifiedCount == 1;
        }

        public void ClearIncomingReferences(EntityId entity)
        {
            var currentReferencers = _db.GetCollection<dynamic>(entity.Type.Name)
                .Find(Builders<dynamic>.Filter.Eq("_id", entity.Id))
                .First()
                .Referencers;

            foreach (var r in currentReferencers)
            {
                // Note: We must use the internal key "_id" here since we use dynamic objects
                var source = new EntityId(ResourceType.Parse(r.Collection), (int)r._id);
                RemoveReference(source, entity);
            }
        }

        public void ClearOutgoingReferences(EntityId entity)
        {
            var currentReferences = _db.GetCollection<dynamic>(entity.Type.Name)
                .Find(Builders<dynamic>.Filter.Eq("_id", entity.Id))
                .First()
                .References;

            foreach (var r in currentReferences)
            {
                // Note: We must use the internal key "_id" here since we use dynamic objects
                var target = new EntityId(ResourceType.Parse(r.Collection), (int)r._id);
                RemoveReference(entity, target);
            }
        }
    }
}
