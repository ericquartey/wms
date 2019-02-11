using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Categoria Merceologica Articolo
    public sealed class ItemCategory : IDataModel
    {
        #region Properties

        public string Description { get; set; }

        public int Id { get; set; }

        public IEnumerable<Item> Items { get; set; }

        #endregion
    }
}
