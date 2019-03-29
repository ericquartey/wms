using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Posizione in Cella
    public sealed class CellPosition : IDataModel
    {
        #region Properties

        public IEnumerable<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes
        {
            get;
            set;
        }

        public string Description { get; set; }

        public int Id { get; set; }

        public IEnumerable<LoadingUnit> LoadingUnits { get; set; }

        public double? XOffset { get; set; }

        public double? YOffset { get; set; }

        public double? ZOffset { get; set; }

        #endregion
    }
}
