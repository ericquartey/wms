using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class ItemDraperyDataConfirm
    {
        #region Properties

        public double? AvailableQuantity { get; set; }

        public string Barcode { get; set; }

        public int BarcodeLength { get; set; }

        public bool CanInputQuantity { get; set; }

        public bool CloseLine { get; set; }

        public double? InputQuantity { get; set; }

        public bool IsPartiallyCompleteOperation { get; set; }

        public string ItemDescription { get; set; }

        public int ItemId { get; set; }

        public int LoadingUnitId { get; set; }

        public string MeasureUnitTxt { get; set; }

        public int MissionId { get; set; }

        public MissionOperationType MissionOperationType { get; set; }

        public double MissionRequestedQuantity { get; set; }

        public double QuantityIncrement { get; set; }

        public int? QuantityTolerance { get; set; }

        #endregion
    }
}
