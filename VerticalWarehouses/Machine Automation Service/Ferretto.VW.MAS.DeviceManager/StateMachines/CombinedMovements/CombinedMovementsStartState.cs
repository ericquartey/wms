using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements
{
    internal class CombinedMovementsStartState : StateBase
    {
        #region Fields

        private readonly ICombinedMovementsMachineData machineData;

        private readonly IServiceScope scope;

        private readonly ICombinedMovementsStateData stateData;

        private bool isHorizontalPositioningDone;

        private bool isVerticalPositioningDone;

        #endregion

        #region Constructors

        public CombinedMovementsStartState(ICombinedMovementsStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ICombinedMovementsMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source}");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogDebug($"1:Process Notitication Message {message.Type} Source {message.Source}");
            if (message.Type == MessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        {
                            var data = message.Data as IPositioningMessageData;

                            if (data.AxisMovement == Axis.Vertical)
                            {
                                this.isVerticalPositioningDone = true;
                            }
                            if (data.AxisMovement == Axis.Horizontal)
                            {
                                this.isHorizontalPositioningDone = true;
                            }

                            if (this.isVerticalPositioningDone &&
                                this.isHorizontalPositioningDone)
                            {
                                this.ParentStateMachine.ChangeState(new CombinedMovementsEndState(this.stateData, this.Logger));
                            }
                            break;
                        }

                    case MessageStatus.OperationError:
                        {
                            this.ParentStateMachine.ChangeState(new CombinedMovementsErrorState(this.stateData, this.Logger));
                            break;
                        }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void Start()
        {
            this.isHorizontalPositioningDone = false;
            this.isVerticalPositioningDone = false;

            var horizontalMessageData = this.machineData.MessageData.HorizontalPositioningMessageData;
            this.Logger.LogDebug($"Horizontal movement :");

            // Send message to move the horizontal axis
            var message = new CommandMessage(
                horizontalMessageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                BayNumber.ElevatorBay);

            this.ParentStateMachine.PublishCommandMessage(message);

            var verticalMessageData = this.machineData.MessageData.VerticalPositioningMessageData;
            this.Logger.LogDebug($"Vertical movement :");

            // Send message to move the vertical axis
            message = new CommandMessage(
                verticalMessageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                BayNumber.ElevatorBay);

            this.ParentStateMachine.PublishCommandMessage(message);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new CombinedMovementsEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
