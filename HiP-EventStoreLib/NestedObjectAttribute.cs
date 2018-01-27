using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// Marks an object as a nested object and therefore the properties of this object will be considered by the <see cref="EntityManager"/> instead of comparing the whole object 
    /// </summary>

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class NestedObjectAttribute : Attribute
    {
    }
}
