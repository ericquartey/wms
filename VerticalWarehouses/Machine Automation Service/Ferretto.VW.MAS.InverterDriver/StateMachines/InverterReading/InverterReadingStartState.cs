using System;
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

            var message = this.GetNewInverterMessage(parameter);
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
            this.Logger.LogTrace($"1:message={message}:Is Error={message.IsError}");

            return true;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            //check Id
            var currentParameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

            if (currentParameter.Code != message.ShortParameterId)
            {
                return true;
            }
            else if (message.IsError)
            {
                this.Logger.LogError($"Inverter Reading Error State, message={message}, parameter={message.ParameterId}, dataset={message.DataSetIndex}, telegramError={message.TelegramErrorText}");
                this.ParentStateMachine.ChangeState(
                     new InverterReadingErrorState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogDebug($"Inverter Reading parameter={message.ParameterId}, dataset={message.DataSetIndex}");

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
                    var datasetString = this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().GetParameter(message.SystemIndex, currentParameter.ReadCode, 0).StringValue;
                    dataset = int.Parse(datasetString);
                }
                else
                {
                    dataset = message.DataSetIndex;
                }

                var notificationMessage = new NotificationMessage(
                              new InverterParametersMessageData(message.ShortParameterId, dataset, result, true),
                              $"Starting inverter Reading state on inverters",
                              MessageActor.Any,
                              MessageActor.DeviceManager,
                              MessageType.InverterParameter,
                              BayNumber.All,
                              BayNumber.All,
                              MessageStatus.OperationStepEnd);
                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);

                //check parameter in db
                if (this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().ExistInverterParameter(message.SystemIndex, message.ShortParameterId, message.DataSetIndex))
                {
                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameter(message.SystemIndex, message.ShortParameterId, result, message.DataSetIndex);
                }
                else
                {
                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().AddInverterParameter(message.SystemIndex, message.ShortParameterId, message.DataSetIndex, currentParameter.IsReadOnly, currentParameter.Type, result, currentParameter.Description, currentParameter.ReadCode, currentParameter.WriteCode, currentParameter.DecimalCount);
                }

                if (this.currentParametersPosition == (this.inverterReadingFieldMessageData.InverterParametersData.Parameters.Count() - 1) ||
                this.inverterReadingFieldMessageData.InverterParametersData.Parameters.Count() == 1)
                {
                    this.ParentStateMachine.ChangeState(
                         new InverterReadingEndState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
                }
                else
                {
                    this.currentParametersPosition++;
                    var parameter = (InverterParameter)this.inverterReadingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);
                    var data = this.GetNewInverterMessage(parameter);

                    this.ParentStateMachine.EnqueueCommandMessage(data);
                }
            }

            return true;
        }

        private InverterMessage GetNewInverterMessage(InverterParameter parameter)
        {
            return new InverterMessage((byte)this.InverterStatus.SystemIndex, parameter.Code, parameter.DataSet);
        }

        #endregion
    }
}
