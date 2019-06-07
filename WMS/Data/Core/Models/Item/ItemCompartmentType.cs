using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemCompartmentType))]
    public sealed class ItemCompartmentType : BaseModel<int>
    {
        #region Properties

        public int CompartmentTypeId { get; set; }

        public int ItemId { get; set; }

        [Positive]
        public double? MaxCapacity { get; set; }

        #endregion
    }
}
