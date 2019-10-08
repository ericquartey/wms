using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IMachineControlProvider
    {
        #region Methods

        bool FilterNotifications(NotificationMessage notification, MessageActor destination);

        /// <summary>
        /// Check message status for inverter power related messages
        /// </summary>
        /// <param name="message">Notification message to check</param>
        /// <returns>Status for the inverter power related message. NoStatus otherwise</returns>
        MessageStatus InverterPowerChangeStatus(NotificationMessage message);

        /// <summary>
        /// Check message status for power related messages
        /// </summary>
        /// <param name="message">Notification message to check</param>
        /// <returns>Status for the power related message. NoStatus otherwise</returns>
        MessageStatus PowerStatusChangeStatus(NotificationMessage message);

        void ResetBayFault(BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        /// <summary>
        /// Check message status for Reset Bay (inverter) related messages
        /// </summary>
        /// <param name="message">Notification message to check</param>
        /// <returns>Status for the Reset Bay (inverter) related message. NoStatus otherwise</returns>
        MessageStatus ResetBayFaultStatus(NotificationMessage message);

        void ResetSecurity(MessageActor sender, BayNumber requestingBay);

        /// <summary>
        /// Check message status for reset security related messages
        /// </summary>
        /// <param name="message">Notification message to check</param>
        /// <returns>Status for the reset security related message. NoStatus otherwise</returns>
        MessageStatus ResetSecurityStatus(NotificationMessage message);

        void StartChangePowerStatus(IChangeRunningStateMessageData messageData, MessageActor sender, BayNumber requestingBay);

        void StartInverterPowerChange(IInverterPowerEnableMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        /// <summary>
        /// Check message status for stop related messages
        /// </summary>
        /// <param name="message">Notification message to check</param>
        /// <returns>Status for the stop related message. NoStatus otherwise</returns>
        MessageStatus StopOperationStatus(NotificationMessage message);

        #endregion
    }
}
