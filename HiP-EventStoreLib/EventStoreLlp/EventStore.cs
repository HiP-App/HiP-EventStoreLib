using System;
using System.Collections.Generic;
using System.Text;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public class EventStore : IEventStore, IEventStreamCollection
    {
        public IEventStreamCollection Streams { get; }

        IEventStream IEventStreamCollection.this[string name]
        {
            get
            {

            }
        }
    }
}
