using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.ShutterPositioning
{
    public class ShutterPositioningExecutingState : StateBase
    {
        #region Fields

        private readonly InverterIndex inverterIndex;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly IShutterPositioningMessageData shutterPositioningMessageData;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningExecutingState(
            IStateMachine parentMachine,
            IShutterPositioningMessageData shutterPositioningMessageData,
            InverterIndex inverterIndex,
            IMachineSensorsStatus machineSensorsStatus,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.shutterPositioningMessageData = shutterPositioningMessageData;
            this.machineSensorsStatus = machineSensorsStatus;
            this.inverterIndex = inverterIndex;
        }

        #endregion

        #region Destructors

        ~ShutterPositioningExecutingState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.ShutterPositioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.Logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new ShutterPositioningErrorState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, message, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var notificationMessageData = new ShutterPositioningMessageData(this.shutterPositioningMessageData);
            var inverterStatus = new AglInverterStatus((byte)this.inverterIndex);
            int sensorStart = (int)(IOMachineSensors.PowerOnOff + (int)this.inverterIndex * inverterStatus.aglInverterInputs.Length);
            Array.Copy(this.machineSensorsStatus.DisplayedInputs, sensorStart, inverterStatus.aglInverterInputs, 0, inverterStatus.aglInverterInputs.Length);
            notificationMessageData.ShutterPosition = inverterStatus.CurrentShutterPosition;
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Move from {notificationMessageData.ShutterPosition}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationExecuting);

            this.Logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new ShutterPositioningEndState(this.ParentStateMachine, this.shutterPositioningMessageData, this.inverterIndex, this.machineSensorsStatus, this.Logger, true));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
