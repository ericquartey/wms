﻿using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Peso Udc
    public sealed class LoadingUnitWeightClass : IDataModel<int>
    {
        #region Properties

        public string Description { get; set; }

        public int Id { get; set; }

        public IEnumerable<LoadingUnitType> LoadingUnitTypes { get; set; }

        public int MaxWeight { get; set; }

        public int MinWeight { get; set; }

        #endregion
    }
}
