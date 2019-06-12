namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemListRowMissionInfo
    {
        #region Properties

        public string Code { get; set; }

        [PositiveOrZero]
        public double DispatchedQuantity { get; set; }

        public int Id { get; set; }

        [Positive]
        public double RequestedQuantity { get; set; }

        #endregion
    }
}
