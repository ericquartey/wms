using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver
{
    partial class InverterDriverService
    {
        #region Methods

        private void OnCommandReceived(FieldCommandMessage receivedMessage)
        {
            this.logger.LogTrace($"1:Command received: {receivedMessage.Type}, destination: {receivedMessage.Destination}, source: {receivedMessage.Source}");

            var messageDeviceIndex = Enum.Parse<InverterIndex>(receivedMessage.DeviceIndex.ToString());

            if (this.inverterStatuses.Count == 0)
            {
                this.logger.LogError("4:Invert Driver not configured for this message Type");

                var ex = new Exception();
                this.SendOperationErrorMessage(messageDeviceIndex, new InverterExceptionFieldMessageData(ex, "Invert Driver not configured for this message Type", 0), FieldMessageType.InverterError);

                return;
            }

            this.currentStateMachines.TryGetValue(messageDeviceIndex, out var messageCurrentStateMachine);

            if (messageCurrentStateMachine != null && receivedMessage.Type == FieldMessageType.InverterStop)
            {
                this.logger.LogTrace("4: Stop the timer for update shaft position");
                this.axisPositionUpdateTimer[(int)messageDeviceIndex].Change(Timeout.Infinite, Timeout.Infinite);

                messageCurrentStateMachine.Stop();

                return;
            }

            if (messageCurrentStateMachine != null && receivedMessage.Type != FieldMessageType.InverterSetTimer)
            {
                this.logger.LogWarning($"5:Inverter Driver already executing operation {messageCurrentStateMachine.GetType()}");
                this.logger.LogError($"5a: Message {receivedMessage.Type} will be discarded!");
                var ex = new Exception();
                this.SendOperationErrorMessage(messageDeviceIndex, new InverterExceptionFieldMessageData(ex, "Inverter operation already in progress", 0), FieldMessageType.InverterError);

                return;
            }

            switch (receivedMessage.Type)
            {
                case FieldMessageType.CalibrateAxis:
                    this.ProcessCalibrateAxisMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterPowerOff:
                    this.ProcessPowerOffMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterPowerOn:
                    this.ProcessPowerOnMessage(receivedMessage);
                    break;

                case FieldMessageType.Positioning:
                case FieldMessageType.TorqueCurrentSampling:
                    this.ProcessPositioningMessage(receivedMessage);
                    break;

                case FieldMessageType.ShutterPositioning:
                    this.ProcessShutterPositioningMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterSetTimer:
                    this.ProcessInverterSetTimerMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterSwitchOff:
                    this.ProcessInverterSwitchOffMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterSwitchOn:
                    this.ProcessInverterSwitchOnMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterStop:
                    this.ProcessStopMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterFaultReset:
                    this.ProcessFaultResetMessage(receivedMessage);
                    break;

                case FieldMessageType.InverterDisable:
                    this.ProcessDisableMessage(receivedMessage);
                    break;
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

            this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

            this.logger.LogTrace($"Socket Timings: Read Wait Samples {this.ReadWaitTimeData.TotalSamples}, Max {this.ReadWaitTimeData.MaxValue}ms, Min {this.ReadWaitTimeData.MinValue}ms, Average {this.ReadWaitTimeData.AverageValue}ms, Deviation {this.ReadWaitTimeData.StandardDeviation}ms / Round Trip Samples {this.WriteRoundtripTimeData.TotalSamples}, Max {this.WriteRoundtripTimeData.MaxValue}ms, Min {this.WriteRoundtripTimeData.MinValue}ms, Average {this.WriteRoundtripTimeData.AverageValue}ms, Deviation {this.WriteRoundtripTimeData.StandardDeviation}ms");
            this.logger.LogTrace($"Axis Timings: Request interval Samples {this.AxisTimeData.TotalSamples}, Max {this.AxisTimeData.MaxValue}ms, Min {this.AxisTimeData.MinValue}ms, Average {this.AxisTimeData.AverageValue}ms, Deviation {this.AxisTimeData.StandardDeviation}ms / Round Trip Samples {this.AxisIntervalTimeData.TotalSamples}, Max {this.AxisIntervalTimeData.MaxValue}ms, Min {this.AxisIntervalTimeData.MinValue}ms, Average {this.AxisIntervalTimeData.AverageValue}ms, Deviation {this.AxisIntervalTimeData.StandardDeviation}ms");
            this.logger.LogTrace($"Sensor Timings: Request interval Samples {this.SensorTimeData.TotalSamples}, Max {this.SensorTimeData.MaxValue}ms, Min {this.SensorTimeData.MinValue}ms, Average {this.SensorTimeData.AverageValue}ms, Deviation {this.SensorTimeData.StandardDeviation}ms / Round Trip Samples {this.SensorIntervalTimeData.TotalSamples}, Max {this.SensorIntervalTimeData.MaxValue}ms, Min {this.SensorIntervalTimeData.MinValue}ms, Average {this.SensorIntervalTimeData.AverageValue}ms, Deviation {this.SensorIntervalTimeData.StandardDeviation}ms");
        }

        #endregion
    }
}
