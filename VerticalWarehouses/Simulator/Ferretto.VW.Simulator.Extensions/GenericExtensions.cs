using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Simulator
{
    public static class Extensions
    {
        #region Methods

        public static string FormatWith(this string value, params object[] args)
        {
            return string.Format(value, args);
        }

        #endregion
    }
}
