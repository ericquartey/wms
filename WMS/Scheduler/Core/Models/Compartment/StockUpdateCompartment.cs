namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class StockUpdateCompartment : Model
    {
        #region Properties

        public bool IsItemPairingFixed { get; set; }

        public int? ItemId { get; set; }

        public System.DateTime? LastPickDate { get; set; }

        public int LoadingUnitId { get; set; }

        public double ReservedForPick { get; set; }

        public double Stock { get; set; }

        #endregion
    }
}
