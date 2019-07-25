using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ferretto.VW.MAS.Utils.Utilities
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
            return this.TryDequeue(out result);
        }

        public bool Peek(out T result)
        {
            this.dataReady?.Reset();
            return this.TryPeek(out result);
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
                if (this.Count > 0)
                {
                    return this.Dequeue(out result);
                }

                if (this.dataReady?.Wait(timeout, cancellationToken) ?? false)
                {
                    this.dataReady.Reset();
                    return this.TryDequeue(out result);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception {ex.Message} while trying to dequeue object from blocking queue {this.GetType()}");
            }

            result = default(T);

            return false;
        }

        public bool TryPeek(int timeout, CancellationToken cancellationToken, out T result)
        {
            try
            {
                if (this.Count > 0)
                {
                    return this.Peek(out result);
                }

                if (this.dataReady?.Wait(timeout, cancellationToken) ?? false)
                {
                    this.dataReady.Reset();
                    return this.TryPeek(out result);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception {ex.Message} while trying to peek object from blocking queue {this.GetType()}");
            }

            result = default(T);

            return false;
        }
        #endregion
    }
}
