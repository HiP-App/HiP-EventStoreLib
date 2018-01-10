using EventStore.ClientAPI;
using Newtonsoft.Json;
using PaderbornUniversity.SILab.Hip.EventSourcing.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public static class EventDataExtensions
    {
        private const string EventClrTypeHeader = "ClrType";
        private static readonly JsonSerializerSettings HeaderSerializationSettings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.DateTimeOffset };
        private static readonly Dictionary<string, object> NoHeaders = new Dictionary<string, object>();

        /// <summary>
        /// Converts an <see cref="IEvent"/> object to EventStore's <see cref="EventData"/>.
        /// </summary>
        public static EventData ToEventData(this IEvent ev, Guid eventId, IDictionary<string, object> headers = null)
        {
            var type = ev.GetType();

            var eventHeaders = new Dictionary<string, object>(headers ?? NoHeaders)
            {
                { EventClrTypeHeader, type.FullName }
            };

            if (typeof(IEventWithMetadata).IsAssignableFrom(type))
            {
                var customEvent = (IEventWithMetadata)ev;
                eventHeaders = eventHeaders.Concat(customEvent.GetAdditionalMetadata())
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            var json = JsonConvert.SerializeObject(ev is PropertyChangedEvent e ? e.Value : ev);

            var data = Encoding.UTF8.GetBytes(json);
            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders, HeaderSerializationSettings));

            return new EventData(eventId, type.Name, true, data, metadata);
        }

        /// <summary>
        /// Converts EventStore's <see cref="RecordedEvent"/> to an <see cref="IEvent"/> object.
        /// </summary>
        public static IEvent ToIEvent(this RecordedEvent ev)
        {
            if (!ev.IsJson)
                throw new ArgumentException("The event is not JSON-formatted");

            var metadata = Encoding.UTF8.GetString(ev.Metadata);
            var headers = JsonConvert.DeserializeObject<IReadOnlyDictionary<string, object>>(metadata, HeaderSerializationSettings);

            if (headers == null || !headers.TryGetValue(EventClrTypeHeader, out var typeNameEntry))
                throw new ArgumentException("Cannot resolve event type: Metadata does not specify the CLR type");

            var typeName = typeNameEntry.ToString();
            var type = FindType(typeName);

            if (type == null)
                throw new ArgumentException($"Cannot resolve event type: '{typeName}'");

            if (!typeof(IEvent).IsAssignableFrom(type))
                throw new ArgumentException($"The event type does not implement {nameof(IEvent)}");

            if (type == typeof(PropertyChangedEvent))
            {
                // Property changed events need to be handled seperately since the event data only contains the value 
                var valueTypeName = headers[nameof(PropertyChangedEvent.ValueTypeName)].ToString();
                var propertyName = headers[nameof(PropertyChangedEvent.PropertyName)].ToString();
                var resourceTypeName = headers[nameof(PropertyChangedEvent.ResourceTypeName)].ToString();
                var id = (int)(long)headers[nameof(PropertyChangedEvent.Id)];
                var userId = headers[nameof(PropertyChangedEvent.UserId)]?.ToString();
                var valueType = FindType(valueTypeName);
                object value = null;
                if (valueType != null)
                {
                    var valueJson = Encoding.UTF8.GetString(ev.Data);
                    value = JsonConvert.DeserializeObject(valueJson, valueType);
                }

                var timestamp = (DateTimeOffset)headers[nameof(PropertyChangedEvent.Timestamp)];

                return new PropertyChangedEvent(propertyName, resourceTypeName, id, userId, value)
                {
                    Timestamp = timestamp
                };
            }

            var json = Encoding.UTF8.GetString(ev.Data);
            var result = (IEvent)JsonConvert.DeserializeObject(json, type);

            if (typeof(IEventWithMetadata).IsAssignableFrom(type))
            {
                var eventWithMetadata = (IEventWithMetadata)result;
                eventWithMetadata.RestoreMetadata(headers);
            }


            return result;
        }

        private static Type FindType(string typeName)
        {
            return Type.GetType(typeName) ??
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.FullName == typeName);
        }
    }
}
