using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(LoadingUnit))]
    public class LoadingUnitDetails : BaseModel<int>, ILoadingUnitDeletePolicy, ILoadingUnitWithdrawPolicy, ILoadingUnitUpdatePolicy
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ActiveMissionsCount { get; set; }

        [JsonIgnore]
        [PositiveOrZero]
        public int ActiveSchedulerRequestsCount { get; set; }

        public int? AisleId { get; set; }

        [PositiveOrZero]
        public double? AreaFillRate { get; set; }

        public int? AreaId { get; set; }

        public string AreaName { get; set; }

        public int? CellId { get; set; }

        public string CellPositionDescription { get; set; }

        public int? CellPositionId { get; set; }

        public Enums.Side CellSide { get; set; }

        [Required]
        [Unique]
        public string Code { get; set; }

        [PositiveOrZero]
        public int CompartmentsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public double Depth { get; set; }

        [PositiveOrZero]
        public double EmptyWeight { get; set; }

        [PositiveOrZero]
        public int? HandlingParametersCorrection { get; set; }

        [Positive]
        public double Height { get; set; }

        public DateTime? InventoryDate { get; set; }

        public bool IsCellPairingFixed { get; set; }

        public DateTime? LastHandlingDate { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        public string LoadingUnitStatusDescription { get; set; }

        public string LoadingUnitStatusId { get; set; }

        public string LoadingUnitTypeDescription { get; set; }

        public bool LoadingUnitTypeHasCompartments { get; set; }

        public int LoadingUnitTypeId { get; set; }

        [PositiveOrZero]
        public int MaxNetWeight { get; set; }

        [PositiveOrZero]
        public int MissionsCount { get; set; }

        public string Note { get; set; }

        public Enums.ReferenceType ReferenceType { get; set; }

        [PositiveOrZero]
        public int Weight { get; set; }

        public double Width { get; set; }

        public int? MachineId { get; internal set; }

        #endregion
    }
}
