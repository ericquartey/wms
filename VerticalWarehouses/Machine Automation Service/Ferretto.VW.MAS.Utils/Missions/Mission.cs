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

        protected readonly CancellationTokenSource CancellationTokenSource;

        protected IFiniteStateMachine CurrentStateMachine;

        protected Guid MissionId;

        protected MissionType Type;

        private bool disposed;

        #endregion

        #region Constructors

        protected Mission()
            : this(Guid.NewGuid(), null)
        {
            this.MissionId = Guid.NewGuid();
            this.CancellationTokenSource = new CancellationTokenSource();
        }

        protected Mission(Guid id, IFiniteStateMachine currentStateMachine)
        {
            this.Id = id;
            this.CurrentStateMachine = currentStateMachine;
        }

        #endregion

        #region Properties

        public IFiniteStateMachine CurrentStateMachine { get; private set; }

        public Guid Id { get; private set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        public virtual void EndMachine()
        {
            this.CancellationTokenSource.Cancel();
        }

        public virtual void StartMachine(CommandMessage command)
        {
            this.CurrentStateMachine.Start(command, this.CancellationTokenSource.Token);
        }

        public virtual void StopMachine(StopRequestReason reason)
        {
            this.CurrentStateMachine.Stop(reason);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.CancellationTokenSource?.Dispose();
            }

            this.disposed = true;
        }

        #endregion
    }
}
