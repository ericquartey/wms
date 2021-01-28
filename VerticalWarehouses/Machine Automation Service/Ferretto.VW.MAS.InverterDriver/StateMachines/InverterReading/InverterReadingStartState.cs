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
                this.Logger.LogError($"Inverter Reading Error State, message={message}, parameter={message.ParameterId}, dataset={message.DataSetIndex}, telegramError={message.TelegramErrorText}");

                this.ParentStateMachine.ChangeState(
                    new InverterReadingErrorState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
            }
            else
            {
                //check Id
                var currentParameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

                if (currentParameter.Code != message.ShortParameterId)
                {
                    return true;
                }

                this.Logger.LogDebug($"Inverter Reading: write parameter={message.ParameterId}, dataset={message.DataSetIndex}");

                //convert to string
                string result = default(string);
                switch (currentParameter.Type)
                {
                    case "short":
                        result = message.ShortPayload.ToString();
                        break;

                    case "ushort":
                        result = message.UShortPayload.ToString();
                        break;

                    case "int":
                        result = message.IntPayload.ToString();
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

                if (this.currentParametersPosition == (this.inverterReadingFieldMessageData.InverterParametersData.Parameters.Count() - 1) ||
                    this.inverterReadingFieldMessageData.InverterParametersData.Parameters.Count() == 1)
                {
                    this.ParentStateMachine.ChangeState(
                         new InverterReadingEndState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));

                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameters(this.localParameter, this.inverterReadingFieldMessageData.InverterParametersData.InverterIndex);
                    this.localParameter.Clear();
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

                    this.ParentStateMachine.EnqueueCommandMessage(data);
                }
            }

            return !message.IsError;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"Inverter Reading Error State, message={message}, parameter={message.ParameterId}, dataset={message.DataSetIndex}, telegramError={message.TelegramErrorText}");
                this.ParentStateMachine.ChangeState(
                     new InverterReadingErrorState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
            }
            else
            {
                //check Id
                var currentParameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

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
                        break;

                    case "ushort":
                        result = message.UShortPayload.ToString();
                        break;

                    case "int":
                        result = message.IntPayload.ToString();
                        break;

                    case "string":
                        result = message.StringPayload;
                        break;
                }

                var dataset = default(int);
                if (currentParameter.ReadCode != 0)
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

                this.Logger.LogDebug($"Inverter Reading parameter={message.ParameterId}, dataset={dataset}, messageDataset={message.DataSetIndex}");

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

                if (this.currentParametersPosition == (this.inverterReadingFieldMessageData.InverterParametersData.Parameters.Count() - 1) ||
                this.inverterReadingFieldMessageData.InverterParametersData.Parameters.Count() == 1)
                {
                    this.localParameter.Add(currentParameter);

                    this.ParentStateMachine.ChangeState(
                         new InverterReadingEndState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));

                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameters(this.localParameter, this.inverterReadingFieldMessageData.InverterParametersData.InverterIndex);
                    this.localParameter.Clear();
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

                    if (currentParameter.Code != parameter.ReadCode ||
                        currentParameter.Code == 0)
                    {
                        this.localParameter.Add(currentParameter);
                    }

                    this.ParentStateMachine.EnqueueCommandMessage(data);
                }
            }

            return true;
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

        #endregion
    }
}
