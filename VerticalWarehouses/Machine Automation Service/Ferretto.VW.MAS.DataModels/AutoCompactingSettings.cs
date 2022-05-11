using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class AutoCompactingSettings : DataModel
    {
        #region Properties

        public TimeSpan BeginTime { get; set; }

        public bool IsActive { get; set; }

        #endregion
    }
}
