using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    public abstract class EventBase : ICustomEvent, IEvent, IEntity<int>
    {
        [JsonIgnore]
        public string ResourceTypeName { get; set; }

        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public string UserId { get; set; }

        [JsonIgnore]
        public DateTimeOffset Timestamp { get; set; } 

        public EventBase(string resourceTypeName, int id, string userId)
        {
            ResourceTypeName = resourceTypeName;
            Id = id;
            UserId = userId;
        }

        public virtual IDictionary<string, object> GetAdditionalMetadata()
        {
            return new Dictionary<string, object>
            {
                { nameof(ResourceTypeName), ResourceTypeName},
                { nameof(Id), Id },
                { nameof(UserId), UserId}
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
        }
    }
}
