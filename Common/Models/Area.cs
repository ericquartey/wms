using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Area
    public sealed class Area
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public IEnumerable<LoadingUnitRange> LoadingUnitRanges { get; set; }
        public IEnumerable<Aisle> Aisles { get; set; }
        public IEnumerable<ItemArea> AreaItems { get; set; }
        public IEnumerable<ItemList> ItemLists { get; set; }
    }
}
