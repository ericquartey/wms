using System;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public interface IInverterState : IDisposable
    {
        #region Properties

        string Type { get; }

        #endregion

        #region Methods

        void ProcessMessage(InverterMessage message);

        #endregion
    }
}
