using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public abstract class Mission<TMachine> : IMission
        where TMachine : class, IFiniteStateMachine
    {
        #region Fields

        private readonly CancellationTokenSource cancellationTokenSource;

        private readonly IServiceScope serviceScope;

        private bool disposed;

        #endregion

        #region Constructors

        protected Mission(IServiceScopeFactory serviceScopeFactory)
        {
            if (serviceScopeFactory is null)
            {
                throw new ArgumentNullException(nameof(serviceScopeFactory));
            }

            this.serviceScope = serviceScopeFactory.CreateScope();

            this.cancellationTokenSource = new CancellationTokenSource();
            this.CurrentStateMachine = this.serviceScope.ServiceProvider.GetRequiredService<TMachine>();
            this.FsmId = this.CurrentStateMachine.InstanceId;
            this.MachineData = this.CurrentStateMachine.MachineData;
        }

        #endregion

        #region Properties

        public Guid FsmId { get; }

        public IFiniteStateMachineData MachineData { get; set; }

        public MissionStatus Status { get; protected set; }

        public FsmType Type { get; protected set; }

        protected TMachine CurrentStateMachine { get; }

        #endregion

        #region Methods

        public virtual void AbortMachineMission()
        {
            this.CurrentStateMachine.Abort();
        }

        public abstract bool AllowMultipleInstances(CommandMessage command);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void EndMachine()
        {
            this.cancellationTokenSource.Cancel();
        }

        public void PauseMachineMission()
        {
            this.CurrentStateMachine.Pause();
        }

        public void RemoveHandler(EventHandler<FiniteStateMachinesEventArgs> endHandler)
        {
            this.CurrentStateMachine.Completed -= endHandler;
        }

        public void ResumeMachineMission()
        {
            this.CurrentStateMachine.Resume();
        }

        public void StartMachine(CommandMessage command)
        {
            this.CurrentStateMachine.Start(command, this.serviceScope.ServiceProvider, this.cancellationTokenSource.Token);
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
                this.serviceScope.Dispose();
            }

            this.disposed = true;
        }

        #endregion
    }
}
