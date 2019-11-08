namespace Ferretto.VW.MAS.DataLayer
{
    internal interface IElevatorVolatileDataProvider
    {
        #region Properties

        double HorizontalPosition { get; set; }

        double VerticalPosition { get; set; }

        #endregion
    }
}
