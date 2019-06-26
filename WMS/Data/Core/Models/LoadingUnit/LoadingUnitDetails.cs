using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;
using Newtonsoft.Json;

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

        public Side CellSide { get; set; }

        [Required]
        public string Code { get; set; }

        [PositiveOrZero]
        public int CompartmentsCount { get; set; }

        public DateTime CreationDate { get; set; }

        [PositiveOrZero]
        public double EmptyWeight { get; set; }

        [PositiveOrZero]
        public int? HandlingParametersCorrection { get; set; }

        [Positive]
        public double Height { get; set; }

        [PositiveOrZero]
        public int InMissionCount { get; set; }

        public DateTime? InventoryDate { get; set; }

        public bool IsCellPairingFixed { get; set; }

        public DateTime? LastHandlingDate { get; set; }

        public DateTime? LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        [Positive]
        public double Depth { get; set; }

        public string LoadingUnitStatusDescription { get; set; }

        public string LoadingUnitStatusId { get; set; }

        public string LoadingUnitTypeDescription { get; set; }

        public bool LoadingUnitTypeHasCompartments { get; set; }

        public int LoadingUnitTypeId { get; set; }

        [PositiveOrZero]
        public int MaxNetWeight { get; set; }

        public string Note { get; set; }

        [PositiveOrZero]
        public int OtherMissionCount { get; set; }

        [PositiveOrZero]
        public int OutMissionCount { get; set; }

        public ReferenceType ReferenceType { get; set; }

        [PositiveOrZero]
        public int Weight { get; set; }

        [Positive]
        public double Width { get; set; }

        #endregion
    }
}
