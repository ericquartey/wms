using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public class ItemAddedToLoadingUnitDetail
    {
        #region Properties

        public int CompartmentId { get; set; }

        public string ItemDescription { get; set; }

        public int ItemId { get; set; }

        public int LoadingUnitId { get; set; }

        public string MeasureUnitTxt { get; set; }

        public MissionOperation MissionOperation { get; set; }

        public double QuantityIncrement { get; set; }

        public int? QuantityTolerance { get; set; }

        #endregion
    }
}
