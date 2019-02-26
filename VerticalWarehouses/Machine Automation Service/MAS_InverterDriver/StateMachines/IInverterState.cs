using System;

namespace Ferretto.VW.InverterDriver.StateMachines
{
    public interface IInverterState
    {
        #region Properties

        String Type { get; }

        #endregion

        #region Methods

        void NotifyMessage(InverterMessage message);

        #endregion
    }
}
