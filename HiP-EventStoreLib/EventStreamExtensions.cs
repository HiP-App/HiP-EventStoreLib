using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using System;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public static class EventStreamExtensions
    {
        /// <summary>
        /// Creates the current state of an entity from the event stream
        /// </summary>
        /// <typeparam name="T">Type of the resulting object</typeparam>
        /// <param name="stream">Event stream</param>
        /// <param name="resourceType">Resource type</param>
        /// <param name="id"></param>
        /// <returns>The resulting object</returns>
        public static async Task<T> GetCurrentObjectFromEventStream<T>(this IEventStream stream, ResourceType resourceType, int id) where T : new()
        {
            if (ResourceType.ResourceTypeDictionary.ContainsKey(resourceType.Name))
            {
                var targetType = typeof(T);
                if (!resourceType.Type.Equals(targetType)) throw new ArgumentException("The type parameter doesn't match up with the associated type of the ResourceType");

                var enumerator = stream.GetEnumerator();
                T obj = default(T);

                while (await enumerator.MoveNextAsync())
                {
                    switch (enumerator.Current)
                    {
                        case CreatedEvent createdEv:
                            if (createdEv.ResourceTypeName.Equals(resourceType.Name) && createdEv.Id == id)
                                obj = Activator.CreateInstance<T>();
                            break;

                        case PropertyChangedEvent propertyEv:
                            if (!obj.Equals(default(T)) && Equals(propertyEv.ResourceTypeName, resourceType.Name) && propertyEv.Id == id)
                            {
                                var propertyInfo = targetType.GetProperty(propertyEv.PropertyName);
                                propertyInfo.SetValue(obj, propertyEv.Value);
                            }
                            break;
                    }
                }

                return obj;
            }
            else
            {
                throw new ArgumentException("A resource type with the given name does not exist");
            }
        }
    }
}
