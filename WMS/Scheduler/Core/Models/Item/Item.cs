namespace Ferretto.WMS.Scheduler.Core.Models
{
    public sealed class Item : Model
    {
        #region Properties

        public System.DateTime? LastPickDate { get; set; }

        public ItemManagementType ManagementType { get; set; }

        #endregion
    }
}
