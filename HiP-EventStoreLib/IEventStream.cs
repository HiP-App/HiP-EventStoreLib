﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    public interface IEventStream
    {
        IObservable<(IEventStream sender, IEvent ev)> Appended { get; }

        string Name { get; }

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task AppendAsync(IEvent ev);

        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task AppendManyAsync(IEnumerable<IEvent> events);

        /// <summary>
        /// Deletes a stream. This also completes and disposes the <see cref="Appended"/>-observable.
        /// Further read/write operations on this stream cause a <see cref="StreamDeletedException"/>.
        /// To recreate a stream with the same name, obtain a new <see cref="IEventStream"/>-instance
        /// from the <see cref="IEventStore"/>.
        /// </summary>
        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task DeleteAsync();

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        EventStreamTransaction BeginTransaction();

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task<(T value, bool isSuccessful)> TryGetMetadataAsync<T>(string key);

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        Task SetMetadataAsync(string key, object value);

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        IEventStreamEnumerator GetEnumerator();

        /// <exception cref="StreamDeletedException">The stream has been deleted</exception>
        IEventStreamSubscription SubscribeCatchUp();
    }

    public interface IEventStreamSubscription : IDisposable
    {
        IObservable<IEvent> EventAppeared { get; }

        event EventHandler<EventParsingFailedArgs> EventParsingFailed;
    }
}