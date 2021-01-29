using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver
{
    partial class InverterDriverService
    {
        #region Methods

        protected override Task OnCommandReceivedAsync(FieldCommandMessage receivedMessage, IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");

            var inverterIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (this.currentStateMachines.TryGetValue(inverterIndex, out var messageCurrentStateMachine)
                &&
                messageCurrentStateMachine != null)
            {
                if (receivedMessage.Type == FieldMessageType.InverterStop)
                {
                    this.Logger.LogTrace("4: Stop the timer for update shaft position");
                    this.axisPositionUpdateTimer[(int)inverterIndex].Change(Timeout.Infinite, Timeout.Infinite);

                    messageCurrentStateMachine.Stop();

                    return Task.CompletedTask;
                }
                if (receivedMessage.Type == FieldMessageType.ContinueMovement)
                {
                    double? targetPosition = null;
                    if (receivedMessage.Data is IPositioningFieldMessageData positioningData)
                    {
                        var axisOrientation = positioningData.AxisMovement == Axis.Horizontal
                            ? Orientation.Horizontal
                            : Orientation.Vertical;
                        var invertersProvider = serviceProvider.GetRequiredService<IInvertersProvider>();
                        targetPosition = invertersProvider.ConvertMillimetersToPulses(positioningData.TargetPosition, axisOrientation);
                    }
                    messageCurrentStateMachine.Continue(targetPosition);

                    return Task.CompletedTask;
                }

                if (receivedMessage.Type != FieldMessageType.InverterSetTimer &&
                    receivedMessage.Type != FieldMessageType.MeasureProfile &&
                    receivedMessage.Type != FieldMessageType.InverterCurrentError)
                {
                    this.Logger.LogWarning($"5:Inverter Driver already executing operation {messageCurrentStateMachine.GetType().Name}");
                    this.Logger.LogError($"5a: Message {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source} will be discarded!");
                    var ex = new Exception();
                    this.SendOperationErrorMessage(inverterIndex, new InverterExceptionFieldMessageData(ex, "Inverter operation already in progress", 0), FieldMessageType.InverterError);

                    return Task.CompletedTask;
                }
            }

            try
            {
                var inverter = serviceProvider
                    .GetRequiredService<IInvertersProvider>()
                    .GetByIndex(inverterIndex);

                switch (receivedMessage.Type)
                {
                    case FieldMessageType.CalibrateAxis:
                        this.ProcessCalibrateAxisMessage(receivedMessage, inverter);
                        break;

                    case FieldMessageType.InverterPowerOff:
                        this.ProcessPowerOffMessage(receivedMessage, inverter);
                        break;

                    case FieldMessageType.InverterPowerOn:
                        this.ProcessPowerOnMessage(receivedMessage, inverter);
                        break;

                    case FieldMessageType.Positioning:
                        this.ProcessPositioningMessage(receivedMessage, inverter, serviceProvider);
                        break;

                    case FieldMessageType.ShutterPositioning:
                        this.ProcessShutterPositioningMessage(receivedMessage, inverter);
                        break;

                    case FieldMessageType.InverterSetTimer:
                        this.ProcessInverterSetTimerMessage(receivedMessage, inverter);
                        break;

                        // not used
                    //case FieldMessageType.InverterSwitchOff:
                    //    this.ProcessInverterSwitchOffMessage(inverter);
                    //    break;

                    case FieldMessageType.InverterSwitchOn:
                        this.ProcessInverterSwitchOnMessage(receivedMessage, inverter);
                        break;

                    case FieldMessageType.InverterStop:
                        this.ProcessStopMessage(inverter);
                        break;

                    case FieldMessageType.InverterFaultReset:
                        this.ProcessFaultResetMessage(inverter);
                        break;

                        // not used
                    //case FieldMessageType.InverterDisable:
                    //    this.ProcessDisableMessage(inverter);
                    //    break;

                    case FieldMessageType.MeasureProfile:
                        this.ProcessMeasureProfileMessage(inverter);
                        break;

                    case FieldMessageType.InverterCurrentError:
                        this.ProcessReadCurrentError(inverter);
                        break;

                    case FieldMessageType.InverterProgramming:
                        this.ProcessInverterProgrammingMessage(receivedMessage, inverter);
                        break;

                    case FieldMessageType.InverterReading:
                        this.ProcessInverterReadingMessage(receivedMessage, inverter);
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Received invalid command");

                this.SendOperationErrorMessage(
                    (InverterIndex)receivedMessage.DeviceIndex,
                    new InverterExceptionFieldMessageData(ex, "Inverter number {currentInverter} is not configured for this machine", 0),
                    FieldMessageType.InverterStop);
            }

            var notificationMessageData = new MachineStatusActiveMessageData(MessageActor.InverterDriver, receivedMessage.Type.ToString(), MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Inverter current machine status {receivedMessage.Type}",
                MessageActor.Any,
                MessageActor.InverterDriver,
                MessageType.MachineStatusActive,
                BayNumber.None,
                BayNumber.None,
                MessageStatus.OperationStart);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);

            return Task.CompletedTask;
        }

        #endregion
    }
}
