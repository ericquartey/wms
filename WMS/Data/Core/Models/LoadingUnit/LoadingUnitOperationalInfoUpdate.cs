using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(LoadingUnit))]
    public class LoadingUnitOperationalInfoUpdate : BaseModel<int>
    {
        #region Properties

        public int CellId { get; set; }

        [Positive]
        public double Height { get; set; }

        public string LoadingUnitStatusId { get; set; }

        [PositiveOrZero]
        public int Weight { get; set; }

        #endregion
    }
}
