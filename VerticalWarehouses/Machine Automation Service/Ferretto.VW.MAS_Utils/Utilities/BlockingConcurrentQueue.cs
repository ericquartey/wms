using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ferretto.VW.Common_Utils.Utilities
{
    public class BlockingConcurrentQueue<T> : ConcurrentQueue<T>
    {
        #region Fields

        private readonly ManualResetEventSlim dataReady;

        #endregion

        #region Constructors

        public BlockingConcurrentQueue()
        {
            this.dataReady = new ManualResetEventSlim(false);
        }

        #endregion

        #region Properties

        public WaitHandle WaitHandle => this.dataReady.WaitHandle;

        #endregion

        #region Methods

        public bool Dequeue(out T result)
        {
            this.dataReady?.Reset();
            return base.TryDequeue(out result);
        }

        public new void Enqueue(T item)
        {
            if (item != null)
            {
                base.Enqueue(item);
                this.dataReady?.Set();
            }
        }

        public bool TryDequeue(int timeout, CancellationToken cancellationToken, out T result)
        {
            try
            {
                if (this.dataReady?.Wait(timeout, cancellationToken) ?? false)
                {
                    this.dataReady.Reset();
                    return base.TryDequeue(out result);
                }
            }
            catch (OperationCanceledException)
            {
                //INFO Just exit as timeout expiration eve if the operation was actually cancelled
            }

            result = default(T);

            return false;
        }

        #endregion
    }
}
