using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IElevatorDataProvider
    {
        #region Methods

        ElevatorAxis GetAxis(Orientation orientation);

        int GetDepositAndPickUpCycleQuantity();

        ElevatorAxis GetHorizontalAxis();

        LoadingUnit GetLoadingUnitOnBoard();

        ElevatorStructuralProperties GetStructuralProperties();

        ElevatorAxis GetVerticalAxis();

        void IncreaseDepositAndPickUpCycleQuantity();

        void ResetDepositAndPickUpCycleQuantity();

        void SetLoadingUnitOnBoard(int? loadingUnitId);

        void UpdateVerticalOffset(double newOffset);

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
