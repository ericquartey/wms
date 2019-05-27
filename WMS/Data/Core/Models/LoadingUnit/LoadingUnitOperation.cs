using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitOperation : BaseModel<int>
    {
        #region Properties

        public DateTime? LastPickDate { get; set; }

        #endregion
    }
}
