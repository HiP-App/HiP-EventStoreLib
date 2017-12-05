using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// This class can be used to create, delete and update entities. The necessary events are appended to the event stream
    /// </summary>
    public static class EntityManager
    {
        /// <summary>
        /// Creates an entity be appending a CreatedEvent and the necessary PropertyChanged events to the event stream
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
            var emptyObject = Activator.CreateInstance<T>();
            var createdEvent = new CreatedEvent(resourceType.Name, id, userId);
            await service.AppendEventAsync(createdEvent);
            await UpdateEntity(service, emptyObject, obj, resourceType, id, userId);
        }

        /// <summary>
        /// Deletes an entity
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
        /// Updates an entity be appending PropertyChanged events to the event stream
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

        public static IEnumerable<PropertyChangedEvent> CompareEntities<T>(T oldObject, T newObject, ResourceType resourceType, int id, string userId)
        {
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                var oldValue = prop.GetValue(oldObject);
                var newValue = prop.GetValue(newObject);

                var type = oldValue?.GetType() ?? newValue?.GetType();

                //both values are null
                if (type == null) continue;
                //ReSharper disable All
                //the value was null before was set to a new value
                if (oldValue == null && newValue != null)
                {
                    yield return new PropertyChangedEvent(prop.Name, resourceType.Name, id, userId, newValue);
                }
                else if (type == typeof(string) && !Equals(oldValue, newValue))
                {
                    yield return new PropertyChangedEvent(prop.Name, resourceType.Name, id, userId, newValue);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    var oldList = ((IEnumerable)oldValue)?.Cast<object>();
                    var newList = ((IEnumerable)newValue)?.Cast<object>();

                    if (oldList == null || newList == null)
                    {
                        yield return new PropertyChangedEvent(prop.Name, resourceType.Name, id, userId, newValue);
                    }
                    else if (!oldList.SequenceEqual(newList))
                    {
                        yield return new PropertyChangedEvent(prop.Name, resourceType.Name, id, userId, newValue);
                    }
                }
                else if (!oldValue.Equals(newValue))
                {
                    yield return new PropertyChangedEvent(prop.Name, resourceType.Name, id, userId, newValue);
                }

                //ReSharper enable All
            }
        }
    }
}
