using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class HostedSHDIoDriver
    {
        #region Methods

        private void ExecuteSensorsStateUpdate(FieldCommandMessage receivedMessage)
        {
            this.logger.LogTrace("1:Method Start");

            if (receivedMessage.Data is ISensorsChangedFieldMessageData sensorsChangedMessageData)
            {
                if (sensorsChangedMessageData.SensorsStatus == true)
                {
                    this.forceIoStatusPublish = true;
                }
                else
                {
                    this.logger.LogTrace("2:Wrong message Data data type");
                    var errorNotification = new FieldNotificationMessage(receivedMessage.Data,
                        "Wrong message Data data type",
                        FieldMessageActor.Any,
                        FieldMessageActor.IoDriver,
                        FieldMessageType.SensorsChanged,
                        MessageStatus.OperationError,
                        ErrorLevel.Critical);

                    this.eventAggregator?.GetEvent<FieldNotificationEvent>().Publish(errorNotification);
                }
            }
        }

        #endregion
    }
}
