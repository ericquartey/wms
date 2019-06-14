using System.ComponentModel.DataAnnotations;

namespace Ferretto.WMS.Data.Core.Models
{
    public sealed class ItemCompartmentType : BaseModel<int>
    {
        #region Properties

        public int CompartmentTypeId { get; set; }

        public int ItemId { get; set; }

        [Positive]
        [Required]
        public double? MaxCapacity { get; set; }

        #endregion
    }
}
