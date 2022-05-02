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
                                this.processData.SetPoint(nord.SystemIndex, nord.CommonControlWord.Value, nord.SetPointFrequency, nord.SetPointPosition, nord.SetPointRampTime);
                            }
                        }
                        this.processData.FromBytesImplicit(e.receivedMessage, inverters.Last().SystemIndex);
                        this.socketTransport.ImplicitMessageStart(this.processData.RawData);
                    }
                    else
                    {
                        this.processData.FromBytesImplicit(e.receivedMessage, inverters.Last().SystemIndex);
                        foreach (var inverter in inverters)
                        {
                            var refresh = inverter.UpdateInputsStates(this.processData.DigitalInConverted(inverter.SystemIndex));
                            if (refresh || this.forceStatusPublish[(int)inverter.SystemIndex])
                            {
                                this.Logger.LogTrace($"Inverter {inverter.SystemIndex} Sensor Update {inverter.Inputs}");

                                var msgNotification = new FieldNotificationMessage(
                                    new InverterStatusUpdateFieldMessageData(inverter.Inputs),
                                    "Inverter Inputs update",
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageType.InverterStatusUpdate,
                                    MessageStatus.OperationExecuting,
                                    inverter.SystemIndex);

                                this.eventAggregator.GetEvent<FieldNotificationEvent>().Publish(msgNotification);
                                this.forceStatusPublish[(int)inverter.SystemIndex] = false;
                            }
                        }
                    }
                    this.currentStateMachines.TryGetValue(this.processData.SystemIndex, out var messageCurrentStateMachine);

                    if (this.processData.IsError)
                    {
                        this.Logger.LogError($"Received error Message: {BitConverter.ToString(e.receivedMessage)}");
                    }

                    messageCurrentStateMachine?.ValidateCommandResponse(this.processData);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Exception while parsing Inverter raw message bytes {BitConverter.ToString(e.receivedMessage)}");
                    scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);

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
