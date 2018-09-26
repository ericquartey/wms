namespace Ferretto.Common.DataModels
{
    // Articolo-Area
    public sealed class ItemArea
    {
        public int ItemId { get; set; }
        public int AreaId { get; set; }

        public Item Item { get; set; }
        public Area Area { get; set; }
    }
}
