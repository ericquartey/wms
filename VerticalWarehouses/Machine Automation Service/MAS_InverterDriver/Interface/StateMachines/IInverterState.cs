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
        /// <returns>True if the message satisfied state business logic and caused a state transition, false otherwise</returns>
        bool ProcessMessage(InverterMessage message);

        #endregion
    }
}
