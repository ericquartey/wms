using System;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class WarningAttribute : Attribute
    {
        #region Constructors

        public WarningAttribute(WarningsArea area)
        {
            this.Area = area;
        }

        #endregion

        #region Properties

        public WarningsArea Area { get; set; }

        #endregion
    }
}
