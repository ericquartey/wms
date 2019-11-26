using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class ObjectExtensions
    {
        #region Methods

        public static bool IsNotNull(this object obj)
        {
            return !(obj is null);
        }

        public static bool IsNull(this object obj)
        {
            return obj is null;
        }

        #endregion
    }
}
