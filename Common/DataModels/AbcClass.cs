using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe ABC
    public sealed class AbcClass
    {
        #region Properties

        public List<Cell> Cells { get; set; }

        public string Description { get; set; }

        public string Id { get; set; }

        public List<Item> Items { get; set; }

        public List<LoadingUnit> LoadingUnits { get; set; }

        #endregion
    }
}
