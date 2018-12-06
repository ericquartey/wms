using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Tipo Cella
    public sealed class CellType : IDataModel
    {
        #region Properties

        public IEnumerable<CellConfigurationCellType> CellConfigurationCellTypes { get; set; }

        public CellHeightClass CellHeightClass { get; set; }

        public int CellHeightClassId { get; set; }

        public IEnumerable<Cell> Cells { get; set; }

        public CellSizeClass CellSizeClass { get; set; }

        public int CellSizeClassId { get; set; }

        public IEnumerable<CellTotal> CellTotals { get; set; }

        public IEnumerable<CellTypeAisle> CellTypeAisles { get; set; }

        public CellWeightClass CellWeightClass { get; set; }

        public int CellWeightClassId { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        #endregion Properties
    }
}
