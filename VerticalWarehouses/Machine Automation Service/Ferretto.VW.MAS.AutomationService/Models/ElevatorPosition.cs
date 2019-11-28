namespace Ferretto.VW.MAS.AutomationService
{
    public sealed class ElevatorPosition
    {
        #region Properties

        public int? BayPositionId { get; set; }

        public int? CellId { get; set; }

        public double Horizontal { get; set; }

        public double Vertical { get; set; }

        #endregion
    }
}
