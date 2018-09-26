using System.Collections.Generic;

namespace Ferretto.Common.DataModels
{
    // Unità di Misura
    public sealed class MeasureUnit
    {
        public string Id { get; set; }
        public string Description { get; set; }

        public IEnumerable<Item> Items { get; set; }
    }
}
