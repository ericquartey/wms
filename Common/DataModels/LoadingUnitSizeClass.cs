using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Dimensione Udc
    public sealed class LoadingUnitSizeClass : IDataModel
    {
        #region Properties

        public int? BayForksUnthread { get; set; }

        public int? BayOffset { get; set; }

        public int? CellForksUnthread { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public int Length { get; set; }

        public int? Lift { get; set; }

        public IEnumerable<LoadingUnitType> LoadingUnitTypes { get; set; }

        public int Width { get; set; }

        #endregion
    }
}
