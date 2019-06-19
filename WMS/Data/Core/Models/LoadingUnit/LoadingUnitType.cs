using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(LoadingUnitType))]
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
