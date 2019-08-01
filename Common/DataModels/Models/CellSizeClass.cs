﻿using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Dimensione Cella
    public sealed class CellSizeClass : IDataModel<int>
    {
        #region Properties

        public IEnumerable<CellType> CellTypes { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public double Depth { get; set; }

        public double Width { get; set; }

        #endregion
    }
}
