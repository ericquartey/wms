﻿using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces
{
    public interface IInverterStatusBase
    {
        #region Properties

        IControlWord CommonControlWord { get; }

        IStatusWord CommonStatusWord { get; }

        bool[] Inputs { get; }

        bool IsStarted { get; }

        ushort OperatingMode { get; set; }

        InverterIndex SystemIndex { get; }

        #endregion

        #region Methods

        bool UpdateInputsStates(bool[] newInputStates);

        #endregion
    }
}
