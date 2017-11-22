using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    public class PropertyChangedEvent : EventBase
    {
        [JsonIgnore]
        public string PropertyName { get; set; }

        [JsonIgnore]
        public string ValueTypeName { get; set; }

        public object Value { get; set; }

        public PropertyChangedEvent(string propertyName, string resourceTypeName, int id, string userId, object value) : base(resourceTypeName, id, userId)
        {
            PropertyName = propertyName;
            Value = value;
            ValueTypeName = value.GetType().FullName;
        }

        public override IDictionary<string, object> GetAdditionalMetadata()
        {
            var dict = base.GetAdditionalMetadata();
            dict.Add(nameof(PropertyName), PropertyName);
            dict.Add(nameof(ValueTypeName), ValueTypeName);
            return dict;
        }

        public override void RestoreMetatdata(IDictionary<string, object> metadata)
        {
            base.RestoreMetatdata(metadata);
            if (metadata.TryGetValue(nameof(PropertyName), out var propertyName))
            {
                PropertyName = propertyName as string;
            }
            if (metadata.TryGetValue(nameof(ValueTypeName), out var valueTypeName))
            {
                ValueTypeName = valueTypeName as string;
            }
        }
    }


}
