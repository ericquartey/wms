using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Stato Cella
    public sealed class CellStatus : IDataModel
    {
        #region Properties

        public IEnumerable<Cell> Cells { get; set; }

        public IEnumerable<CellTotal> CellTotals { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        #endregion Properties
    }
}
