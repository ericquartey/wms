using System;
using Ferretto.VW.MAS_Utils.Messages;

namespace Ferretto.VW.MAS_InverterDriver.Interface.StateMachines
{
    public interface IInverterStateMachine : IDisposable
    {
        #region Methods

        /// <summary>
        /// Change state.
        /// </summary>
        /// <param name="newState">A new <see cref="IInverterState"/> state.</param>
        void ChangeState(IInverterState newState);

        /// <summary>
        /// Enqueue message.
        /// </summary>
        /// <param name="message">A <see cref="InverterMessage"/> message to enqueue.</param>
        void EnqueueMessage(InverterMessage message);

        /// <summary>
        /// On publishing a given notification message.
        /// </summary>
        /// <param name="notificationMessage">THe <see cref="FieldNotificationMessage"/> message to be published.</param>
        void PublishNotificationEvent(FieldNotificationMessage notificationMessage);

        /// <summary>
        /// Start states machine.
        /// </summary>
        void Start();

        /// <summary>
        /// Process a given message.
        /// </summary>
        /// <param name="message">The <see cref="InverterMessage"/> message to be processed.</param>
        /// <returns></returns>
        bool ValidateCommandMessage(InverterMessage message);

        /// <summary>
        /// Validates the received status work against current state issued control word
        /// </summary>
        /// <param name="message">Inverter message containing last status word read</param>
        /// <returns>True if the status word confirms that the issued control word command has been executed by the device</returns>
        bool ValidateCommandResponse(InverterMessage message);

        #endregion
    }
}
