namespace Ferretto.VW.App.Services
{
    public class ItemWeightChangedMessage
    {
        #region Constructors

        public ItemWeightChangedMessage(double? measureadQuantity,
            double? requestedQuantity,
            double? netWeight,
            double? tare,
            double? unitWeight)
        {
            this.MeasureadQuantity = measureadQuantity;
            this.RequestedQuantity = requestedQuantity;
            this.NetWeight = netWeight;
            this.Tare = tare;
            this.UnitWeight = unitWeight;
        }

        #endregion

        #region Properties

        public double? MeasureadQuantity { get; }

        public double? NetWeight { get; }

        public double? RequestedQuantity { get; }

        public double? Tare { get; }

        public double? UnitWeight { get; }

        #endregion
    }
}
