using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class RotationClassSchedule : DataModel
    {
        #region Properties

        public int DaysCount { get; set; }

        public DateTime? LastSchedule { get; set; }

        #endregion
    }
}
