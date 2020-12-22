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

            var parameter = (InverterParameter)this.inverterProgrammingFieldMessageData.Parameters.ElementAt(this.currentParametersPosition);

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
                this.Logger.LogError($"1:Inverter Programming Start State, message={message}");

                this.ParentStateMachine.ChangeState(
                    new InverterProgrammingErrorState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));
            }

            return !message.IsError;
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            if (message.IsError)
            {
                this.Logger.LogError($"1:Inverter Programming StartState, message={message}");
                this.ParentStateMachine.ChangeState(
                     new InverterProgrammingErrorState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));
            }
            else
            {
                this.ParentStateMachine.GetRequiredService<IDigitalDevicesDataProvider>().UpdateInverterParameter(message.SystemIndex, message.ShortParameterId, message.StringPayload);
                if (this.inverterProgrammingFieldMessageData.IsCheckInverterVersion)
                {
                    var currentParameter = (InverterParameter)this.inverterProgrammingFieldMessageData.Parameters.ElementAt(this.currentParametersPosition);
                    if (currentParameter.StringValue != message.StringPayload)
                    {
                        this.Logger.LogError($"1:Inverter Programming StartState, message={message}, version check error, found '{message.StringPayload}' should be '{currentParameter.StringValue}'");
                        this.ParentStateMachine.ChangeState(
                             new InverterProgrammingErrorState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));
                        return true;
                    }
                }
                if (this.currentParametersPosition == (this.inverterProgrammingFieldMessageData.Parameters.Count() - 1))
                {
                    this.ParentStateMachine.ChangeState(
                         new InverterProgrammingEndState(this.ParentStateMachine, this.inverterProgrammingFieldMessageData, this.InverterStatus, this.Logger));
                }
                else
                {
                    this.currentParametersPosition++;
                    var parameter = (InverterParameter)this.inverterProgrammingFieldMessageData.Parameters.ElementAt(this.currentParametersPosition);
                    var data = this.GetNewInverterMessage(parameter);
                    _ = data.ToBytes();

                    this.ParentStateMachine.EnqueueCommandMessage(data);
                }
            }

            return true;
        }

        private InverterMessage GetNewInverterMessage(InverterParameter parameter)
        {
            if (this.inverterProgrammingFieldMessageData.IsCheckInverterVersion)
            {
                return new InverterMessage((byte)this.InverterStatus.SystemIndex, InverterParameterId.SoftwareVersion);
            }
            else
            {
                //check payload
                return new InverterMessage((byte)this.InverterStatus.SystemIndex, (short)parameter.Code, parameter.StringValue, (InverterDataset)parameter.DataSet);
            }
        }

        #endregion
    }
}
