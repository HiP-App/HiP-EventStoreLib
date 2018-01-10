using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public class ResourceType : IEquatable<ResourceType>
    {
        private static readonly Dictionary<string, ResourceType> Dictionary = new Dictionary<string, ResourceType>();
        public static IReadOnlyDictionary<string, ResourceType> ResourceTypeDictionary => Dictionary;

        /// <summary>
        /// This name is used in two ways:
        /// 1) as a "type"/"kind of resource" identifier in events
        /// 2) as the collection name in the MongoDB cache database
        /// </summary>
        [BsonElement]
        public string Name { get; }

        public Type Type { get; private set; }

        /// <summary>
        /// This property can be used to model inheritance with resource types. This is useful if the <see cref="Type"/> inherits from the <see cref="Type"/> of the <see cref="BaseResourceType"/>.
        /// </summary>
        public ResourceType BaseResourceType { get; private set; }

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

        public static ResourceType Register(string name, Type type, ResourceType baseResourceType = null)
        {
            var resourceType = new ResourceType(name);

            resourceType.Type = type;
            if (baseResourceType != null)
            {
                if (!baseResourceType.Type.IsAssignableFrom(type)) throw new ArgumentException("The type must be a subclass of the type of the BaseResourceType", nameof(baseResourceType));
                resourceType.BaseResourceType = baseResourceType;
            }
            Dictionary.Add(name, resourceType);
            return resourceType;
        }
    }
}
