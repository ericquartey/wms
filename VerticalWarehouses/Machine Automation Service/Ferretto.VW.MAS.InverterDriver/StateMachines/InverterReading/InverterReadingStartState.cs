using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.InverterReading
{
    internal class InverterReadingStartState : InverterStateBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterReadingFieldMessageData inverterReadingFieldMessageData;

        private readonly DateTime startTime;

        private int currentParametersPosition;

        private List<InverterParameter> localParameter;

        #endregion

        #region Constructors

        public InverterReadingStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IEventAggregator eventAggregator,
            IInverterReadingFieldMessageData inverterReadingFieldMessageData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Logger.LogTrace("1:Method Start");
            this.startTime = DateTime.UtcNow;
            this.eventAggregator = eventAggregator;
            this.inverterReadingFieldMessageData = inverterReadingFieldMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug("1:Inverter Reading Start state");

            var parameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

            this.localParameter = new List<InverterParameter>();

            var message = this.GetNewInverterMessage(parameter, false);
            this.Logger.LogTrace($"5:inverterMessage={message}");
            this.ParentStateMachine.EnqueueCommandMessage(message);

            var notificationMessage = new FieldNotificationMessage(
                this.inverterReadingFieldMessageData,
                $"Inverter Reading Start",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterReading,
                MessageStatus.OperationStart,
                (byte)this.InverterStatus.SystemIndex);

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

            this.Logger.LogDebug("Inverter Reading Start State Start");
        }

        public override void Stop()
        {
            this.Logger.LogDebug("1:Inverter Reading Stop requested");

            this.ParentStateMachine.ChangeState(
                 new InverterReadingEndState(
                     this.ParentStateMachine,
                     this.inverterReadingFieldMessageData,
                     this.InverterStatus,
                     this.Logger));
        }

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"Inverter Reading Error, message={message}, parameter={message.ParameterId}, dataset={message.DataSetIndex}, telegramError={message.TelegramErrorText}");

                if (message.TelegramErrorText == "11 Unknown parameter" || message.TelegramErrorText == "15 Unknown error")
                {
                    this.Logger.LogDebug($"Inverter Reading: write parameter={message.ParameterId}, dataset={message.DataSetIndex}, shortPayload{message.ShortPayload}");

                    var currentParameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);
                    currentParameter.Error = true;

                    this.NextParameter(currentParameter, false);

                    return true;
                }
                else
                {
                    this.ParentStateMachine.ChangeState(
                        new InverterReadingErrorState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
                }
            }
            else
            {
                //check Id
                var currentParameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

                if (currentParameter.Code != message.ShortParameterId)
                {
                    return false;
                }

                this.Logger.LogDebug($"Inverter Reading: write parameter={message.ParameterId}, dataset={message.DataSetIndex}");

                //convert to string
                string result = default(string);
                switch (currentParameter.Type)
                {
                    case "short":
                        result = message.ShortPayload.ToString();
                        result = this.FixDecimalValue(result, currentParameter.DecimalCount);
                        break;

                    case "ushort":
                        result = message.UShortPayload.ToString();
                        result = this.FixDecimalValue(result, currentParameter.DecimalCount);
                        break;

                    case "int":
                        result = message.IntPayload.ToString();
                        result = this.FixDecimalValue(result, currentParameter.DecimalCount);
                        break;

                    case "string":
                        result = message.StringPayload;
                        break;
                }

                var notificationMessage = new NotificationMessage(
                               new InverterParametersMessageData(MessageType.InverterReading, message.ShortParameterId, message.DataSetIndex, result, false),
                               $"Starting inverter Reading state on inverters",
                               MessageActor.Any,
                               MessageActor.DeviceManager,
                               MessageType.InverterParameters,
                               BayNumber.All,
                               BayNumber.All,
                               MessageStatus.OperationStepEnd);
                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

                currentParameter.StringValue = result;
                currentParameter.Error = false;

                this.NextParameter(currentParameter, false);
            }

            return false;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"Inverter Reading Error State, message={message}, parameter={message.ParameterId}, dataset={message.DataSetIndex}, telegramError={message.TelegramErrorText}");
                if (message.TelegramErrorText == "11 Unknown parameter")
                {
                    this.Logger.LogDebug($"Inverter Reading parameter={message.ParameterId}, dataset={message.DataSetIndex}, shortPayload{message.ShortPayload}");

                    var currentParameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);
                    currentParameter.Error = true;

                    this.NextParameter(currentParameter, false);

                    return true;
                }
                else
                {
                    this.ParentStateMachine.ChangeState(
                        new InverterReadingErrorState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
                }
            }
            else
            {
                var par = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);
                var currentParameter = new InverterParameter()
                {
                    Id = par.Id,
                    Code = par.Code,
                    DataSet = par.DataSet,
                    DecimalCount = par.DecimalCount,
                    Description = par.Description,
                    Error = par.Error,
                    IsReadOnly = par.IsReadOnly,
                    ReadCode = par.ReadCode,
                    StringValue = par.StringValue,
                    Type = par.Type,
                    Um = par.Um,
                    WriteCode = par.WriteCode
                };

                if (currentParameter.Code != message.ShortParameterId)
                {
                    return true;
                }

                //convert to string
                string result = default(string);
                switch (currentParameter.Type)
                {
                    case "short":
                        result = message.ShortPayload.ToString();
                        result = this.FixDecimalValue(result, currentParameter.DecimalCount);
                        break;

                    case "ushort":
                        result = message.UShortPayload.ToString();
                        result = this.FixDecimalValue(result, currentParameter.DecimalCount);
                        break;

                    case "int":
                        result = message.IntPayload.ToString();
                        result = this.FixDecimalValue(result, currentParameter.DecimalCount);
                        break;

                    case "string":
                        result = message.StringPayload;
                        break;
                }

                var dataset = default(int);
                if (currentParameter.ReadCode == 1)
                {
                    dataset = int.Parse(currentParameter.StringValue);
                }
                else if (currentParameter.ReadCode > 1)
                {
                    var datasetString = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition - 1);
                    dataset = int.Parse(datasetString.StringValue);
                }
                else
                {
                    dataset = message.DataSetIndex;
                }

                currentParameter.DataSet = dataset;
                currentParameter.StringValue = result;
                currentParameter.Error = false;

                this.Logger.LogDebug($"Inverter Reading parameter={message.ParameterId}, dataset={dataset}, messageDataset={message.DataSetIndex}, readCode={currentParameter.ReadCode}, result={result}");

                var notificationMessage = new NotificationMessage(
                              new InverterParametersMessageData(MessageType.InverterReading, message.ShortParameterId, dataset, result, true),
                              $"Starting inverter Reading state on inverters",
                              MessageActor.Any,
                              MessageActor.DeviceManager,
                              MessageType.InverterParameters,
                              BayNumber.All,
                              BayNumber.All,
                              MessageStatus.OperationStepEnd);
                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

                this.NextParameter(currentParameter, !string.IsNullOrEmpty(result) && currentParameter.ReadCode != 1);
            }

            return true;
        }

        private string FixDecimalValue(string value, int decimalCount)
        {
            if (decimalCount > value.Length)
            {
                this.Logger.LogTrace("Fix decimal count");
                if (decimalCount == 1)
                {
                    value = "0" + value;
                }
                else if (decimalCount == 2)
                {
                    value = "00" + value;
                }
            }

            return value;
        }

        private InverterMessage GetNewInverterMessage(InverterParameter parameter, bool isWriteMessage)
        {
            if (!isWriteMessage)
            {
                //this.Logger.LogDebug($"Message read: Code={parameter.Code}, Dataset={parameter.DataSet}");
                return new InverterMessage((byte)this.InverterStatus.SystemIndex, parameter.Code, parameter.DataSet);
            }
            else
            {
                //this.Logger.LogDebug($"Message write: Code={parameter.Code}, Dataset={parameter.DataSet}, Value={parameter.StringValue}");
                return new InverterMessage((byte)this.InverterStatus.SystemIndex, (short)parameter.Code, parameter.Payload, parameter.DataSet);
            }
        }

        private void NextParameter(InverterParameter currentParameter, bool addParameter)
        {
            if (this.currentParametersPosition == (this.inverterReadingFieldMessageData.InverterParametersData.Parameters.Count() - 1) ||
                this.inverterReadingFieldMessageData.InverterParametersData.Parameters.Count() == 1)
            {
                if (addParameter)
                {
                    this.localParameter.Add(currentParameter);
                }

                try
                {
                    if (this.localParameter.Any())
                    {
                        this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameters(this.localParameter, this.inverterReadingFieldMessageData.InverterParametersData.InverterIndex);
                    }

                    var notificationMessage = new NotificationMessage(
                                   new InverterParametersMessageData(MessageType.InverterParameters, 0, 0, $"Inverter {this.InverterStatus.SystemIndex} values updated", false),
                                   $"save inverter structure",
                                   MessageActor.Any,
                                   MessageActor.DeviceManager,
                                   MessageType.InverterParameters,
                                   BayNumber.All,
                                   BayNumber.All,
                                   MessageStatus.OperationUpdateData);
                    this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, ex.Message);
                }
                this.localParameter.Clear();

                this.ParentStateMachine.ChangeState(
                     new InverterReadingEndState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
            }
            else
            {
                this.currentParametersPosition++;
                var parameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

                var isWriteMessage = false;
                if (parameter.ReadCode == 1)
                {
                    isWriteMessage = true;
                }
                var data = this.GetNewInverterMessage(parameter, isWriteMessage);

                if ((currentParameter.Code != parameter.ReadCode ||
                        currentParameter.Code == 0) &&
                    addParameter)
                {
                    this.localParameter.Add(currentParameter);
                }

                this.ParentStateMachine.EnqueueCommandMessage(data);
            }
        }

        #endregion
    }
}
