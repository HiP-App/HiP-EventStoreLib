﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// This class can be used to create, delete and update entities. The necessary events are appended to the event stream.
    /// </summary>
    public static class EntityManager
    {
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
        public static async Task CreateEntity<T>(EventStoreService service, T obj, ResourceType resourceType, int id, string userId) where T : new()
        {
            if (obj == null)
                throw new ArgumentException("The object to store cannot be null", nameof(obj));

            var emptyObject = Activator.CreateInstance<T>();
            var createdEvent = new CreatedEvent(resourceType.Name, id, userId);
            await service.AppendEventAsync(createdEvent);
            await UpdateEntity(service, emptyObject, obj, resourceType, id, userId);
        }

        /// <summary>
        /// Deletes an entity by appendeing a <see cref="DeletedEvent"/> to the event stream.
        /// </summary>
        /// <param name="service">EventStoreService</param>
        /// <param name="resourceType">Resource type</param>
        /// <param name="id">Id of the entity</param>
        /// <param name="userId">Id of the user</param>
        /// <returns></returns>
        public static async Task DeleteEntity(EventStoreService service, ResourceType resourceType, int id, string userId)
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
        public static async Task UpdateEntity<T>(EventStoreService service, T oldObject, T newObject, ResourceType resourceType, int id, string userId)
        {
            var events = CompareEntities(oldObject, newObject, resourceType, id, userId);
            await service.AppendEventsAsync(events);
        }

        /// <summary>
        /// Compares two entites to each other and returns an enumerable of <see cref="PropertyChangedEvent"/>.
        /// The comparison is based on the readable public properties of type <typeparamref name="T"/>. 
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
            if (oldObject == null || newObject == null)
                throw new ArgumentNullException("None of the objects to compare can be null");

            if (resourceType == null)
                throw new ArgumentNullException("A valid ResourceType has to be provided", nameof(resourceType));

            var properties = typeof(T).GetProperties().Where(p => p.CanRead);
            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldObject);
                var newValue = prop.GetValue(newObject);

                var type = oldValue?.GetType() ?? newValue?.GetType();

                // both values are null
                if (type == null) continue;

                if (type == typeof(string) && !Equals(oldValue, newValue))
                {
                    yield return new PropertyChangedEvent(prop.Name, resourceType.Name, id, userId, newValue);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    var oldList = ((IEnumerable)oldValue)?.Cast<object>();
                    var newList = ((IEnumerable)newValue)?.Cast<object>();

                    if (oldList == null || newList == null || !oldList.SequenceEqual(newList))
                    {
                        yield return new PropertyChangedEvent(prop.Name, resourceType.Name, id, userId, newValue);
                    }
                }
                else if (!Equals(oldValue, newValue))
                {
                    yield return new PropertyChangedEvent(prop.Name, resourceType.Name, id, userId, newValue);
                }
            }
        }
    }
}
