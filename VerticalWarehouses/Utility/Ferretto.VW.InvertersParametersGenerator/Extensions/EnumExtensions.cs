using System;

namespace Ferretto.VW.InvertersParametersGenerator.Extensions
{
    public static class InverterIndexExtensions
    {
        #region Methods

        public static InverterIndex Next<InverterIndex>(this InverterIndex src)
        {
            var Arr = (InverterIndex[])Enum.GetValues(src.GetType());
            var j = Array.IndexOf(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        #endregion
    }
}
