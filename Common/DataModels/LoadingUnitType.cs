using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Udc
    public sealed class LoadingUnitType
    {
        #region Properties

        public IEnumerable<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes { get; set; }

        public IEnumerable<DefaultLoadingUnit> DefaultLoadingUnits { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public LoadingUnitHeightClass LoadingUnitHeightClass { get; set; }
        public int LoadingUnitHeightClassId { get; set; }
        public IEnumerable<LoadingUnit> LoadingUnits { get; set; }
        public LoadingUnitSizeClass LoadingUnitSizeClass { get; set; }
        public int LoadingUnitSizeClassId { get; set; }
        public IEnumerable<LoadingUnitTypeAisle> LoadingUnitTypeAisles { get; set; }
        public LoadingUnitWeightClass LoadingUnitWeightClass { get; set; }
        public int LoadingUnitWeightClassId { get; set; }
        public IEnumerable<SchedulerRequest> SchedulerRequests { get; set; }

        #endregion Properties
    }
}
