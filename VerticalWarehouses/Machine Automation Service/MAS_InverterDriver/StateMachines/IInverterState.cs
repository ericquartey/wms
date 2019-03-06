using System;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public interface IInverterState
    {
        #region Properties

        string Type { get; }

        #endregion

        #region Methods

        void NotifyMessage(InverterMessage message);

        #endregion
    }
}
