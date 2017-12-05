using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    /// <summary>
    /// Abstract base class for events. Contains properties that every event e.g. Id or ResourceTypeName
    /// </summary>
    public abstract class BaseEvent : ICustomEvent, IEvent, IEntity<int>
    {
        /// <summary>
        /// Name of the Resource type the event belongs to
        /// </summary>
        [JsonIgnore]
        public string ResourceTypeName { get; set; }

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public string UserId { get; set; }

        [JsonIgnore]
        public DateTimeOffset Timestamp { get; set; }

        public BaseEvent(string resourceTypeName, int id, string userId)
        {
            ResourceTypeName = resourceTypeName;
            Id = id;
            UserId = userId;
            Timestamp = DateTimeOffset.Now;
        }

        public virtual IDictionary<string, object> GetAdditionalMetadata()
        {
            return new Dictionary<string, object>
            {
                { nameof(ResourceTypeName), ResourceTypeName},
                { nameof(Id), Id },
                { nameof(UserId), UserId},
                { nameof(Timestamp), Timestamp }
            };
        }

        public virtual void RestoreMetatdata(IDictionary<string, object> metadata)
        {

            if (metadata.TryGetValue(nameof(ResourceTypeName), out var typeName))
            {
                ResourceTypeName = typeName as string;
            }

            if (metadata.TryGetValue(nameof(Id), out var id))
            {
                Id = (int)(long)id;
            }

            if (metadata.TryGetValue(nameof(UserId), out var userId))
            {
                UserId = (string)userId;
            }

            if (metadata.TryGetValue(nameof(Timestamp), out var timestamp))
            {
                Timestamp = (DateTimeOffset)timestamp;
            }
        }

        public ResourceType GetEntityType()
        {
            return ResourceType.ResourceTypeDictionary[ResourceTypeName];
        }
    }
}
