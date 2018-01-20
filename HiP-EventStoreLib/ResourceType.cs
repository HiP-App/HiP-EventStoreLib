using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// This class is the "glue" between different representations of the same entities.
    /// For example, consider an exhibit in DataStore: There's a class 'Exhibit' defining which information
    /// of an exhibit is stored in the Mongo database for caching purposes. There's also a class 'ExhibitArgs'
    /// defining the properties posted to the REST API. There may be further REST-related classes such as
    /// 'ExhibitUpdateArgs' for cases where the POST- and PUT-methods expect different parameters.
    /// However, all these different representations of an exhibit are "connected" to the same ResourceType.
    /// </remarks>
    public class ResourceType : IEquatable<ResourceType>
    {
        private static readonly ConcurrentDictionary<string, ResourceType> Dictionary = 
            new ConcurrentDictionary<string, ResourceType>();

        /// <summary>
        /// This name is used in two ways:
        /// 1) as a "type"/"kind of resource" identifier in events
        /// 2) as the collection name in the MongoDB cache database
        /// </summary>
        [BsonElement]
        public string Name { get; }

        /// <summary>
        /// Refers to the class that describes the core properties of an entity.
        /// In many cases, this is the '*Args'-class that is also used for REST requests.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// This property can be used to model inheritance with resource types.
        /// This is useful if the <see cref="Type"/> inherits from the <see cref="Type"/>
        /// of the <see cref="BaseResourceType"/>.
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

        /// <remarks>
        /// Registering the same resource type multiple times is intentionally allowed
        /// to better support testing scenarios.
        /// </remarks>
        /// <returns>The newly registered resource type</returns>
        public static ResourceType Register(string name, Type type, ResourceType baseResourceType = null)
        {
            if (baseResourceType != null && !baseResourceType.Type.IsAssignableFrom(type))
                throw new ArgumentException("The type must be a subclass of the type of the BaseResourceType", nameof(baseResourceType));

            var resourceType = new ResourceType(name)
            {
                Type = type,
                BaseResourceType = baseResourceType
            };

            Dictionary[name] = resourceType;
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
