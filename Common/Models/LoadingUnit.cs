using System;
using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Unità di Carico
    public partial class LoadingUnit
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public int CellId { get; set; }
        public Pairing CellPairing { get; set; }
        public int CellPositionId { get; set; }
        public int LoadingUnitTypeId { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public int LoadingUnitStatusId { get; set; }
        public ReferenceType Reference { get; set; }
        public AbcClass Class { get; set; }
        public int HandlingParametersCorrection { get; set; }
        public int InCycleCount { get; set; }
        public int OutCycleCount { get; set; }
        public int OtherCycleCount { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastHandlingDate { get; set; }
        public DateTime? InventoryDate { get; set; }
        public DateTime? LastPickDate { get; set; }
        public DateTime? LastStoreDate { get; set; }
        public string Note { get; set; }

        public Cell Cell { get; set; }
        public CellPosition CellPosition { get; set; }
        public LoadingUnitType LoadingUnitType { get; set; }
        public LoadingUnitStatus LoadingUnitStatus { get; set; }

        public List<Compartment> Compartments { get; set; }
    }
}
