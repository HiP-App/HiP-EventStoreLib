using System;
using EventStore.ClientAPI;
using System.Reactive.Subjects;
using PaderbornUniversity.SILab.Hip.EventSourcing.Migrations;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    public class EventStoreStreamCatchUpSubscription : IEventStreamSubscription
    {
        private readonly ReplaySubject<IEvent> _eventAppeared = new ReplaySubject<IEvent>();
        private readonly EventStoreCatchUpSubscription _subscription;

        public IObservable<IEvent> EventAppeared => _eventAppeared;

        public event EventHandler<EventParsingFailedArgs> EventParsingFailed;

        public EventStoreStreamCatchUpSubscription(IEventStoreConnection connection, string streamName)
        {
            _subscription = connection.SubscribeToStreamFrom(
                streamName,
                null, // don't use StreamPosition.Start (see https://groups.google.com/forum/#!topic/event-store/8tpXJMNEMqI),
                CatchUpSubscriptionSettings.Default,
                OnEventAppeared);
        }

        private void OnEventAppeared(EventStoreCatchUpSubscription _, ResolvedEvent resolvedEvent)
        {
            try
            {
                // Note regarding migration:
                // Event types may change over time (properties get added/removed etc.)
                // Whenever an event has multiple versions, an event of an obsolete type should be transformed to an event
                // of the latest version, so that ApplyEvent(...) only has to deal with events of the current version.

                var ev = resolvedEvent.Event.ToIEvent().MigrateToLatestVersion();
                _eventAppeared.OnNext(ev);
            }
            catch (Exception e)
            {
                EventParsingFailed?.Invoke(this, new EventParsingFailedArgs(resolvedEvent, e));
            }
        }

        public void Dispose()
        {
            _subscription.Stop();
            _eventAppeared.OnCompleted();
            _eventAppeared.Dispose();
        }
    }
}
