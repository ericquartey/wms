namespace Ferretto.WMS.Data.Core.Models
{
    public class Compartment : BaseModel<int>
    {
        #region Properties

        public string CompartmentStatusDescription { get; set; }

        public bool HasRotation { get; set; }

        public double? Height { get; set; }

        public bool IsItemPairingFixed { get; set; }

        public string ItemDescription { get; set; }

        public int? ItemId { get; set; }

        public string ItemMeasureUnit { get; set; }

        public string LoadingUnitCode { get; set; }

        public int LoadingUnitId { get; set; }

        public string Lot { get; set; }

        public string MaterialStatusDescription { get; set; }

        public int Stock { get; set; }

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        public double? Width { get; set; }

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        #endregion
    }
}
