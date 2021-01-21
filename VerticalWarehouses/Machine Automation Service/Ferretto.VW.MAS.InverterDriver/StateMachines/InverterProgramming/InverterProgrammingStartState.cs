using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.InverterProgramming
{
    internal class InverterProgrammingStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterProgrammingFieldMessageData inverterProgrammingFieldMessageData;

        private readonly DateTime startTime;

        private int currentParametersPosition;

        #endregion

        #region Constructors

        public InverterProgrammingStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterProgrammingFieldMessageData inverterProgrammingFieldMessageData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Logger.LogTrace("1:Method Start");
            this.startTime = DateTime.UtcNow;
            this.inverterProgrammingFieldMessageData = inverterProgrammingFieldMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug("1:Inverter Programming Start state");

            var parameter = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);

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

                this.Logger.LogDebug($"Inverter Programming parameter={message.ParameterId}, dataset={message.DataSetIndex}");

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
                    var datasetString = this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().GetParameter(message.SystemIndex, currentParameter.WriteCode, 0).StringValue;
                    dataset = int.Parse(datasetString);
                }
                else
                {
                    dataset = message.DataSetIndex;
                }

                //check parameter in db
                if (this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().ExistInverterParameter(message.SystemIndex, message.ShortParameterId, message.DataSetIndex))
                {
                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameter(message.SystemIndex, message.ShortParameterId, result, message.DataSetIndex);
                }
                else
                {
                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().AddInverterParameter(message.SystemIndex, message.ShortParameterId, message.DataSetIndex, currentParameter.IsReadOnly, currentParameter.Type, result, currentParameter.Description, currentParameter.WriteCode, currentParameter.ReadCode, currentParameter.DecimalCount);
                }

                if (this.currentParametersPosition == (this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.Count() - 1) ||
                    this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.Count() == 1)
                {
                    this.ParentStateMachine.ChangeState(
                         new InverterProgrammingEndState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));
                }
                else
                {
                    this.currentParametersPosition++;
                    var parameter = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);
                    var data = this.GetNewInverterMessage(parameter);

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

                //check parameter in db
                if (this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().ExistInverterParameter(message.SystemIndex, message.ShortParameterId, message.DataSetIndex))
                {
                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameter(message.SystemIndex, message.ShortParameterId, result, message.DataSetIndex);
                }
                else
                {
                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().AddInverterParameter(message.SystemIndex, message.ShortParameterId, message.DataSetIndex, currentParameter.IsReadOnly, currentParameter.Type, result, currentParameter.Description, currentParameter.WriteCode, currentParameter.ReadCode, currentParameter.DecimalCount);
                }

                if (this.currentParametersPosition == (this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.Count() - 1) ||
                this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.Count() == 1)
                {
                    this.ParentStateMachine.ChangeState(
                         new InverterProgrammingEndState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));
                }
                else
                {
                    this.currentParametersPosition++;
                    var parameter = (InverterParameter)this.inverterProgrammingFieldMessageData.InverterParametersData.Parameters.ElementAt(this.currentParametersPosition);
                    var data = this.GetNewInverterMessage(parameter);

                    this.ParentStateMachine.EnqueueCommandMessage(data);
                }
            }

            return !message.IsError;
        }

        private InverterMessage GetNewInverterMessage(InverterParameter parameter)
        {
            if (parameter.IsReadOnly)
            {
                return new InverterMessage((byte)this.InverterStatus.SystemIndex, parameter.Code, parameter.DataSet);
            }
            else
            {
                return new InverterMessage((byte)this.InverterStatus.SystemIndex, (short)parameter.Code, parameter.Payload, parameter.DataSet);
            }
        }

        #endregion
    }
}
