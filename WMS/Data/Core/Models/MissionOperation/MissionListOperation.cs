namespace Ferretto.WMS.Data.Core.Models
{
    public sealed class MissionListOperation : MissionOperation
    {
        #region Properties

        public ItemList ItemList { get; set; }

        public int ItemListId { get; set; }

        public ItemListRow ItemListRow { get; set; }

        public int ItemListRowId { get; set; }

        #endregion
    }
}
