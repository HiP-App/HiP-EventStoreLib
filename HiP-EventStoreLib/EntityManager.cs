using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// This class can be used to create, delete and update entities. The necessary events are appended to the event stream.
    /// </summary>
    public static class EntityManager
    {
        private static readonly Dictionary<string, IEnumerable<PropertyInfo>> _propertiesDict = new Dictionary<string, IEnumerable<PropertyInfo>>();

        /// <summary>
        /// Creates an entity by appending a <see cref="CreatedEvent"/> and the necessary
        /// <see cref="PropertyChangedEvent"/>s to the event stream.
        /// </summary>
        /// <typeparam name="T">Type of the entity</typeparam>
        /// <param name="service">EventStoreService</param>
        /// <param name="obj">Object which contains the values</param>
        /// <param name="resourceType">Resource type of the object</param>
        /// <param name="id">Id of the object</param>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public static async Task CreateEntityAsync<T>(EventStoreService service, T obj, ResourceType resourceType, int id, string userId) where T : new()
        {
            if (obj == null)
                throw new ArgumentException("The object to store cannot be null", nameof(obj));

            var emptyObject = Activator.CreateInstance<T>();
            var createdEvent = new CreatedEvent(resourceType.Name, id, userId);
            await service.AppendEventAsync(createdEvent);
            var events = CompareEntitiesInternal(emptyObject, obj, resourceType, id, userId, "", 1, true);
            await service.AppendEventsAsync(events);
        }

        /// <summary>
        /// Deletes an entity by appendeing a <see cref="DeletedEvent"/> to the event stream.
        /// </summary>
        /// <param name="service">EventStoreService</param>
        /// <param name="resourceType">Resource type</param>
        /// <param name="id">Id of the entity</param>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public static async Task DeleteEntityAsync(EventStoreService service, ResourceType resourceType, int id, string userId)
        {
            await service.AppendEventAsync(new DeletedEvent(resourceType.Name, id, userId));
        }

        /// <summary>
        /// Updates an entity by appending <see cref="PropertyChangedEvent"/>s to the event stream.
        /// Uses <see cref="CompareEntities{T}(T, T, ResourceType, int, string)"/> to compare the entities.
        /// </summary>
        /// <typeparam name="T">Type of the entitiy</typeparam>
        /// <param name="service">EventStoreService</param>
        /// <param name="oldObject">Old object</param>
        /// <param name="newObject">New object</param>
        /// <param name="resourceType">Resource type</param>
        /// <param name="id">Id of the entity</param>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public static async Task UpdateEntityAsync<T>(EventStoreService service, T oldObject, T newObject, ResourceType resourceType, int id, string userId)
        {
            var events = CompareEntities(oldObject, newObject, resourceType, id, userId);
            await service.AppendEventsAsync(events);
        }

        /// <summary>
        /// Compares two entites to each other and returns an enumerable of <see cref="PropertyChangedEvent"/>.
        /// The comparison is based on the readable public properties of type <typeparamref name="T"/>.
        /// If a property has a <see cref="NestedObjectAttribute"/> the properties of these object will be compared recursively
        /// </summary>
        /// <typeparam name="T">Type of both entities</typeparam>
        /// <param name="oldObject">Old entity</param>
        /// <param name="newObject">New entity</param>
        /// <param name="resourceType">Resource type</param>
        /// <param name="id">Id of the entity</param>
        /// <param name="userId">Id of the user</param>        
        /// <returns>Enumerable of <see cref="PropertyChangedEvent"/>s</returns>

        public static IEnumerable<PropertyChangedEvent> CompareEntities<T>(T oldObject, T newObject, ResourceType resourceType, int id, string userId)
        {
            return CompareEntitiesInternal(oldObject, newObject, resourceType, id, userId, "", 1);
        }

        private static IEnumerable<PropertyChangedEvent> CompareEntitiesInternal<T>(T oldObject, T newObject, ResourceType resourceType, int id, string userId, string propertyPath, int recursionDepth, bool entityCreated = false)
        {
            if (oldObject == null || newObject == null)
                throw new ArgumentNullException("None of the objects to compare can be null");

            if (resourceType == null)
                throw new ArgumentNullException("A valid ResourceType has to be provided", nameof(resourceType));

            if (!_propertiesDict.TryGetValue(typeof(T).Name, out var properties))
            {
                properties = typeof(T).GetProperties().Where(p => p.CanRead);
                _propertiesDict[typeof(T).Name] = properties;
            }

            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldObject);
                var newValue = prop.GetValue(newObject);

                var type = oldValue?.GetType() ?? newValue?.GetType();

                // both values are null
                if (type == null) continue;

                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    //this also holds for strings
                    var oldList = ((IEnumerable)oldValue)?.Cast<object>();
                    var newList = ((IEnumerable)newValue)?.Cast<object>();

                    if (oldList == null || newList == null || !oldList.SequenceEqual(newList))
                    {
                        yield return new PropertyChangedEvent(BuildPath(propertyPath, prop.Name), resourceType.Name, id, userId, newValue);
                    }
                }
                else if (prop.CustomAttributes.Any(d => d.AttributeType.Equals(typeof(NestedObjectAttribute))))
                {
                    if (recursionDepth >= 50) throw new InvalidOperationException("The NestedObject graph seems to contain a cycle");

                    if (newValue == null)
                    {
                        //the nested object has been set to null
                        yield return new PropertyChangedEvent(BuildPath(propertyPath, prop.Name), resourceType.Name, id, userId, newValue);
                    }
                    else
                    {
                        if (oldValue == null)
                        {
                            if (!type.HasEmptyConstructor()) throw new InvalidOperationException("The property type where the NestedObjectAttribute is used must have an empty constructor");
                            oldValue = Activator.CreateInstance(type, true);
                        }

                        var methodInfo = typeof(EntityManager).GetMethod(nameof(CompareEntitiesInternal), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        var genericMethod = methodInfo.MakeGenericMethod(oldValue.GetType());
                        var events = (IEnumerable<PropertyChangedEvent>)genericMethod.Invoke(null, new[] { oldValue, newValue, resourceType, id, userId, BuildPath(propertyPath, prop.Name), ++recursionDepth, entityCreated });

                        foreach (var e in events)
                        {
                            yield return e;
                        }
                    }
                }
                else if (entityCreated && type.IsValueType)
                {
                    yield return new PropertyChangedEvent(BuildPath(propertyPath, prop.Name), resourceType.Name, id, userId, newValue);
                }
                else if (!Equals(oldValue, newValue))
                {
                    yield return new PropertyChangedEvent(BuildPath(propertyPath, prop.Name), resourceType.Name, id, userId, newValue);
                }
            }
        }

        /// <summary>
        /// Checks if <paramref name="type"/> has an empty constructor
        /// </summary>
        /// <returns></returns>
        public static bool HasEmptyConstructor(this Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Builds the property path which is used for the property name
        /// </summary>
        /// <returns></returns>
        private static string BuildPath(string path, string propertyName) => string.IsNullOrEmpty(path) ? propertyName : $"{path}.{propertyName}";

    }
}
