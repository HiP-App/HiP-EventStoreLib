using System;
using System.Reflection;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Events
{
    public static class PropertyChangedEventExtensions
    {
        /// <summary>
        /// Sets the property of the object (if it exists) to the value of the event 
        /// </summary>       
        public static void ApplyTo(this PropertyChangedEvent ev, object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var type = obj.GetType();
            var property = type.GetProperty(ev.PropertyName);
            if (property != null)
            {
                property.SetValue(obj, ev.Value);
            }
        }
    }
}
