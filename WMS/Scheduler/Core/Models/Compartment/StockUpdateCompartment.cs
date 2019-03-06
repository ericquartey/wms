namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class StockUpdateCompartment : Model
    {
        #region Properties

        public bool IsItemPairingFixed { get; set; }

        public int? ItemId { get; set; }

        public System.DateTime? LastPickDate { get; set; }

        public int ReservedForPick { get; set; }

        public int Stock { get; set; }

        #endregion
    }
}
