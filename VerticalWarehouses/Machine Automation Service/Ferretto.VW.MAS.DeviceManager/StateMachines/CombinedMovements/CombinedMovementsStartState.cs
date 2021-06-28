using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements
{
    internal class CombinedMovementsStartState : StateBase, IDisposable
    {
        #region Fields

        private const int TIMER_ELAPSED = 1500;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ICombinedMovementsMachineData machineData;

        private readonly IServiceScope scope;

        private readonly ICombinedMovementsStateData stateData;

        private Timer delayTimer;

        private bool isDisposed;

        private bool isHorizontalPositioningDone;

        private bool isVerticalPositioningDone;

        private int timerElapsed;

        #endregion

        #region Constructors

        public CombinedMovementsStartState(ICombinedMovementsStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ICombinedMovementsMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

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
                                this.machineData.OnVerticalPositioningStopped = true;
                            }
                            if (data.AxisMovement == Axis.Horizontal)
                            {
                                this.isHorizontalPositioningDone = true;
                                this.machineData.OnHorizontalPositioningStopped = true;
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

            this.Logger.LogDebug($"1:Start Method Start Horizontal movement and Vertical movement");

            var horizontalMessageData = this.machineData.MessageData.HorizontalPositioningMessageData;

            // Send message to move the horizontal axis
            var message = new CommandMessage(
                horizontalMessageData,
                $"Execute {Axis.Horizontal} Positioning Command",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                BayNumber.ElevatorBay);

            this.Logger.LogDebug($"2:Start Horizontal movement");
            this.ParentStateMachine.PublishCommandMessage(message);

            var verticalMessageData = this.machineData.MessageData.VerticalPositioningMessageData;

            //// Send message to move the vertical axis
            //message = new CommandMessage(
            //    verticalMessageData,
            //    $"Execute {Axis.Vertical} Positioning Command",
            //    MessageActor.DeviceManager,
            //    MessageActor.DeviceManager,
            //    MessageType.Positioning,
            //    this.machineData.RequestingBay,
            //    BayNumber.ElevatorBay);

            //this.ParentStateMachine.PublishCommandMessage(message);

            if (verticalMessageData.DelayStart > 0)
            {
                this.timerElapsed = verticalMessageData.DelayStart;
                this.delayTimer = new Timer(this.DelayElapsed, null, this.timerElapsed, Timeout.Infinite);
            }
            else
            {
                this.timerElapsed = 0;
                this.delayTimer = null;
                this.DelayElapsed(null);
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"1:Stop Method: Start Reason:{reason}");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new CombinedMovementsEndState(this.stateData, this.Logger));
        }

        protected void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
                this.scope.Dispose();
            }

            this.isDisposed = true;
        }

        private void DelayElapsed(object state)
        {
            var verticalMessageData = this.machineData.MessageData.VerticalPositioningMessageData;

            // Send message to move the vertical axis
            var message = new CommandMessage(
                verticalMessageData,
                $"Execute {Axis.Vertical} Positioning Command",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                BayNumber.ElevatorBay);

            this.Logger.LogDebug($"3:Start Vertical movement [fixed delay: {this.timerElapsed} ms]");
            this.ParentStateMachine.PublishCommandMessage(message);
        }

        #endregion
    }
}
