namespace Ferretto.Common.DataModels
{
    // Articolo-Area
    public sealed class ItemArea
    {
        #region Properties

        public Area Area { get; set; }

        public int AreaId { get; set; }

        public Item Item { get; set; }

        public int ItemId { get; set; }

        #endregion
    }
}
