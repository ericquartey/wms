namespace Ferretto.VW.MAS.DataModels
{
    public sealed class LoadingUnitStatistics
    {
        public double WeightPercentage { get; set; }

        public int CompartmentsCount { get; set; }

        public int TotalMovements { get; set; }

        public double AreaFillRate { get; set; }
    }
}
