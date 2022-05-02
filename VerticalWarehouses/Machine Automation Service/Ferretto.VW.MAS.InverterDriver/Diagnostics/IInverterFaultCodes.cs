using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.InverterDriver.Diagnostics
{
    public interface IInverterFaultCodes
    {
        #region Methods

        string GetErrorByCode(int code);

        #endregion
    }
}
