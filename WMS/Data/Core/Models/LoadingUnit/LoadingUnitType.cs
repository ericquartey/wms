using System.Collections.Generic;
using System.Linq;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitType : BaseModel<int>
    {
        #region Properties

        public string Description { get; set; }

        public int HasCompartments { get; set; }

        public int LoadingUnitHeightClassId { get; set; }

        public int LoadingUnitSizeClassId { get; set; }

        public int LoadingUnitWeightClassId { get; set; }

        #endregion
    }
}
