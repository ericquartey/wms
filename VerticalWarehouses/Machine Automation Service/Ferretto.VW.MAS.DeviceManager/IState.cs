using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager
{
    internal interface IState
    {
        #region Properties

        ILogger Logger { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Send a command message.
        /// </summary>
        /// <param name="message">A <see cref="CommandMessage"/> is published from state</param>
        void ProcessCommandMessage(CommandMessage message);

        /// <summary>
        /// Process the notification message incoming to the Finite State Machines from the field.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> notification message to be parsed.</param>
        void ProcessFieldNotificationMessage(FieldNotificationMessage message);

        /// <summary>
        /// Send a notification message.
        /// </summary>
        /// <param name="message">A <see cref="NotificationMessage"/> is published from state.</param>
        void ProcessNotificationMessage(NotificationMessage message);

        /// <summary>
        /// Starts executing the current state logic
        /// </summary>
        void Start();

        /// <summary>
        /// Executes stop action in the current state to stop running Finite State Machine
        /// </summary>
        /// <param name="reason"></param>
        void Stop(StopRequestReason reason);

        #endregion
    }
}
