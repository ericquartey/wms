using System;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class LoadingUnitDetails
    {
        #region Constructors

        public LoadingUnitDetails()
        {
        }

        #endregion

        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        [JsonIgnore]
        public int ActiveMissionsCount { get; set; }

        [JsonIgnore]
        public int ActiveSchedulerRequestsCount { get; set; }

        public int? AisleId { get; set; }

        public double? AreaFillRate { get; set; }

        public int? AreaId { get; set; }

        public string AreaName { get; set; }

        public int? CellId { get; set; }

        public string CellPositionDescription { get; set; }

        public int? CellPositionId { get; set; }

        public Side CellSide { get; set; }

        public string Code { get; set; }

        public int CompartmentsCount { get; set; }

        public DateTime CreationDate { get; set; }

        public double Depth { get; set; }

        public double EmptyWeight { get; set; }

        public int? HandlingParametersCorrection { get; set; }

        public double Height { get; set; }

        public int Id { get; set; }

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

        public int? MachineId { get; internal set; }

        public int MaxNetWeight { get; set; }

        public int MissionsCount { get; set; }

        public string Note { get; set; }

        public ReferenceType ReferenceType { get; set; }

        public int Weight { get; set; }

        public double Width { get; set; }

        #endregion
    }
}
