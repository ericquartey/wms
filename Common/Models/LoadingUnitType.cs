using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Tipo Udc
    public partial class LoadingUnitType
    {
        public int Id { get; set; }
        public int LoadingUnitHeightClassId { get; set; }
        public int LoadingUnitWeightClassId { get; set; }
        public int LoadingUnitSizeClassId { get; set; }
        public string Description { get; set; }

        public LoadingUnitHeightClass LoadingUnitHeightClass { get; set; }
        public LoadingUnitWeightClass LoadingUnitWeightClass { get; set; }
        public LoadingUnitSizeClass LoadingUnitSizeClass { get; set; }

        public List<LoadingUnit> LoadingUnits { get; set; }
        public List<LoadingUnitTypeAisle> LoadingUnitTypeAisles { get; set; }
        public List<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes { get; set; }
        public List<DefaultLoadingUnit> DefaultLoadingUnits { get; set; }
    }
}
