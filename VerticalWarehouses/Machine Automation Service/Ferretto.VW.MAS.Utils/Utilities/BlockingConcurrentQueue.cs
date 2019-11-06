using System;
using System.Collections.Concurrent;
using System.Threading;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Utilities
{
    public class BlockingConcurrentQueue<T> : ConcurrentQueue<T>
    {
        #region Fields

        private readonly ManualResetEventSlim dataReady;

        private readonly object syncRoot = new object();

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
            lock (this.syncRoot)
            {
                this.dataReady?.Reset();
                return this.TryDequeue(out result);
            }
        }

        public new void Enqueue(T item)
        {
            if (item != null)
            {
                lock (this.syncRoot)
                {
                    base.Enqueue(item);
                    this.dataReady?.Set();
                }
            }
        }

        public bool Peek(out T result)
        {
            return this.TryPeek(out result);
        }

        public bool TryDequeue(int timeout, CancellationToken cancellationToken, out T result)
        {
            result = default(T);

            try
            {
                if (this.Count > 0)
                {
                    return this.Dequeue(out result);
                }

                if (this.dataReady?.Wait(timeout, cancellationToken) ?? false)
                {
                    lock (this.syncRoot)
                    {
                        this.dataReady.Reset();
                        return this.TryDequeue(out result);
                    }
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while trying to dequeue from blocking queue {this.GetType().Name}", ex);
            }

            return false;
        }

        public bool TryPeek(int timeout, CancellationToken cancellationToken, out T result)
        {
            result = default(T);

            try
            {
                if (this.Count > 0)
                {
                    return this.Peek(out result);
                }

                if (this.dataReady?.Wait(timeout, cancellationToken) ?? false)
                {
                    return this.TryPeek(out result);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while trying to peek from blocking queue {this.GetType().Name}", ex);
            }

            return false;
        }

        #endregion
    }
}
