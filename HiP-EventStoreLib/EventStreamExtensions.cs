using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using System;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public static class EventStreamExtensions
    {
        /// <summary>
        /// Creates the current state of an entity from the event stream.
        /// </summary>
        /// <typeparam name="T">Type of the resulting entity</typeparam>
        /// <param name="stream">Event stream</param>
        /// <param name="resourceType">Resource type</param>
        /// <param name="id"></param>
        /// <returns>The resulting entity</returns>
        public static async Task<T> GetCurrentEntityAsync<T>(this IEventStream stream, ResourceType resourceType, int id) where T : class, new()
        {
            var targetType = typeof(T);
            if (!targetType.IsAssignableFrom(resourceType.Type)) throw new ArgumentException("The type parameter doesn't match up with the associated type of the ResourceType");

            var enumerator = stream.GetEnumerator();
            var obj = default(T);

            while (await enumerator.MoveNextAsync())
            {
                switch (enumerator.Current)
                {
                    case CreatedEvent createdEv:
                        if (createdEv.ResourceTypeName.Equals(resourceType.Name) && createdEv.Id == id)
                            obj = Activator.CreateInstance<T>();
                        break;

                    case PropertyChangedEvent propertyEv:
                        if (!Equals(obj, default(T)) && Equals(propertyEv.ResourceTypeName, resourceType.Name) && propertyEv.Id == id)
                        {
                            var propertyInfo = targetType.GetProperty(propertyEv.PropertyName);
                            propertyInfo.SetValue(obj, propertyEv.Value);
                        }
                        break;

                    case DeletedEvent deletedEv:
                        if (Equals(deletedEv.ResourceTypeName, resourceType.Name) && deletedEv.Id == id)
                            obj = default(T); // entity might be recreated later
                        break;
                }
            }

            return obj;
        }
    }
}
