using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Diagnostics;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver
{
    internal partial class InverterDriverService
    {
        #region Fields

        private readonly Task explicitMessagesTask;

        private InverterMessage processData;

        private int sendPort;

        #endregion

        #region Methods

        private async Task ExplicitMessages()
        {
            do
            {
                try
                {
                    if (this.socketTransport.IsConnected
                        && this.inverterCommandQueue.TryPeek(Timeout.Infinite, this.CancellationToken, out var inverterMessage)
                        && inverterMessage != null)
                    {
                        this.Logger.LogTrace($"1:inverterMessage={inverterMessage}");
                        this.Logger.LogTrace($"2:Command queue length: {this.inverterCommandQueue.Count}");

                        var result = await this.ProcessInverterCommandNord(inverterMessage);

                        if (result)
                        {
                            this.inverterCommandQueue.Dequeue(out _);
                        }
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is ThreadAbortException)
                {
                    this.Logger.LogDebug("Terminating ExplicitMessages task.");
                    break;
                }
            }
            while (!this.CancellationToken.IsCancellationRequested);
        }

        private void OnConnectionStatus(object sender, ConnectionStatusChangedEventArgs e)
        {
            this.Logger.LogDebug($"Connection status tcp: {e.IsConnected}, udp: {e.IsConnectedUdp}");
            if (!e.IsConnectedUdp)
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(MachineErrorCode.InverterConnectionError);
                }

                this.Logger.LogDebug("Reconnect");
                this.socketTransport.Disconnect();
                this.socketTransport.Configure(this.inverterAddress, this.sendPort);
                for (var i = 0; i < this.forceStatusPublish.Length; i++)
                {
                    this.forceStatusPublish[i] = true;
                }
            }
            else
            {
                // TEST
                var fieldMessageData = new InverterCurrentErrorFieldMessageData();
                var commandMessage = new FieldCommandMessage(
                    fieldMessageData,
                    $"Request Inverter Error Code",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterCurrentError,
                    (byte)InverterIndex.MainInverter);

                this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(commandMessage);
                // END TEST

            }
        }

        private void OnInverterMessageReceivedExplicit(InverterMessage message, byte[] messageBytes, int length)
        {
            this.Logger.LogTrace($"1:receivedMessage={messageBytes}");
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                try
                {
                    message.FromBytesExplicit(messageBytes, length);

                    this.currentStateMachines.TryGetValue(message.SystemIndex, out var messageCurrentStateMachine);

                    if (message.IsError)
                    {
                        this.Logger.LogError($"Received error Message: {message}");
                        var errorCode = (int)MachineErrorCode.InverterErrorBaseCode + message.UShortPayload;
                        if (!Enum.IsDefined(typeof(MachineErrorCode), errorCode))
                        {
                            errorCode = (int)MachineErrorCode.InverterErrorBaseCode;
                        }

                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew((MachineErrorCode)errorCode, additionalText: message.SystemIndex.ToString());
                    }

                    if (message.IsWriteMessage)
                    {
                        this.EvaluateWriteMessage(message, messageCurrentStateMachine, scope.ServiceProvider);
                    }
                    else
                    {
                        this.EvaluateReadMessage(message, messageCurrentStateMachine, scope.ServiceProvider, this.nordFaultCodes);
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Exception while parsing Inverter raw message bytes {BitConverter.ToString(messageBytes)}");
                    scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);

                    this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);
                }
            }
        }

        private void OnInverterMessageReceivedImplicit(object sender, ImplicitReceivedEventArgs e)
        {
            this.Logger.LogTrace($"1:inverterMessage={BitConverter.ToString(e.receivedMessage)}");
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                try
                {
                    var invertersProvider = scope.ServiceProvider.GetRequiredService<IInvertersProvider>();
                    var inverters = invertersProvider.GetAll();
                    if (this.processData is null)
                    {
                        this.processData = new InverterMessage(InverterIndex.MainInverter, InverterParameterId.StatusWord, 0);
                        foreach (var inverter in inverters)
                        {
                            if (inverter is INordInverterStatus nord)
                            {
                                if (nord.SetPointRampTime == 0)
                                {
                                    nord.SetPointRampTime = 200;
                                }
                                this.processData.SetPoint(nord.SystemIndex, nord.NordControlWord.Value, nord.SetPointFrequency, nord.SetPointPosition, nord.SetPointRampTime);
                                this.Logger.LogDebug($"SetPoint inverter {nord.SystemIndex}, CW 0x{nord.NordControlWord.Value:X4}, Freq {nord.SetPointFrequency}, Pos {nord.SetPointPosition}, ramp {nord.SetPointRampTime}");
                            }
                        }
                        this.processData.FromBytesImplicit(e.receivedMessage, inverters.Last().SystemIndex);
                        this.socketTransport.ImplicitMessageWrite(this.processData.RawData);
                    }
                    else
                    {
                        this.processData.FromBytesImplicit(e.receivedMessage, inverters.Last().SystemIndex);
                        if (!e.isOk)
                        {
                            this.Logger.LogError($"Received error Message: {BitConverter.ToString(e.receivedMessage)}");
                        }
                        var elevatorDataProvider = scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                        foreach (var inverter in inverters)
                        {
                            if (inverter is INordInverterStatus nord)
                            {
                                if (nord.NordStatusWord.Value != this.processData.StatusWord[inverter.SystemIndex])
                                {
                                    nord.NordStatusWord.Value = this.processData.StatusWord[inverter.SystemIndex];
                                    this.Logger.LogDebug($"status word 0x{nord.NordStatusWord.Value:X4}");
                                    // TEST begin
                                    nord.NordControlWord.EnableVoltage = true;
                                    nord.NordControlWord.QuickStop = true;
                                    nord.NordControlWord.EnableOperation = true;
                                    nord.NordControlWord.NewSetPoint = true;
                                    nord.NordControlWord.ParameterSet1 = true;
                                    nord.NordControlWord.ParameterSet2 = true;
                                    nord.NordControlWord.FreeBit10 = true;
                                    nord.NordControlWord.FaultReset = nord.NordStatusWord.IsFault;
                                    // TEST end
                                }

                                var refresh = nord.UpdateInputsStates(this.processData.DigitalInConverted(inverter.SystemIndex));
                                var refreshPosition = nord.UpdateInverterCurrentPosition(this.processData.ActualPosition[inverter.SystemIndex]);
                                var refreshAnalogIn = nord.UpdateAnalogIn(this.processData.AnalogIn[inverter.SystemIndex]);
                                var refreshCurrent = nord.UpdateCurrent(this.processData.Current[inverter.SystemIndex]);

                                if (refresh || refreshPosition || this.forceStatusPublish[(int)inverter.SystemIndex])
                                {
                                    InverterStatusUpdateFieldMessageData notificationData;
                                    if (refreshPosition || this.forceStatusPublish[(int)inverter.SystemIndex])
                                    {
                                        // TODO - MainInverter can move both Horizontal and Vertical?
                                        var axis = inverter.SystemIndex == InverterIndex.MainInverter
                                            ? Axis.Vertical
                                            : Axis.Horizontal;

                                        if (inverter.SystemIndex > InverterIndex.Slave1)
                                        {
                                            axis = Axis.BayChain;
                                        }
                                        var axisOrientation = (axis == Axis.Horizontal || axis == Axis.BayChain) ? Orientation.Horizontal : Orientation.Vertical;

                                        double currentAxisPosition = 0;
                                        double offset = 0;
                                        if (axis == Axis.BayChain)
                                        {
                                            currentAxisPosition = invertersProvider.ConvertPulsesToMillimeters(this.processData.ActualPosition[inverter.SystemIndex], inverter);
                                        }
                                        else
                                        {
                                            currentAxisPosition = invertersProvider.ConvertPulsesToMillimeters(this.processData.ActualPosition[inverter.SystemIndex], axisOrientation);
                                            offset = (axis == Axis.Vertical)
                                                ? elevatorDataProvider.GetAxis(Orientation.Vertical).Offset
                                                : elevatorDataProvider.GetAxis(Orientation.Horizontal).Offset;
                                        }
                                        currentAxisPosition += offset;

                                        this.Logger.LogTrace($"refreshPosition inverter={inverter.SystemIndex}; axis={axis}; currentAxisPosition={currentAxisPosition}; Sensors={inverter.Inputs}");
                                        notificationData = new InverterStatusUpdateFieldMessageData(axis, inverter.Inputs, currentAxisPosition);
                                    }
                                    else
                                    {
                                        this.Logger.LogTrace($"Inverter {inverter.SystemIndex} Sensors={inverter.Inputs}");
                                        notificationData = new InverterStatusUpdateFieldMessageData(inverter.Inputs);
                                    }
                                    var msgNotification = new FieldNotificationMessage(
                                        notificationData,
                                        "Inverter values update",
                                        FieldMessageActor.DeviceManager,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.InverterStatusUpdate,
                                        MessageStatus.OperationExecuting,
                                        (byte)inverter.SystemIndex);

                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                                }

                                this.forceStatusPublish[(int)inverter.SystemIndex] = false;

                                if (refreshAnalogIn)
                                {
                                    var notificationData = new MeasureProfileFieldMessageData(profile: nord.AnalogIn);
                                    var msgNotification = new FieldNotificationMessage(
                                        notificationData,
                                        "Inverter measure profile",
                                        FieldMessageActor.DeviceManager,
                                        FieldMessageActor.InverterDriver,
                                        FieldMessageType.MeasureProfile,
                                        MessageStatus.OperationEnd,
                                        (byte)inverter.SystemIndex);

                                    this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);

                                    this.Logger.LogDebug($"ProfileInput inverter={inverter.SystemIndex}; value={nord.AnalogIn}");
                                }
                            }
                        }
                        this.currentStateMachines.TryGetValue(this.processData.SystemIndex, out var messageCurrentStateMachine);
                        var update = messageCurrentStateMachine?.ValidateCommandResponse(this.processData);

                        if (true ||
                            update.HasValue && update == true)
                        {
                            var toWrite = false;
                            foreach (var inverter in inverters)
                            {
                                if (inverter is INordInverterStatus nord
                                    && (this.processData.ControlWord[inverter.SystemIndex] != nord.NordControlWord.Value
                                        || this.processData.SetpointFrequency[inverter.SystemIndex] != nord.SetPointFrequency
                                        || this.processData.SetpointPosition[inverter.SystemIndex] != nord.SetPointPosition
                                        || this.processData.RampTime[inverter.SystemIndex] != nord.SetPointRampTime
                                    ))
                                {
                                    this.processData.SetPoint(nord.SystemIndex, nord.NordControlWord.Value, nord.SetPointFrequency, nord.SetPointPosition, nord.SetPointRampTime);
                                    this.Logger.LogDebug($"SetPoint inverter {nord.SystemIndex}, CW 0x{nord.NordControlWord.Value:X4}, Freq {nord.SetPointFrequency}, Pos {nord.SetPointPosition}, ramp {nord.SetPointRampTime}");
                                    toWrite = true;
                                }
                            }

                            if (toWrite)
                            {
                                this.socketTransport.ImplicitMessageWrite(this.processData.RawData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Exception while parsing Inverter raw message bytes {BitConverter.ToString(e.receivedMessage)}");
                    scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);

                    this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while parsing Inverter raw message bytes", 0), FieldMessageType.InverterException);
                }
            }
        }

        private async Task<bool> ProcessInverterCommandNord(InverterMessage inverterMessage)
        {
            var result = false;

            var nordMsg = new NordMessage(inverterMessage);
            if (nordMsg.ParameterId != 0)
            {
                result = this.socketTransport.ExplicitMessage(nordMsg.ClassId, nordMsg.InstanceId, nordMsg.ParameterId, nordMsg.ServiceId, nordMsg.Data, out var received, out var length);
                if (result)
                {
                    this.OnInverterMessageReceivedExplicit(inverterMessage, received, length);
                }
            }
            return result;
        }

        private Task StartCommunicationNord(Inverter masterInverter)
        {
            this.inverterAddress = masterInverter.IpAddress;
            this.sendPort = masterInverter.TcpPort;

            try
            {
                this.Logger.LogDebug("Start connection");
                this.socketTransport.Configure(this.inverterAddress, this.sendPort);
                this.socketTransport.ImplicitReceivedChanged += this.OnInverterMessageReceivedImplicit;
                this.socketTransport.ConnectionStatusChanged += this.OnConnectionStatus;

                this.explicitMessagesTask.Start();
                for (var i = 0; i < this.forceStatusPublish.Length; i++)
                {
                    this.forceStatusPublish[i] = true;
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogCritical($"Error while starting inverter socket threads: {ex.Message}");

                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while starting service threads", 0), FieldMessageType.InverterException);
                throw new InverterDriverException($"Exception {ex.Message} StartHardwareCommunications Failed 2", ex);
            }
            return Task.CompletedTask;
        }

        #endregion
    }
}
