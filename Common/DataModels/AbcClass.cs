using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Classe ABC
    public sealed class AbcClass
    {
        #region Properties

        public List<Cell> Cells { get; }

        public string Description { get; set; }

        public string Id { get; set; }

        public List<Item> Items { get; }

        public List<LoadingUnit> LoadingUnits { get; }

        #endregion Properties
    }
}
