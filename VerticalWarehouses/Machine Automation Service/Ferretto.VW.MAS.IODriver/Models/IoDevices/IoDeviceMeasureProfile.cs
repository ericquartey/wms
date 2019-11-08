﻿using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.IODriver.StateMachines.MeasureProfile;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver
{
    internal partial class IoDevice
    {
        #region Methods

        public void ExecuteMeasureProfile(FieldCommandMessage receivedMessage)
        {
            this.logger.LogTrace("1:Method Start");

            if (receivedMessage.Data is IMeasureProfileFieldMessageData messageData)
            {
                if (this.CurrentStateMachine != null)
                {
                    this.logger.LogError($"ExecuteMeasureProfile: Io Driver already executing operation {this.CurrentStateMachine.GetType().Name}");

                    var ex = new Exception();
                    this.SendOperationErrorMessage(new IoExceptionFieldMessageData(ex, "Io Driver already executing operation", 0));
                }
                else
                {
                    this.CurrentStateMachine = new MeasureProfileStateMachine(
                        messageData.Enable,
                        this.ioCommandQueue,
                        this.ioStatus,
                        this.deviceIndex,
                        this.eventAggregator,
                        this.logger);

                    this.CurrentStateMachine.Start();
                }
            }
            else
            {
                this.logger.LogTrace("2:Wrong message Data data type");
                var errorNotification = new FieldNotificationMessage(
                    receivedMessage.Data,
                    "Wrong message Data data type",
                    FieldMessageActor.Any,
                    FieldMessageActor.IoDriver,
                    FieldMessageType.SensorsChanged,
                    MessageStatus.OperationError,
                    (byte)this.deviceIndex,
                    ErrorLevel.Error);

                this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
            }
        }

        #endregion
    }
}
