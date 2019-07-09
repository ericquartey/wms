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
        /// Release all resources
        /// </summary>
        void Release();

        /// <summary>
        /// Starts executing the current state logic
        /// </summary>
        void Start();

        /// <summary>
        /// Executes stop action in the current state to stop running Inverter States Machine
        /// </summary>
        void Stop();

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
