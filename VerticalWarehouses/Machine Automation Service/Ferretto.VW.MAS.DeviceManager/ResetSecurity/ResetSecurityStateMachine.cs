using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.ResetSecurity.Interfaces;
using Ferretto.VW.MAS.DeviceManager.ResetSecurity.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.ResetSecurity
{
    internal class ResetSecurityStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IResetSecurityMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public ResetSecurityStateMachine(
            BayNumber requestingBay,
            BayNumber targetBay,
            IEventAggregator eventAggregator,
            ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(eventAggregator, logger, serviceScopeFactory)
        {
            this.machineData = new ResetSecurityMachineData(requestingBay, targetBay, eventAggregator, logger, serviceScopeFactory);
        }

        #endregion

        #region Destructors

        ~ResetSecurityStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");

            lock (this.CurrentState)
            {
                this.CurrentState.ProcessCommandMessage(message);
            }
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            lock (this.CurrentState)
            {
                var stateData = new ResetSecurityStateData(this, this.machineData);
                this.CurrentState = new ResetSecurityStartState(stateData);
                this.CurrentState?.Start();
            }

            this.Logger.LogTrace($"1:CurrentState{this.CurrentState.GetType().Name}");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogTrace("1:Method Start");

            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
