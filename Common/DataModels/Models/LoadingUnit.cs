using System;
using System.Collections.Generic;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.Common.DataModels
{
    // Unità di Carico
    public sealed class LoadingUnit : ITimestamped, IDataModel<int>
    {
        #region Properties

        public AbcClass AbcClass { get; set; }

        public string AbcClassId { get; set; }

        public Cell Cell { get; set; }

        public int? CellId { get; set; }

        public CellPosition CellPosition { get; set; }

        public int? CellPositionId { get; set; }

        public string Code { get; set; }

        public IEnumerable<Compartment> Compartments { get; set; }

        public DateTime CreationDate { get; set; }

        public int? HandlingParametersCorrection { get; set; }

        public double Height { get; set; }

        public int Id { get; set; }

        public DateTime? InventoryDate { get; set; }

        public bool IsCellPairingFixed { get; set; }

        public DateTime? LastHandlingDate { get; set; }

        public DateTime LastModificationDate { get; set; }

        public DateTime? LastPickDate { get; set; }

        public DateTime? LastPutDate { get; set; }

        public LoadingUnitStatus LoadingUnitStatus { get; set; }

        public string LoadingUnitStatusId { get; set; }

        public LoadingUnitType LoadingUnitType { get; set; }

        public int LoadingUnitTypeId { get; set; }

        public int MissionsCount { get; set; }

        public IEnumerable<Mission> Missions { get; set; }

        public string Note { get; set; }

        public Enums.ReferenceType ReferenceType { get; set; }

        public IEnumerable<SchedulerRequest> SchedulerRequests { get; set; }

        public int Weight { get; set; }

        #endregion
    }
}
