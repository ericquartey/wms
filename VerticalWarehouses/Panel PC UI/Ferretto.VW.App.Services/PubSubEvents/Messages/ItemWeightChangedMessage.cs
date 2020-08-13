namespace Ferretto.VW.App.Services
{
    public class ItemWeightChangedMessage
    {
        #region Constructors

        public ItemWeightChangedMessage(double? measureadQuantity, double? requestedQuantity)
        {
            this.MeasureadQuantity = measureadQuantity;
            this.RequestedQuantity = requestedQuantity;
        }

        #endregion

        #region Properties

        public double? MeasureadQuantity { get; }
        public double? RequestedQuantity { get; }

        #endregion
    }
}
