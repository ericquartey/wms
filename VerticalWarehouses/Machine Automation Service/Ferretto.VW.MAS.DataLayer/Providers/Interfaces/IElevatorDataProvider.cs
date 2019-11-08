using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IElevatorDataProvider
    {
        #region Properties

        double HorizontalPosition { get; set; }

        double VerticalPosition { get; set; }

        #endregion

        #region Methods

        ElevatorAxis GetAxis(Orientation orientation);

        IDbContextTransaction GetContextTransaction();

        ElevatorAxis GetHorizontalAxis();

        LoadingUnit GetLoadingUnitOnBoard();

        ElevatorStructuralProperties GetStructuralProperties();

        ElevatorAxis GetVerticalAxis();

        void LoadLoadingUnit(int loadingUnitId);

        void ResetMachine();

        MovementParameters ScaleMovementsByWeight(Orientation orientation);

        void UnloadLoadingUnit();

        void UpdateVerticalOffset(double newOffset);

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
