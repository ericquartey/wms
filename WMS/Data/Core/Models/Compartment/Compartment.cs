using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Compartment))]
    public class Compartment : BaseModel<int>, ICompartmentDeletePolicy, ICompartmentUpdatePolicy
    {
        #region Properties

        public string AisleName { get; set; }

        public string AreaName { get; set; }

        public string CompartmentStatusDescription { get; set; }

        public bool HasRotation { get; set; }

        [Required]
        [Positive]
        public double? Height { get; set; }

        public bool IsItemPairingFixed { get; set; }

        public string ItemDescription { get; set; }

        public int? ItemId { get; set; }

        public string ItemMeasureUnit { get; set; }

        public string LoadingUnitCode { get; set; }

        public int LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        [PositiveOrZero]
        public double Stock { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        [Required]
        [Positive]
        public double? Width { get; set; }

        [Required]
        [PositiveOrZero]
        public double? XPosition { get; set; }

        [Required]
        [PositiveOrZero]
        public double? YPosition { get; set; }

        #endregion
    }
}
