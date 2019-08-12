using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(ItemCompartmentType))]
    public sealed class ItemCompartmentType : BaseModel<(int ItemId, int CompartmentTypeId)>
    {
        #region Properties

        public int CompartmentsCount { get; set; }

        public int CompartmentTypeId { get; set; }

        public int EmptyCompartmentsCount { get; set; }

        public double? Depth { get; set; }

        public new (int, int) Id => (this.ItemId, this.CompartmentTypeId);

        public int ItemId { get; set; }

        [Positive]
        [Required]
        public double MaxCapacity { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
