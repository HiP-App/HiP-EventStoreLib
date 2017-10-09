using System;
using EventStore.ClientAPI;
using PaderbornUniversity.SILab.Hip.EventSourcing.Migrations;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public class EventStoreStreamCatchUpSubscription : IEventStreamSubscription
    {
        private readonly EventStoreCatchUpSubscription _subscription;
        private readonly Action<IEvent> _handler;

        public event EventHandler<EventParsingFailedArgs> EventParsingFailed;

        public EventStoreStreamCatchUpSubscription(IEventStoreConnection connection, string streamName, Action<IEvent> handler)
        {
            _handler = handler;

            _subscription = connection.SubscribeToStreamFrom(
                streamName,
                null, // don't use StreamPosition.Start (see https://groups.google.com/forum/#!topic/event-store/8tpXJMNEMqI),
                CatchUpSubscriptionSettings.Default,
                OnEventAppeared);
        }

        private Task OnEventAppeared(EventStoreCatchUpSubscription _, ResolvedEvent resolvedEvent)
        {
            try
            {
                // Note regarding migration:
                // Event types may change over time (properties get added/removed etc.)
                // Whenever an event has multiple versions, an event of an obsolete type should be transformed to an event
                // of the latest version, so that ApplyEvent(...) only has to deal with events of the current version.

                var ev = resolvedEvent.Event.ToIEvent().MigrateToLatestVersion();
                _handler?.Invoke(ev);
            }
            catch (Exception e)
            {
                EventParsingFailed?.Invoke(this, new EventParsingFailedArgs(resolvedEvent, e));
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _subscription.Stop();
        }
    }
}
