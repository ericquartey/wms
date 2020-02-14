using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;


namespace Ferretto.VW.MAS.IODriver
{
    internal partial class IoDevice
    {
        #region Methods

        public void ExecuteSensorsStateUpdate(FieldCommandMessage receivedMessage)
        {
            this.logger.LogTrace("1:Method Start");

            if (receivedMessage.Data is ISensorsChangedFieldMessageData sensorsChangedMessageData)
            {
                if (sensorsChangedMessageData.SensorsStatus)
                {
                    this.forceIoStatusPublish = true;
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
