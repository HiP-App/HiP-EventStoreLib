using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public class ResourceType : IEquatable<ResourceType>
    {
        private static readonly Dictionary<string, ResourceType> Dictionary = new Dictionary<string, ResourceType>();

        /// <summary>
        /// This name is used in two ways:
        /// 1) as a "type"/"kind of resource" identifier in events
        /// 2) as the collection name in the MongoDB cache database
        /// </summary>
        [BsonElement]
        public string Name { get; }

        public Type Type { get; private set; }

        [BsonConstructor]
        [JsonConstructor]
        private ResourceType(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name was null or empty", nameof(name));

            Name = name;

            if (Dictionary.TryGetValue(name, out var type))
            {
                // this is used for deserialization
                Type = type.Type;
            }
        }

        public override string ToString() => Name ?? "";

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object obj) => obj is ResourceType other && Equals(other);

        public bool Equals(ResourceType other) => Name == other?.Name;

        public static bool operator ==(ResourceType a, ResourceType b) => Equals(a, b);

        public static bool operator !=(ResourceType a, ResourceType b) => !(a == b);

        public static ResourceType Register(string name, Type type)
        {
            var resourceType = new ResourceType(name);
            resourceType.Type = type;
            Dictionary.Add(name, resourceType);
            return resourceType;
        }

        /// <summary>
        /// Converts the specified name to one of the registered resource types.
        /// Throws if no resource type with that name is registered.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        public static ResourceType Parse(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (Dictionary.TryGetValue(name, out var type))
                return type;

            throw new FormatException($"No {nameof(ResourceType)} with name '{name}' is registered");
        }

        /// <summary>
        /// Tries to convert the specified name to one of the registered resource types.
        /// Returns false if no resource type with that name is registered.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public static bool TryParse(string name, out ResourceType type) =>
            Dictionary.TryGetValue(name, out type);
    }
}
