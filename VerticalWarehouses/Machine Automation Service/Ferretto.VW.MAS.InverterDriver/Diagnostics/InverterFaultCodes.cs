using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.InverterDriver.Diagnostics
{
    public static class InverterFaultCodes
    {
        #region Fields

        public static Dictionary<int, string> Errors = new Dictionary<int, string>()
        {
            { 0, "No fault" },
        };

        #endregion

        #region Methods

        public static string GetErrorByCode(int code)
        {
            return Errors.ContainsKey(code) ? Errors[code] : "Unknown error";
        }

        #endregion
    }
}
