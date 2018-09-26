using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe Altezza Udc
    public sealed class LoadingUnitHeightClass
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }

        public IEnumerable<LoadingUnitType> LoadingUnitTypes { get; set; }
    }
}
