using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IElevatorDataProvider
    {
        #region Methods

        ElevatorAxis GetAxis(Orientation orientation);

        IDbContextTransaction GetContextTransaction();

        ElevatorAxis GetHorizontalAxis();

        LoadingUnit GetLoadingUnitOnBoard();

        ElevatorStructuralProperties GetStructuralProperties();

        ElevatorAxis GetVerticalAxis();

        void LoadLoadingUnit(int loadingUnitId);

        MovementParameters ScaleMovementsByWeight(Orientation orientation);

        void UnloadLoadingUnit();

        void UpdateVerticalOffset(double newOffset);

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
