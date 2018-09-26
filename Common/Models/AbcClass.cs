using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe ABC
    public sealed class AbcClass
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public List<Cell> Cells { get; set; }
        public List<Item> Items { get; set; }
        public List<LoadingUnit> LoadingUnits { get; set; }
    }
}
