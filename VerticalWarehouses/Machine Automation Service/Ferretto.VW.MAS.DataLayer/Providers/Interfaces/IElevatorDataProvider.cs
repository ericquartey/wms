using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IElevatorDataProvider
    {
        #region Properties

        /// <summary>
        /// The horizontal position, in millimeters, of the elevator's chain.
        /// </summary>
        double HorizontalPosition { get; set; }

        /// <summary>
        /// The vertical position, in millimeters, of the elevator.
        /// </summary>
        double VerticalPosition { get; set; }

        #endregion

        #region Methods

        ElevatorAxisManualParameters GetAssistedMovementsAxis(Orientation orientation);

        ElevatorAxis GetAxis(Orientation orientation);

        IDbContextTransaction GetContextTransaction();

        /// <summary>
        /// Gets the bay position where the elevator is currently located, or null if the elevator is not located opposite a bay.
        /// </summary>
        BayPosition GetCurrentBayPosition();

        /// <summary>
        /// Gets the cell where the elevator is currently located, or null if the elevator is not located opposite a cell.
        /// </summary>
        Cell GetCurrentCell();

        LoadingUnit GetLoadingUnitOnBoard();

        ElevatorAxisManualParameters GetManualMovementsAxis(Orientation orientation);

        ElevatorStructuralProperties GetStructuralProperties();

        void ResetMachine();

        MovementParameters ScaleMovementsByWeight(Orientation orientation);

        /// <summary>
        /// Sets the bay position where the elevator is currently located, or null if the elevator is not located opposite a bay.
        /// </summary>
        void SetCurrentBayPosition(int? bayPositionId);

        /// <summary>
        /// Gets the cell where the elevator is currently located, or null if the elevator is not located opposite a cell.
        /// </summary>
        void SetCurrentCell(int? cellId);

        void SetLoadingUnit(int? loadingUnitId);

        void UpdateLastIdealPosition(double position, Orientation orientation = Orientation.Horizontal);

        void UpdateVerticalOffset(double newOffset);

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
