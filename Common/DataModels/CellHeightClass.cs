using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Altezza Cella
    public sealed class CellHeightClass : IDataModel
    {
        #region Properties

        public IEnumerable<CellType> CellTypes { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public int MaxHeight { get; set; }

        public int MinHeight { get; set; }

        #endregion Properties
    }
}
