using System;

namespace Ferretto.WMS.Data.Core.Models
{
    public class StockUpdateCompartment : Model<int>
    {
        #region Properties

        public bool IsItemPairingFixed { get; set; }

        public int? ItemId { get; set; }

        public DateTime? LastPickDate { get; set; }

        public int LoadingUnitId { get; set; }

        public double ReservedForPick { get; set; }

        public double Stock { get; set; }

        #endregion
    }
}
