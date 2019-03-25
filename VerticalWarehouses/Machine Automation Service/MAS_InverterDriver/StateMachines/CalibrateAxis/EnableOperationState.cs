﻿using System;
using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Ferretto.VW.MAS_InverterDriver.StateMachines;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.InverterDriver.StateMachines.CalibrateAxis
{
    public class EnableOperationState : InverterStateBase
    {
        #region Fields

        private const int SEND_DELAY = 50;

        private const ushort StatusWordValue = 0x0037;

        private readonly Axis axisToCalibrate;

        private readonly ILogger logger;

        private readonly ushort parameterValue;

        #endregion

        #region Constructors

        public EnableOperationState(IInverterStateMachine parentStateMachine, Axis axisToCalibrate, ILogger logger)
        {
            this.parentStateMachine = parentStateMachine;
            this.axisToCalibrate = axisToCalibrate;
            this.logger = logger;

            this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - EnableOperationState:Ctor");

            switch (this.axisToCalibrate)
            {
                case Axis.Horizontal:
                    this.parameterValue = 0x800F;
                    break;

                case Axis.Vertical:
                    this.parameterValue = 0x000F;
                    break;
            }

            var inverterMessage =
                new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, SEND_DELAY);

            parentStateMachine.EnqueueMessage(inverterMessage);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool ProcessMessage(InverterMessage message)
        {
            var returnValue = false;

            //TEMP this.logger?.LogTrace($"{DateTime.Now}: Thread:{Thread.CurrentThread.ManagedThreadId} - EnableOperationState:ProcessMessage");
            if (message.IsError)
            {
                this.parentStateMachine.ChangeState(new ErrorState(this.parentStateMachine, this.axisToCalibrate, this.logger));
            }

            if (!message.IsWriteMessage && message.ParameterId == InverterParameterId.StatusWordParam)
            {
                if ((message.UShortPayload & StatusWordValue) == StatusWordValue)
                {
                    var messageData = new CalibrateAxisMessageData(this.axisToCalibrate, MessageVerbosity.Info);
                    var notificationMessage = new NotificationMessage(
                        messageData,
                        $"{this.axisToCalibrate} Homing started",
                        MessageActor.AutomationService,
                        MessageActor.InverterDriver,
                        MessageType.CalibrateAxis,
                        MessageStatus.OperationStart,
                        ErrorLevel.NoError,
                        MessageVerbosity.Info);
                    this.parentStateMachine.PublishNotificationEvent(notificationMessage);

                    this.parentStateMachine.ChangeState(new StartingHomeState(this.parentStateMachine, this.axisToCalibrate, this.logger));
                    returnValue = true;
                }
            }

            return returnValue;
        }

        #endregion
    }
}
