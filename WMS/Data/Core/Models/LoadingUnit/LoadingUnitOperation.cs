using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitOperation : Model<int>
    {
        #region Properties

        public DateTime? LastPickDate { get; set; }

        #endregion
    }
}
