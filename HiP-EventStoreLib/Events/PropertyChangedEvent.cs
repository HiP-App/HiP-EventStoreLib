using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    /// <summary>
    /// Event that is used if a property of a certain entity has changed
    /// </summary>
    public class PropertyChangedEvent : BaseEvent
    {
        [JsonIgnore]
        public string PropertyName { get; set; }

        /// <summary>
        /// Name of the value type. This property can be used for correctly deserializing the <see cref="Value"/>
        /// </summary>
        [JsonIgnore]
        public string ValueTypeName { get; set; }

        public object Value { get; set; }

        public PropertyChangedEvent(string propertyName, string resourceTypeName, int id, string userId, object value) : base(resourceTypeName, id, userId)
        {
            PropertyName = propertyName;
            Value = value;
            ValueTypeName = value?.GetType().FullName ?? String.Empty;
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
