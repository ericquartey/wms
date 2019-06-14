using Ferretto.Common.Utils;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemCompartmentType))]
    public sealed class ItemCompartmentType : BaseModel<int>
    {
        #region Properties

        public int CompartmentsCount { get; set; }

        public int CompartmentTypeId { get; set; }

        public int EmptyCompartmentsCount { get; set; }

        public double? Height { get; set; }

        public int ItemId { get; set; }

        [Positive]
        [Required]
        public double? MaxCapacity { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
