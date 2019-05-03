using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitExecution : Model<int>
    {
        #region Properties

        public DateTime? LastPickDate { get; set; }

        #endregion
    }
}
