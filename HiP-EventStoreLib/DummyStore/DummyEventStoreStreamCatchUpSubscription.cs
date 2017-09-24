using PaderbornUniversity.SILab.Hip.EventSourcing.Migrations;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.DummyStore
{
    public class DummyEventStoreStreamCatchUpSubscription : IEventStreamSubscription
    {
        private readonly ReplaySubject<IEvent> _eventAppeared = new ReplaySubject<IEvent>();

        public event EventHandler<EventParsingFailedArgs> EventParsingFailed;

        public IObservable<IEvent> EventAppeared => _eventAppeared;

        /// <summary>
        /// Creates a new catch-up subscription.
        /// </summary>
        /// <param name="existingEvents">All events currently stored in the dummy event store</param>
        /// <param name="futureEvents">An observable of events that are appended in the future</param>
        public DummyEventStoreStreamCatchUpSubscription(IEnumerable<IEvent> existingEvents, IObservable<IEvent> futureEvents)
        {
            foreach (var ev in existingEvents)
                _eventAppeared.OnNext(ev.MigrateToLatestVersion());

            futureEvents.Subscribe(ev => _eventAppeared.OnNext(ev.MigrateToLatestVersion()));
        }
        
        public void Dispose()
        {
            _eventAppeared.OnCompleted();
            _eventAppeared.Dispose();
        }
    }
}
