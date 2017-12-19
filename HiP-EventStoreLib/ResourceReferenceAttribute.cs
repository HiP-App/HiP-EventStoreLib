using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// Indicates that the value of the property is an ID or a list of IDs referring to other resources
    /// of a specific type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ResourceReferenceAttribute : Attribute
    {
        public string ResourceType { get; }

        public ResourceReferenceAttribute(string resourceType) => ResourceType = resourceType;
    }
}
