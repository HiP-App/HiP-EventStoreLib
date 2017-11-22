using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// Interface for events that have additional information (e.g. in metadata) and need to be serialized/deserialized diffently
    /// </summary>
    public interface ICustomEvent : IEvent
    {
        IDictionary<string, object> GetAdditionalMetadata();
        void RestoreMetatdata(IDictionary<string, object> metadata);
    }
}
