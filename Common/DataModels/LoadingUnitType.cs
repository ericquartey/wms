using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Udc
    public sealed class LoadingUnitType
    {
        public int Id { get; set; }
        public int LoadingUnitHeightClassId { get; set; }
        public int LoadingUnitWeightClassId { get; set; }
        public int LoadingUnitSizeClassId { get; set; }
        public string Description { get; set; }

        public LoadingUnitHeightClass LoadingUnitHeightClass { get; set; }
        public LoadingUnitWeightClass LoadingUnitWeightClass { get; set; }
        public LoadingUnitSizeClass LoadingUnitSizeClass { get; set; }

        public IEnumerable<LoadingUnit> LoadingUnits { get; set; }
        public IEnumerable<LoadingUnitTypeAisle> LoadingUnitTypeAisles { get; set; }

        public IEnumerable<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes
        {
            get;
            set;
        }

        public IEnumerable<DefaultLoadingUnit> DefaultLoadingUnits { get; set; }
    }
}
