using System;

namespace Ferretto.VW.MAS_InverterDriver.Interface.StateMachines
{
    public interface IInverterState : IDisposable
    {
        #region Properties

        string Type { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Process message to apply state business logic.
        /// </summary>
        /// <param name="message">Inverter driver message to be evaluated</param>
        /// <returns><c>True</c> if the message satisfied state business logic and caused a state transition, <c>False</c> otherwise.</returns>
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
