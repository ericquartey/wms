namespace Ferretto.VW.MAS.AutomationService.Models
{
    public sealed class HomingProcedureParameters
    {
        #region Properties

        public double LowerBound { get; set; }

        public double Offset { get; set; }

        public decimal Resolution { get; set; }

        public double UpperBound { get; set; }

        #endregion
    }
}
