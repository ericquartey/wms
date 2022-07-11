using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
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

        private InverterMessage canData;

        #endregion

        #region Methods

        private void OnConnectionStatusCan(object sender, ConnectionStatusChangedEventArgs e)
        {
            this.Logger.LogDebug($"Connection status can: {e.IsConnected}");
            if (!e.IsConnected)
            {
                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    this.Logger.LogDebug("Reconnect");
                    try
                    {
                        this.socketTransport.Disconnect();
                        scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(MachineErrorCode.InverterConnectionError);
                        this.socketTransport.ConnectAsync();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError($"Exception {ex.Message} while reconnecting");
                        scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);

                        this.SendOperationErrorMessage(InverterIndex.None, new InverterExceptionFieldMessageData(ex, $"Exception {ex.Message} while reconnecting", 0), FieldMessageType.InverterException);
                    }
                }
            }
            else
            {
                // TEST begin
                var fieldMessageData = new InverterCurrentErrorFieldMessageData();
                var commandMessage = new FieldCommandMessage(
                    fieldMessageData,
                    $"Request Inverter Error Code",
                    FieldMessageActor.InverterDriver,
                    FieldMessageActor.DeviceManager,
                    FieldMessageType.InverterCurrentError,
                    (byte)InverterIndex.MainInverter);

                this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(commandMessage);
                // TEST end

                for (var i = 0; i < this.forceStatusPublish.Length; i++)
                {
                    this.forceStatusPublish[i] = true;
                }
            }
        }

        private void OnInverterMessageReceivedPDO(object sender, ImplicitReceivedEventArgs e)
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                try
                {
                    var invertersProvider = scope.ServiceProvider.GetRequiredService<IInvertersProvider>();
                    var inverters = invertersProvider.GetAll();
                    var inverter = inverters.First(x => x.CanOpenNode == e.node);
                    var inverterIndex = inverter.SystemIndex;
                    if (inverter.CanOpenNode.HasValue
                        && this.canData is null
                        && e.isOk)
                    {
                        this.Logger.LogTrace($"1:inverterMessage={BitConverter.ToString(e.receivedMessage)}");
                        this.canData = new InverterMessage(InverterIndex.MainInverter, InverterParameterId.StatusWord, 0);
                        this.canData.SetPointCan(inverter.SystemIndex, inverter.CommonControlWord.Value, 0, inverter.SetPointPosition, 0, inverter.OperatingMode);
                        this.Logger.LogDebug($"SetPoint inverter {inverter.SystemIndex}, CW 0x{inverter.CommonControlWord.Value:X4}, Pos {inverter.SetPointPosition}");
                        this.canData.FromBytesPDO(e.receivedMessage, inverterIndex, inverters.Last().SystemIndex);
                        this.socketTransport.ImplicitMessageWrite(this.canData.RawData, e.node);
                        // TEST begin
                        var fieldMessageData = new InverterCurrentErrorFieldMessageData();
                        var commandMessage = new FieldCommandMessage(
                            fieldMessageData,
                            $"Request Inverter Error Code",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.InverterCurrentError,
                            (byte)InverterIndex.MainInverter);

                        this.EventAggregator.GetEvent<FieldCommandEvent>().Publish(commandMessage);
                        // TEST end
                    }
                    else if (inverter.CanOpenNode.HasValue)
                    {
                        if (e.isOk)
                        {
                            this.Logger.LogTrace($"1:inverterMessage={BitConverter.ToString(e.receivedMessage)}");
                            this.canData.FromBytesPDO(e.receivedMessage, inverterIndex, inverters.Last().SystemIndex);
                            var elevatorDataProvider = scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                            if (inverter.CommonStatusWord.Value != this.canData.StatusWord[inverter.SystemIndex])
                            {
                                inverter.CommonStatusWord.Value = this.canData.StatusWord[inverter.SystemIndex];
                                this.Logger.LogDebug($"status word 0x{inverter.CommonStatusWord.Value:X4}");

                                // TEST begin
                                //nord.NordControlWord.DisableVoltage = true;
                                //nord.NordControlWord.QuickStop = true;
                                //nord.NordControlWord.ControlWordValid = true;
                                //nord.NordControlWord.FaultReset = nord.NordStatusWord.IsFault;
                                // TEST end
                            }

                            var refresh = inverter.UpdateInputsStates(this.canData.DigitalInConverted(inverter.SystemIndex));
                            var refreshPosition = false;
                            var axis = inverter.SystemIndex == InverterIndex.MainInverter
                                ? Axis.Vertical
                                : Axis.Horizontal;
                            if (inverter.SystemIndex > InverterIndex.Slave1)
                            {
                                axis = Axis.BayChain;
                            }
                            if (inverter is IPositioningInverterStatus positioningInverter)
                            {
                                refreshPosition = positioningInverter.UpdateInverterCurrentPosition(axis, this.canData.ActualPosition[inverter.SystemIndex]);
                            }

                            if (refresh || refreshPosition || this.forceStatusPublish[(int)inverter.SystemIndex])
                            {
                                InverterStatusUpdateFieldMessageData notificationData;
                                if (refreshPosition || this.forceStatusPublish[(int)inverter.SystemIndex])
                                {
                                    var axisOrientation = (axis == Axis.Horizontal || axis == Axis.BayChain) ? Orientation.Horizontal : Orientation.Vertical;

                                    double currentAxisPosition = 0;
                                    double offset = 0;
                                    if (axis == Axis.BayChain)
                                    {
                                        currentAxisPosition = invertersProvider.ConvertPulsesToMillimeters(this.canData.ActualPosition[inverter.SystemIndex], inverter);
                                    }
                                    else
                                    {
                                        currentAxisPosition = invertersProvider.ConvertPulsesToMillimeters(this.canData.ActualPosition[inverter.SystemIndex], axisOrientation);
                                        offset = (axis == Axis.Vertical)
                                            ? elevatorDataProvider.GetAxis(Orientation.Vertical).Offset
                                            : elevatorDataProvider.GetAxis(Orientation.Horizontal).Offset;
                                    }
                                    currentAxisPosition += offset;

                                    this.Logger.LogDebug($"refreshPosition inverter={inverter.SystemIndex}; axis={axis}; currentAxisPosition={currentAxisPosition:0.0000}; Sensors={inverter.InputsToString()}");
                                    notificationData = new InverterStatusUpdateFieldMessageData(axis, inverter.Inputs, currentAxisPosition);
                                }
                                else
                                {
                                    this.Logger.LogDebug($"Inverter {inverter.SystemIndex} Sensors={inverter.InputsToString()}");
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

                            this.currentStateMachines.TryGetValue(this.canData.SystemIndex, out var messageCurrentStateMachine);
                            messageCurrentStateMachine?.ValidateCommandResponse(this.canData);

                            if (inverter.CanOpenNode.HasValue
                                && (this.canData.ControlWord[inverter.SystemIndex] != inverter.CommonControlWord.Value
                                    || this.canData.SetpointPosition[inverter.SystemIndex] != inverter.SetPointPosition
                                ))
                            {
                                this.canData.SetPointCan(inverter.SystemIndex, inverter.CommonControlWord.Value, 0, inverter.SetPointPosition, 0, inverter.OperatingMode);
                                this.Logger.LogDebug($"SetPoint inverter {inverter.SystemIndex}, CW 0x{inverter.CommonControlWord.Value:X4}, Pos {inverter.SetPointPosition}");
                                this.socketTransport.ImplicitMessageWrite(this.canData.RawData, inverter.CanOpenNode.Value);
                            }
                        }
                        else if (e.isEmergency)
                        {
                            var error = e.emergencyManufacturerError;
                            this.Logger.LogError($"Received error {CanMessage.ErrorString(e.emergencyError)} register {e.emergencyRegister} node {e.emergencyNode} inverter error {error:X4}{this.inverterFaultCodes.GetErrorByCode(error)}");
                            if (error > 0
                                && error != 0x1454
                                )
                            {
                                // Retrieve the bay number related to the inverter index
                                var baysProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                                var bayNumber = baysProvider.GetByInverterIndex((InverterIndex)e.emergencyNode);

                                // Adds error related to the InverterFaultDetected
                                var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                                var idx = e.emergencyNode + 1; // it has the systemIndex on base 1
                                errorsProvider.RecordNew(idx, error, bayNumber, CanMessage.ErrorString(e.emergencyError) + this.inverterFaultCodes.GetErrorByCode(error));
                            }
                        }
                        else if (e.isSync)
                        {
                            this.Logger.LogDebug($"Received SYNC Message");
                        }
                        else if (e.isNMT)
                        {
                            this.Logger.LogDebug($"Received NMT Message: node {e.nMTNode} state {e.nMTState}");
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

        private void OnInverterMessageReceivedSDO(InverterMessage message, byte[] messageBytes, int length)
        {
            this.Logger.LogTrace($"1:receivedMessage={BitConverter.ToString(messageBytes)}");
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                try
                {
                    if (messageBytes != null && messageBytes.Length > 0)
                    {
                        message.FromBytesSDO(messageBytes, length);
                    }

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

        private async Task<bool> ProcessInverterCommandCan(InverterMessage inverterMessage)
        {
            var result = false;

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var invertersProvider = scope.ServiceProvider.GetRequiredService<IInvertersProvider>();
                var inverters = invertersProvider.GetAll();

                // some commands are not used in CANOpen
                if (inverterMessage.ParameterId == InverterParameterId.ControlWord
                        || inverterMessage.ParameterId == InverterParameterId.StatusWord
                        || inverterMessage.ParameterId == InverterParameterId.DigitalInputsOutputs
                        || inverterMessage.ParameterId == InverterParameterId.PositionTargetPosition
                        || inverterMessage.ParameterId == InverterParameterId.ActualPositionShaft
                        || inverterMessage.ParameterId == InverterParameterId.SetOperatingMode)
                {
                    if (inverterMessage.ParameterId == InverterParameterId.PositionTargetPosition
                        && inverterMessage.IsWriteMessage)
                    {
                        // we use this message to cash the inverter target position
                        var inverter = invertersProvider.GetByIndex(inverterMessage.SystemIndex);
                        inverter.SetPointPosition = (int)inverterMessage.Payload;
                    }
                    result = true;
                }
                else
                {
                    var canMsg = new CanMessage(inverterMessage);
                    if (canMsg.Index != 0
                        && this.socketTransport.IsConnected)
                    {
                        canMsg.NodeId = (byte)inverters.First(x => x.SystemIndex == inverterMessage.SystemIndex).CanOpenNode;
                        result = this.socketTransport.SDOMessage(canMsg.NodeId, canMsg.Index, canMsg.Subindex, canMsg.IsWriteMessage, canMsg.Data, out var received, out var length);
                        if (result)
                        {
                            this.OnInverterMessageReceivedSDO(inverterMessage, received, length);
                        }
                        else
                        {
                            var abortString = CanMessage.AbortString((ulong)length);
                            this.Logger.LogError($"Received error Message: {abortString}");

                            scope.ServiceProvider
                                .GetRequiredService<IErrorsProvider>()
                                .RecordNew(MachineErrorCode.InverterErrorBaseCode, additionalText: abortString);
                        }
                    }
                }
            }
            return result;
        }

        private Task StartCommunicationCan(Inverter masterInverter, IEnumerable<int> nodeList)
        {
            try
            {
                this.nodeList = nodeList;
                this.Logger.LogDebug("Start connection");
                this.socketTransport.ImplicitReceivedChanged += this.OnInverterMessageReceivedPDO;
                this.socketTransport.ConnectionStatusChanged += this.OnConnectionStatusCan;
                this.explicitMessagesTask.Start();
                this.socketTransport.Configure(this.inverterAddress, 0, nodeList);
            }
            catch (Exception ex)
            {
                this.Logger.LogCritical($"Error while starting inverter socket threads: {ex.Message}");

                using (var scope = this.ServiceScopeFactory.CreateScope())
                {
                    var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                    errorsProvider.RecordNew(MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);
                }
                this.SendOperationErrorMessage(InverterIndex.MainInverter, new InverterExceptionFieldMessageData(ex, "while starting service threads", 0), FieldMessageType.InverterException);
            }
            return Task.CompletedTask;
        }

        #endregion
    }
}
