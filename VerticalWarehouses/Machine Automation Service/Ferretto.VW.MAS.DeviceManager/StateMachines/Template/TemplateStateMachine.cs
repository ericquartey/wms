using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Template.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Template.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Template
{
    internal class TemplateStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ITemplateMachineData machineData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateStateMachine(
            CommandMessage receivedMessage,
            IEventAggregator eventAggregator,
            ILogger<DeviceManagerService> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(receivedMessage.TargetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.machineData = new TemplateMachineData(receivedMessage.RequestingBay, receivedMessage.TargetBay, eventAggregator, logger, serviceScopeFactory);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.CurrentState.ProcessCommandMessage(message);
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState.ProcessNotificationMessage(message);
        }

        public override void Start()
        {
            var stateData = new TemplateStateData(this, this.machineData);
            this.ChangeState(new TemplateStartState(stateData, this.Logger));
        }

        public override void Stop(StopRequestReason reason)
        {
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
