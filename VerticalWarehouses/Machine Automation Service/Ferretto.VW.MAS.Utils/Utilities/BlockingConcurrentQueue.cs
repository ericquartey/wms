using System;
using System.Collections.Concurrent;
using System.Threading;


namespace Ferretto.VW.MAS.Utils.Utilities
{
    public sealed class BlockingConcurrentQueue<T> : ConcurrentQueue<T>, IDisposable
    {
        #region Fields

        private readonly ManualResetEventSlim dataReady = new ManualResetEventSlim(false);

        private readonly object syncRoot = new object();

        private bool isDisposed;

        #endregion

        #region Properties

        public WaitHandle WaitHandle => this.dataReady.WaitHandle;

        #endregion

        #region Methods

        public bool Dequeue(out T result)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            lock (this.syncRoot)
            {
                this.dataReady.Reset();
                return this.TryDequeue(out result);
            }
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.dataReady.Dispose();

            this.isDisposed = true;
        }

        public new void Enqueue(T item)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            if (item != null)
            {
                lock (this.syncRoot)
                {
                    base.Enqueue(item);
                    this.dataReady.Set();
                }
            }
        }

        public bool Peek(out T result)
        {
            return this.TryPeek(out result);
        }

        public bool TryDequeue(int timeout, CancellationToken cancellationToken, out T result)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            result = default(T);

            try
            {
                if (this.Count > 0)
                {
                    return this.Dequeue(out result);
                }

                if (this.dataReady.Wait(timeout, cancellationToken))
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
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            result = default(T);

            try
            {
                if (this.Count > 0)
                {
                    return this.Peek(out result);
                }

                if (this.dataReady.Wait(timeout, cancellationToken))
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
