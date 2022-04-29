using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.EnIPStack;
using System.Threading;
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

        private void OnInverterMessageReceivedExplicit(byte[] messageBytes)
        {
            this.Logger.LogTrace($"1:inverterMessage={messageBytes}");
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                try
                {
                    var message = InverterMessage.FromBytesExplicit(messageBytes);

                    //this.currentStateMachines.TryGetValue(message.SystemIndex, out var messageCurrentStateMachine);

                    if (message.IsError)
                    {
                        this.Logger.LogError($"Received error Message: {message}");
                        var errorCode = (int)DataModels.MachineErrorCode.InverterErrorBaseCode + message.UShortPayload;
                        if (!Enum.IsDefined(typeof(DataModels.MachineErrorCode), errorCode))
                        {
                            errorCode = (int)DataModels.MachineErrorCode.InverterErrorBaseCode;
                        }

                        scope.ServiceProvider
                            .GetRequiredService<IErrorsProvider>()
                            .RecordNew((DataModels.MachineErrorCode)errorCode, additionalText: message.SystemIndex.ToString());
                    }

                    //if (message.IsWriteMessage)
                    //{
                    //    this.EvaluateWriteMessage(message, messageCurrentStateMachine, serviceProvider);
                    //}
                    //else
                    //{
                    //    this.EvaluateReadMessage(message, messageCurrentStateMachine, serviceProvider);
                    //}
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Exception while parsing Inverter raw message bytes {BitConverter.ToString(messageBytes)}");
                    scope.ServiceProvider.GetRequiredService<IErrorsProvider>().RecordNew(DataModels.MachineErrorCode.InverterConnectionError, BayNumber.BayOne, ex.Message);

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
                        this.processData = new InverterMessage(0, InverterParameterId.ControlWord, 0);
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
                    //this.currentStateMachines.TryGetValue(message.SystemIndex, out var messageCurrentStateMachine);

                    if (this.processData.IsError)
                    {
                        this.Logger.LogError($"Received error Message: {BitConverter.ToString(e.receivedMessage)}");
                    }

                    //    this.EvaluateReadMessage(message, messageCurrentStateMachine, serviceProvider);
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
            var serviceId = inverterMessage.IsWriteMessage ? CIPServiceCodes.SetAttributeSingle : CIPServiceCodes.GetAttributeSingle;
            var msg = inverterMessage.ToBytes();
            var datalen = msg.Length - 6;
            var data = new List<byte>();
            if (datalen > 0)
            {
                data.AddRange(msg.ToArray().Skip(6));
            }

            //TODO - instanceId is the parameter subindex
            var result = this.socketTransport.ExplicitMessage(101, 0, (ushort)inverterMessage.ParameterId, serviceId, data?.ToArray(), out var received);
            if (result)
            {
                this.OnInverterMessageReceivedExplicit(received);
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
