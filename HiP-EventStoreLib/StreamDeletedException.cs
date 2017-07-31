using System;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public class StreamDeletedException : InvalidOperationException
    {
        public StreamDeletedException()
        {
        }

        public StreamDeletedException(string message) : base(message)
        {
        }

        public StreamDeletedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
