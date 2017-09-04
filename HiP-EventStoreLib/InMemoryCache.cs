using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// A simple in-memory cache that can be updated by applying events to it.
    /// Can be used with ASP.NET Core dependency injection.
    /// </summary>
    public class InMemoryCache
    {
        private readonly ILogger<InMemoryCache> _logger;

        public IReadOnlyCollection<IDomainIndex> Indices { get; }

        public InMemoryCache(IEnumerable<IDomainIndex> indices)
        {
            Indices = indices.ToList();
        }

        /// <summary>
        /// Gets the first <see cref="IDomainIndex"/> of the specified type.
        /// </summary>
        public T Index<T>() where T : IDomainIndex => Indices.OfType<T>().FirstOrDefault();

        public void ApplyEvent(IEvent ev)
        {
            foreach (var index in Indices)
            {
                try
                {
                    index.ApplyEvent(ev);
                }
                catch (Exception e)
                {
                    _logger.LogWarning($"Failed to populate index of type '{index.GetType().Name}' with event of type '{ev.GetType().Name}': {e}");
                }
            }
        }
    }
}
