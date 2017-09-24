using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaderbornUniversity.SILab.Hip.EventSourcing
{
    /// <summary>
    /// A transaction which aggregates multiple events in order to emit them in a single batch.
    /// </summary>
    public class EventStreamTransaction : IDisposable
    {
        private readonly List<IEvent> _events = new List<IEvent>();
        private readonly TaskCompletionSource<IReadOnlyList<IEvent>> _completion = new TaskCompletionSource<IReadOnlyList<IEvent>>();
        private bool _isCommitted;
        private bool _isDisposed;

        /// <summary>
        /// A task that finishes when the transaction is committed or disposed.
        /// </summary>
        public Task<IReadOnlyList<IEvent>> WhenCompleted => _completion.Task;

        public void Append(IEvent ev)
        {
            VerifyState();
            _events.Add(ev);
        }

        public void Append(IEnumerable<IEvent> events)
        {
            VerifyState();
            _events.AddRange(events);
        }

        /// <summary>
        /// Persists all events added to this transaction in the event stream.
        /// </summary>
        /// <returns></returns>
        public void Commit()
        {
            VerifyState();
            _isCommitted = true;
            _completion.SetResult(_events);
        }

        private void VerifyState()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EventStreamTransaction));

            if (_isCommitted)
                throw new InvalidOperationException("A commit has already been executed");
        }

        public void Dispose()
        {
            _isDisposed = true;

            if (!_isCommitted)
                _completion.SetResult(new List<IEvent>());
        }
    }
}
