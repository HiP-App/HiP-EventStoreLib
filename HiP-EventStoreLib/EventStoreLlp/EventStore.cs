﻿using EventStore.ClientAPI;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp
{
    /// <summary>
    /// Provides read/write access to a running instance of Event Store
    /// by Event Store LLP (see https://eventstore.org/).
    /// 
    /// Can be used with ASP.NET Core dependency injection (requires
    /// <see cref="EventStoreConfig"/> options).
    /// </summary>
    public class EventStore : IEventStore, IEventStreamCollection
    {
        private readonly Dictionary<string, IEventStream> _streamWrappers = new Dictionary<string, IEventStream>();
        private readonly AsyncLock _mutex = new AsyncLock();

        internal IEventStoreConnection UnderlyingConnection { get; }

        public IEventStreamCollection Streams => this;

        /// <summary>
        /// Initializes an <see cref="EventStore"/> from an already connected <see cref="IEventStoreConnection"/>.
        /// </summary>
        /// <param name="underlyingConnection"></param>
        public EventStore(IEventStoreConnection underlyingConnection)
        {
            UnderlyingConnection = underlyingConnection;
        }

        public EventStore(IOptions<EventStoreConfig> config)
        {
            // Validate config
            if (string.IsNullOrWhiteSpace(config.Value.Host))
                throw new ArgumentException($"Invalid configuration: Missing value for '{nameof(config.Value.Host)}'");

            // Prevent accidentally working with a production database
            if (Debugger.IsAttached)
            {
                Debug.Assert(config.Value.Host.Contains("localhost"),
                    "It looks like you are trying to connect to a production Event Store database. Are you sure you wish to continue?");
            }

            var settings = ConnectionSettings.Create()
                .EnableVerboseLogging()
                .Build();

            var uri = new Uri(config.Value.Host);
            var connection = EventStoreConnection.Create(settings, uri);
            connection.ConnectAsync().Wait();

            UnderlyingConnection = connection;
        }

        internal async Task DeleteStreamAsync(string name)
        {
            using (await _mutex.LockAsync())
            {
                await UnderlyingConnection.DeleteStreamAsync(name, ExpectedVersion.Any);
                _streamWrappers.Remove(name);
            }
        }

        IEventStream IEventStreamCollection.this[string name]
        {
            get
            {
                using (_mutex.Lock())
                {
                    if (_streamWrappers.TryGetValue(name, out var existingStreamWrapper))
                        return existingStreamWrapper;

                    var streamWrapper = new EventStoreStream(this, name);
                    _streamWrappers[name] = streamWrapper;
                    return streamWrapper;
                }
            }
        }
    }
}
