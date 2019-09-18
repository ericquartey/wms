﻿using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Template.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Template
{
    internal class TemplateErrorState : StateBase
    {

        #region Fields

        private readonly ITemplateMachineData machineData;

        private readonly ITemplateStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public TemplateErrorState(ITemplateStateData stateData)
                    : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ITemplateMachineData;
        }

        #endregion



        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            var notificationMessage = new NotificationMessage(
                null,
                $"Template Error State Notification with {this.machineData.Message} and {this.stateData.Message}. Filed message: {this.stateData.FieldMessage.Description}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.NoType,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationError,
                ErrorLevel.Error);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if(this.disposed)
            {
                return;
            }

            if(disposing)
            {
            }

            this.disposed = true;
            base.Dispose(disposing);
        }

        #endregion
    }
}
