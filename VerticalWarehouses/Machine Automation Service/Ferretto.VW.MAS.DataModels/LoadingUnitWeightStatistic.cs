namespace Ferretto.VW.MAS.DataModels
{
    public class LoadingUnitWeightStatistics
    {
        #region Properties

        public string Code { get; set; }

        public decimal Height { get; set; }

        public decimal MaxGrossWeight { get; set; }

        public double MaxRatio { get; set; }

        public decimal Tare { get; set; }

        public int TotalCompartments { get; set; }

        public decimal Weight { get; set; }

        #endregion
    }
}
