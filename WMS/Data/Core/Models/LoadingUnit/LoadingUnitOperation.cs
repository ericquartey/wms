using System;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(LoadingUnit))]
    public class LoadingUnitOperation : BaseModel<int>
    {
        #region Properties

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        #endregion
    }
}
