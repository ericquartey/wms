using System.Collections.Concurrent;
using System.Threading;

namespace Ferretto.VW.Common_Utils.Utilities
{
    public class BlockingConcurrentQueue<T> : ConcurrentQueue<T>
    {
        #region Fields

        private ManualResetEventSlim dataReady;

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

        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            this.dataReady?.Set();
        }

        public bool TryDequeue(int timeout, CancellationToken cancellationToken, out T result)
        {
            if (this.dataReady?.Wait(timeout, cancellationToken) ?? false)
            {
                this.dataReady.Reset();
                return base.TryDequeue(out result);
            }

            result = default(T);

            return false;
        }

        #endregion
    }
}
