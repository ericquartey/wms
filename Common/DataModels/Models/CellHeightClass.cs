﻿using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Altezza Cella
    public sealed class CellHeightClass : IDataModel<int>
    {
        #region Properties

        public IEnumerable<CellType> CellTypes { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public double MaxHeight { get; set; }

        public double MinHeight { get; set; }

        #endregion
    }
}
