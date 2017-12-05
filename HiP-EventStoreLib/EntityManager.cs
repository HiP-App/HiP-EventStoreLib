using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public static class EntityManager
    {
        public static async Task CreateEntity<T>(EventStoreService service, T obj, ResourceType resourceType, int id, string userId) where T : new()
        {
            var emptyObject = Activator.CreateInstance<T>();
            var createdEvent = new CreatedEvent(resourceType.Name, id, userId);
            await service.AppendEventAsync(createdEvent);
            await CompareAndAddEvents<T>(service, emptyObject, obj, resourceType, id, userId);
        }



        public static async Task DeleteEntity(EventStoreService service, ResourceType resourceType, int id, string userId)
        {
            await service.AppendEventAsync(new DeletedEvent(resourceType.Name, id, userId));
        }

        public static async Task CompareAndAddEvents<T>(EventStoreService service, T oldObject, T newObject, ResourceType resourceType, int id, string userId)
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
            }
        }
    }
}
