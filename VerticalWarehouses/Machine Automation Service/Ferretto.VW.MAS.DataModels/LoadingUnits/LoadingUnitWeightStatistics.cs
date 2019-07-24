namespace Ferretto.VW.MAS.DataModels.LoadingUnits
{
    public class LoadingUnitWeightStatistics
    {
        #region Properties

        public string Code { get; set; }

        public int? CompartmentsCount { get; set; }

        public decimal GrossWeight { get; set; }

        public decimal Height { get; set; }

        public decimal MaxNetWeight { get; set; }

        public decimal MaxWeightPercentage { get; set; }

        public decimal Tare { get; set; }

        #endregion
    }
}
