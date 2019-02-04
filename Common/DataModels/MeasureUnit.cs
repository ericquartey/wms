using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Unità di Misura
    public sealed class MeasureUnit
    {
        #region Properties

        public string Description { get; set; }

        public string Id { get; set; }

        public IEnumerable<Item> Items { get; set; }

        #endregion
    }
}
