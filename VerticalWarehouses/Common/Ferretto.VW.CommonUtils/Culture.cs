using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Ferretto.VW.CommonUtils
{
    public static class Culture
    {
        #region Fields

        private static CultureInfo current = CultureInfo.CurrentCulture;

        #endregion

        #region Properties

        public static CultureInfo Actual
        {
            get => current;
            set => current = value;
        }

        #endregion
    }
}
