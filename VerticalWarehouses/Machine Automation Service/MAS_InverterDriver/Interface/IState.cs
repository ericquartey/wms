using System;

namespace Ferretto.VW.MAS_InverterDriver
{
    public interface IState
    {
        #region Properties

        String Type { get; }

        #endregion
    }
}
