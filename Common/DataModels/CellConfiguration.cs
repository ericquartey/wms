using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Configurazione Cella
    public sealed class CellConfiguration : IDataModel
    {
        #region Properties

        public IEnumerable<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes
        {
            get;
            set;
        }

        public IEnumerable<CellConfigurationCellType> CellConfigurationCellTypes { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        #endregion
    }
}
