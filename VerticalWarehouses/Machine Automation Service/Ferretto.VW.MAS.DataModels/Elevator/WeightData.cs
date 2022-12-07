namespace Ferretto.VW.MAS.DataModels
{
    public sealed class WeightData : DataModel
    {
        #region Properties

        public double Current { get; set; }

        public double LUTare { get; set; }

        public double NetWeight { get; set; }

        public WeightCalibrationStep Step { get; set; }

        #endregion
    }
}
