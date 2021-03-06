﻿using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// Interface for events that have additional information (e.g. in metadata) and
    /// need to be serialized/deserialized differently.
    /// </summary>
    public interface IEventWithMetadata : IEvent
    {
        /// <summary>
        /// Creates a dictionary with additional metadata that should be stored with the event.
        /// </summary>
        /// <returns>Dictionary with metadata</returns>
        IDictionary<string, object> GetAdditionalMetadata();

        /// <summary>
        /// Restores properties from metadata.
        /// </summary>
        /// <param name="metadata">Dictionary with metadata</param>
        void RestoreMetadata(IReadOnlyDictionary<string, object> metadata);
    }
}
