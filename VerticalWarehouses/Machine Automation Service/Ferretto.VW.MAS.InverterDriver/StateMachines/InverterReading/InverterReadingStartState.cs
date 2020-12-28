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

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.InverterReading
{
    internal class InverterReadingStartState : InverterStateBase
    {
        #region Fields

        private readonly IInverterReadingFieldMessageData inverterReadingFieldMessageData;

        private readonly DateTime startTime;

        private int currentParametersPosition;

        #endregion

        #region Constructors

        public InverterReadingStartState(
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            IInverterReadingFieldMessageData inverterReadingFieldMessageData,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.Logger.LogTrace("1:Method Start");
            this.startTime = DateTime.UtcNow;
            this.inverterReadingFieldMessageData = inverterReadingFieldMessageData;
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug("1:Inverter Reading Start state");

            var parameter = (InverterParameter)this.inverterReadingFieldMessageData.Parameters.ElementAt(this.currentParametersPosition);

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
            if (message.IsError)
            {
                this.Logger.LogError($"1:Inverter Reading Start State, message={message}");

                this.ParentStateMachine.ChangeState(
                    new InverterReadingErrorState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
            }

            return !message.IsError;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:Inverter Reading StartState, message={message}");
                this.ParentStateMachine.ChangeState(
                     new InverterReadingErrorState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
            }
            else
            {
                //check Id
                var currentParameter = (InverterParameter)this.inverterReadingFieldMessageData.Parameters.ElementAt(this.currentParametersPosition);

                if (currentParameter.Code != message.ShortParameterId)
                {
                    return true;
                }

                if (this.inverterReadingFieldMessageData.IsCheckInverterVersion)
                {
                    //if (currentParameter.StringValue != message.StringPayload)
                    //{
                    //    this.Logger.LogError($"1:Inverter Reading StartState, message={message}, version check error, found '{message.StringPayload}' should be '{currentParameter.StringValue}'");
                    //    this.ParentStateMachine.ChangeState(
                    //         new InverterReadingErrorState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
                    //    return true;
                    //}
                }

                //check parameter in db
                if (this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().ExistInverterParameter(message.SystemIndex, message.ShortParameterId))
                {
                    this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameter(message.SystemIndex, message.ShortParameterId, message.StringPayload);
                }

                if (this.currentParametersPosition == (this.inverterReadingFieldMessageData.Parameters.Count() - 1))
                {
                    this.ParentStateMachine.ChangeState(
                         new InverterReadingEndState(this.ParentStateMachine, this.inverterReadingFieldMessageData, this.InverterStatus, this.Logger));
                }
                else
                {
                    this.currentParametersPosition++;
                    var parameter = (InverterParameter)this.inverterReadingFieldMessageData.Parameters.ElementAt(this.currentParametersPosition);
                    var data = this.GetNewInverterMessage(parameter);
                    _ = data.ToBytes();

                    this.ParentStateMachine.EnqueueCommandMessage(data);
                }
            }

            return true;
        }

        private InverterMessage GetNewInverterMessage(InverterParameter parameter)
        {
            if (this.inverterReadingFieldMessageData.IsCheckInverterVersion)
            {
                return new InverterMessage((byte)this.InverterStatus.SystemIndex, InverterParameterId.SoftwareVersion, parameter.DataSet);
            }
            else
            {
                return new InverterMessage((byte)this.InverterStatus.SystemIndex, parameter.Code, parameter.DataSet);
            }
        }

        #endregion
    }
}
