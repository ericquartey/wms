using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(LoadingUnit))]
    public class LoadingUnitCreating : BaseModel<int>
    {
        #region Properties

        public string AbcClassId { get; set; }

        public int? AisleId { get; set; }

        public int? AreaId { get; set; }

        public int? CellId { get; set; }

        public int? CellPositionId { get; set; }

        [Required]
        public string Code { get; set; }

        [PositiveOrZero]
        public int? HandlingParametersCorrection { get; set; }

        [Positive]
        public double Height { get; set; }

        public bool IsCellPairingFixed { get; set; }

        public string LoadingUnitStatusId { get; set; }

        public int LoadingUnitTypeId { get; set; }

        public string Note { get; set; }

        public ReferenceType ReferenceType { get; set; }

        [PositiveOrZero]
        public int Weight { get; set; }

        #endregion
    }
}
