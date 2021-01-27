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

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.InverterProgramming
{
    internal class InverterProgrammingStartState : InverterStateBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IInverterProgrammingFieldMessageData inverterProgrammingFieldMessageData;

        private readonly DateTime startTime;

        private int currentParametersPosition;

        private List<InverterParameter> localParameter;

        #endregion

        #region Constructors

        public InverterProgrammingStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IEventAggregator eventAggregator,
            IInverterProgrammingFieldMessageData inverterProgrammingFieldMessageData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Logger.LogTrace("1:Method Start");
            this.startTime = DateTime.UtcNow;
            this.eventAggregator = eventAggregator;
            this.inverterProgrammingFieldMessageData = inverterProgrammingFieldMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug("1:Inverter Programming Start state");

            var parameter = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

            this.localParameter = new List<InverterParameter>();

            var message = this.GetNewInverterMessage(parameter);
            this.Logger.LogTrace($"5:inverterMessage={message}");
            this.ParentStateMachine.EnqueueCommandMessage(message);

            var notificationMessage = new FieldNotificationMessage(
                this.inverterProgrammingFieldMessageData,
                $"Inverter Programming Start",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.InverterProgramming,
                MessageStatus.OperationStart,
                (byte)this.InverterStatus.SystemIndex);

            this.ParentStateMachine.PublishNotificationEvent(notificationMessage);

            this.Logger.LogDebug("Inverter Programming Start State Start");
        }

        public override void Stop()
        {
            this.Logger.LogDebug("1:Inverter Programming Stop requested");

            this.ParentStateMachine.ChangeState(
                 new InverterProgrammingEndState(
                     this.ParentStateMachine,
                     this.inverterProgrammingFieldMessageData,
                     this.InverterStatus,
                     this.Logger));
        }

        public override bool ValidateCommandMessage(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"Inverter Programming Error State, message={message}, parameter={message.ParameterId}, dataset={message.DataSetIndex}, telegramError={message.TelegramErrorText}");

                this.ParentStateMachine.ChangeState(
                    new InverterProgrammingErrorState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));
            }
            else
            {
                //check Id
                var currentParameter = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

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
                if (currentParameter.WriteCode != 0)
                {
                    var datasetString = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition - 1);
                    dataset = int.Parse(datasetString.StringValue);
                }
                else
                {
                    dataset = message.DataSetIndex;
                }

                this.Logger.LogDebug($"Inverter Programming parameter={message.ParameterId}, dataset={dataset}, messageDataset={message.DataSetIndex}");

                var notificationMessage = new NotificationMessage(
                               new InverterParametersMessageData(MessageType.InverterProgramming, message.ShortParameterId, dataset, result, false),
                               $"Starting inverter Programming state on inverters",
                               MessageActor.Any,
                               MessageActor.DeviceManager,
                               MessageType.InverterParameters,
                               BayNumber.All,
                               BayNumber.All,
                               MessageStatus.OperationStepEnd);
                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

                currentParameter.DataSet = dataset;
                currentParameter.StringValue = result;

                if (this.currentParametersPosition == (this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.Count() - 1) ||
                    this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.Count() == 1)
                {
                    this.localParameter.Add(currentParameter);

                    this.ParentStateMachine.ChangeState(
                         new InverterProgrammingEndState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));

                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameters(this.localParameter, this.inverterProgrammingFieldMessageData.InverterParametersData.InverterIndex);
                    this.localParameter.Clear();
                }
                else
                {
                    this.currentParametersPosition++;
                    var parameter = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);
                    var data = this.GetNewInverterMessage(parameter);

                    if (currentParameter.Code != parameter.WriteCode ||
                        currentParameter.Code == 0)
                    {
                        this.localParameter.Add(currentParameter);
                    }

                    this.ParentStateMachine.EnqueueCommandMessage(data);
                }
            }

            return !message.IsError;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"Inverter Programming Error State, message={message}, parameter={message.ParameterId}, dataset={message.DataSetIndex}, telegramError={message.TelegramErrorText}");
                this.ParentStateMachine.ChangeState(
                     new InverterProgrammingErrorState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));
            }
            else
            {
                //check Id
                var currentParameter = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

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
                if (currentParameter.WriteCode != 0)
                {
                    var datasetString = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition - 1);
                    dataset = int.Parse(datasetString.StringValue);
                }
                else if (currentParameter.ReadCode != 0)
                {
                    var datasetString = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition - 1);
                    dataset = int.Parse(datasetString.StringValue);
                }
                else
                {
                    dataset = message.DataSetIndex;
                }

                this.Logger.LogDebug($"Inverter Programming: read parameter={message.ParameterId}, dataset={dataset}, messageDataset={message.DataSetIndex}");

                var notificationMessage = new NotificationMessage(
                               new InverterParametersMessageData(MessageType.InverterProgramming, message.ShortParameterId, message.DataSetIndex, result, true),
                               $"Starting inverter Programming state on inverters",
                               MessageActor.Any,
                               MessageActor.DeviceManager,
                               MessageType.InverterParameters,
                               BayNumber.All,
                               BayNumber.All,
                               MessageStatus.OperationStepEnd);
                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

                currentParameter.DataSet = dataset;
                currentParameter.StringValue = result;

                if (this.currentParametersPosition == (this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.Count() - 1) ||
                this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.Count() == 1)
                {
                    this.localParameter.Add(currentParameter);

                    this.ParentStateMachine.ChangeState(
                         new InverterProgrammingEndState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));

                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameters(this.localParameter, this.inverterProgrammingFieldMessageData.InverterParametersData.InverterIndex);
                    this.localParameter.Clear();
                }
                else
                {
                    this.currentParametersPosition++;
                    var parameter = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);
                    var data = this.GetNewInverterMessage(parameter);

                    if (currentParameter.Code != parameter.ReadCode ||
                        currentParameter.Code == 0)
                    {
                        this.localParameter.Add(currentParameter);
                    }

                    this.ParentStateMachine.EnqueueCommandMessage(data);
                }
            }

            return !message.IsError;
        }

        private InverterMessage GetNewInverterMessage(InverterParameter parameter)
        {
            if (parameter.IsReadOnly)
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
