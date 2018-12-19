using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Dimensione Cella
    public sealed class CellSizeClass : IDataModel
    {
        #region Properties

        public IEnumerable<CellType> CellTypes { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public int Length { get; set; }

        public int Width { get; set; }

        #endregion Properties
    }
}
