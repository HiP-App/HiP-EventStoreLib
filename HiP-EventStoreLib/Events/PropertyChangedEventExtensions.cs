using System;

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
            if (ev.PropertyName.Contains("."))
            {
                var splitResults = ev.PropertyName.Split('.');
                int i = 0;
                object currentObj = obj;
                while (i < splitResults.Length - 1)
                {
                    var property = type.GetProperty(splitResults[i]);
                    type = property.PropertyType;
                    var nestedObject = property.GetValue(currentObj);
                    if (nestedObject == null)
                    {
                        //we need to create a new instance
                        if (!type.HasEmptyConstructor()) throw new InvalidOperationException("The property type where the NestedObjectAttribute is used must have an empty constructor");
                        var newObject = Activator.CreateInstance(type, true);
                        property.SetValue(currentObj, newObject);
                        nestedObject = newObject;
                    }

                    currentObj = nestedObject;


                    i++;
                }
                var finalProperty = type.GetProperty(splitResults[i]);
                if (finalProperty != null)
                {
                    finalProperty.SetValue(currentObj, ev.Value);
                }
            }
            else
            {
                var property = type.GetProperty(ev.PropertyName);
                if (property != null)
                {
                    property.SetValue(obj, ev.Value);
                }
            }
        }
    }
}
