using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public abstract class Mission : IDisposable
    {

        #region Fields

        protected readonly CancellationTokenSource cancellationTokenSource;

        protected IFiniteStateMachine currentStateMachine;

        protected Guid id;

        protected MissionType type;

        private bool disposed;

        #endregion

        #region Constructors

        protected Mission()
        {
            this.id = Guid.NewGuid();
            this.cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion



        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void EndMachine()
        {
            this.cancellationTokenSource.Cancel();
        }

        public virtual void StartMachine(CommandMessage command)
        {
            this.currentStateMachine.Start(command, this.cancellationTokenSource.Token);
        }

        public virtual void StopMachine(StopRequestReason reason)
        {
            this.currentStateMachine.Stop(reason);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.cancellationTokenSource?.Dispose();
            }

            this.disposed = true;
        }

        #endregion
    }
}
