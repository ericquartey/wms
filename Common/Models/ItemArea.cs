namespace Ferretto.Common.Models
{
    // Articolo-Area
    public partial class ItemArea
    {
        public int ItemId { get; set; }
        public int AreaId { get; set; }

        public Item Item { get; set; }
        public Area Area { get; set; }
    }
}
