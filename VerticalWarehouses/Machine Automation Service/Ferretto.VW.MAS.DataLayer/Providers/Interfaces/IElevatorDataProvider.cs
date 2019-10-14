using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IElevatorDataProvider
    {
        #region Methods

        ElevatorAxis GetAxis(Orientation orientation);

        ElevatorAxis GetHorizontalAxis();

        LoadingUnit GetLoadingUnitOnBoard();

        ElevatorStructuralProperties GetStructuralProperties();

        ElevatorAxis GetVerticalAxis();

        void LoadLoadingUnit(int loadingUnitId);

        void UnloadLoadingUnit();

        void UpdateVerticalOffset(double newOffset);

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
