namespace Ferretto.VW.MAS.DataModels
{
    public sealed class LoadingUnitWeightStatistics
    {
        #region Properties

        public string Code { get; set; }

        public int? CompartmentsCount { get; set; }

        public double GrossWeight { get; set; }

        public double Height { get; set; }

        public double MaxNetWeight { get; set; }

        public double MaxWeightPercentage { get; set; }

        public double Tare { get; set; }

        #endregion
    }
}
