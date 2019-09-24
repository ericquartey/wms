using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorDataProvider
    {
        #region Methods

        ElevatorAxis GetHorizontalAxis();

        decimal GetMaximumLoadOnBoard();

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
