using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorDataProvider
    {
        #region Methods

        ElevatorAxis GetAxis(Orientation orientation);

        ElevatorAxis GetHorizontalAxis();

        LoadingUnit GetLoadingUnitOnBoard();

        double GetMaximumLoadOnBoard();

        ElevatorStructuralProperties GetStructuralProperties();

        ElevatorAxis GetVerticalAxis();

        void UpdateVerticalOffset(double newOffset);

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
