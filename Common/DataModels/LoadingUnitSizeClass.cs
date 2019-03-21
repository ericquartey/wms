using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Dimensione Udc
    public sealed class LoadingUnitSizeClass : IDataModel
    {
        #region Properties

        public double? BayForksUnthread { get; set; }

        public double? BayOffset { get; set; }

        public double CellForksUnthread { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public double Length { get; set; }

        public double? Lift { get; set; }

        public IEnumerable<LoadingUnitType> LoadingUnitTypes { get; set; }

        public double Width { get; set; }

        #endregion
    }
}
