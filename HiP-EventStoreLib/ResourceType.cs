﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public class ResourceType : IEquatable<ResourceType>
    {
        private static Dictionary<string, ResourceType> _dictionary = new Dictionary<string, ResourceType>();
        public static IReadOnlyDictionary<string, ResourceType> ResourceTypeDictionary => _dictionary;

        /// <summary>
        /// This name is used in two ways:
        /// 1) as a "type"/"kind of resource" identifier in events
        /// 2) as the collection name in the MongoDB cache database
        /// </summary>
        [BsonElement]
        public string Name { get; }

        public Type Type { get; private set; }

        [BsonConstructor]
        private ResourceType(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name was null or empty", nameof(name));

            Name = name;
        }

        public override string ToString() => Name ?? "";

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object obj) => obj is ResourceType other && Equals(other);

        public bool Equals(ResourceType other) => Name == other.Name;

        public static bool operator ==(ResourceType a, ResourceType b) => a?.Equals(b) ?? b == null;

        public static bool operator !=(ResourceType a, ResourceType b) => !(a == b);

        public static ResourceType Register(string name, Type type)
        {
            var resourceType = new ResourceType(name);
            resourceType.Type = type;
            _dictionary.Add(name, resourceType);
            return resourceType;
        }
    }
}
