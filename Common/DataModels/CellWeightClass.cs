using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Peso Cella
    public sealed class CellWeightClass : IDataModel
    {
        #region Properties

        public IEnumerable<CellType> CellTypes { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public int MaxWeight { get; set; }

        public int MinWeight { get; set; }

        #endregion
    }
}
