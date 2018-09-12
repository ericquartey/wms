using System.Collections.Generic;

namespace Ferretto.Common.DAL.Models
{
    // Configurazione Cella
    public sealed class CellConfiguration
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public IEnumerable<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes
        {
            get;
            set;
        }

        public IEnumerable<CellConfigurationCellType> CellConfigurationCellTypes { get; set; }
    }
}
