using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public abstract class Mission<TMachine> : IMission
        where TMachine : class, IFiniteStateMachine
    {
        #region Fields

        protected readonly TMachine CurrentStateMachine;

        private readonly CancellationTokenSource cancellationTokenSource;

        private bool disposed;

        #endregion

        #region Constructors

        protected Mission(Guid id, TMachine currentStateMachine)
        {
            this.cancellationTokenSource = new CancellationTokenSource();
            this.Id = id;
            this.CurrentStateMachine = currentStateMachine;
        }

        #endregion

        #region Properties

        public Guid Id { get; }

        public MissionType Type { get; }

        #endregion

        #region Methods

        public bool AllowMultipleInstances(CommandMessage command)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public virtual void EndMachine()
        {
            this.cancellationTokenSource.Cancel();
        }

        public void RemoveHandler(EventHandler<FiniteStateMachinesEventArgs> endHandler)
        {
            this.CurrentStateMachine.Completed -= endHandler;
        }

        public void StartMachine(CommandMessage command)
        {
            this.CurrentStateMachine.Start(command, this.cancellationTokenSource.Token);
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
                this.cancellationTokenSource?.Dispose();
            }

            this.disposed = true;
        }

        #endregion
    }
}
