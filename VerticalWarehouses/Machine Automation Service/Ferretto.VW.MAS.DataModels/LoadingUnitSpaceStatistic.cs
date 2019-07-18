namespace Ferretto.VW.MAS.DataModels
{
    public class LoadingUnitSpaceStatistics
    {
        #region Properties

        public int CompartmentsFilled { get; set; }

        public decimal Depth { get; set; }

        public double RatioFillingCompartments { get; set; }

        public int TotalCompartments { get; set; }

        public int TotalMissions { get; set; }

        public decimal Width { get; set; }

        #endregion
    }
}
