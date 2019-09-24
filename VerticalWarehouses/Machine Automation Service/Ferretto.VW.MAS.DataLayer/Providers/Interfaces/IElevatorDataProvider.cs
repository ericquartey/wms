namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorDataProvider
    {
        #region Methods

        decimal GetMaximumLoadOnBoard();

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
