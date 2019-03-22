using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Altezza Udc
    public sealed class LoadingUnitHeightClass : IDataModel
    {
        #region Properties

        public string Description { get; set; }

        public int Id { get; set; }

        public IEnumerable<LoadingUnitType> LoadingUnitTypes { get; set; }

        public double MaxHeight { get; set; }

        public double MinHeight { get; set; }

        #endregion
    }
}
