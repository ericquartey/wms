using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Area
    public partial class Area
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<LoadingUnitRange> LoadingUnitRanges { get; set; }
        public List<Aisle> Aisles { get; set; }
        public List<ItemArea> AreaItems { get; set; }
        public List<ItemList> ItemLists { get; set; }
    }
}
