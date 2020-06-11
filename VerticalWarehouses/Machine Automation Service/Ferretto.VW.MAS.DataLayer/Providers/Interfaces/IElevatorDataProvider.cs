﻿using System.Collections.Generic;
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

        void CompleteProcedure();

        ElevatorAxisManualParameters GetAssistedMovementsAxis(Orientation orientation);

        ElevatorAxis GetAxis(Orientation orientation);

        BayPosition GetCachedCurrentBayPosition();

        Cell GetCachedCurrentCell();

        IDbContextTransaction GetContextTransaction();

        /// <summary>
        /// Gets the bay position where the elevator is currently located, or null if the elevator is not located opposite a bay.
        /// </summary>
        BayPosition GetCurrentBayPosition();

        /// <summary>
        /// Gets the cell where the elevator is currently located, or null if the elevator is not located opposite a cell.
        /// </summary>
        Cell GetCurrentCell();

        int GetCyclesFromCalibration(Orientation orientation = Orientation.Horizontal);

        IEnumerable<ElevatorAxis> GetElevatorAxes();

        LoadingUnit GetLoadingUnitOnBoard();

        ElevatorAxisManualParameters GetManualMovementsAxis(Orientation orientation);

        ElevatorStructuralProperties GetStructuralProperties();

        bool IsVerticalPositionWithinTolerance(double position);

        void ResetMachine();

        MovementParameters ScaleMovementsByWeight(Orientation orientation, bool isLoadingUnitOnBoard);

        /// <summary>
        /// Sets the bay position where the elevator is currently located, or null if the elevator is not located opposite a bay.
        /// </summary>
        void SetCurrentBayPosition(int? bayPositionId);

        /// <summary>
        /// Gets the cell where the elevator is currently located, or null if the elevator is not located opposite a cell.
        /// </summary>
        void SetCurrentCell(int? cellId);

        void SetLoadingUnit(int? loadingUnitId);

        void UpdateHorizontalDistance(double newDistance);

        void UpdateLastCalibrationCycles(Orientation orientation = Orientation.Horizontal);

        void UpdateLastIdealPosition(double position, Orientation orientation = Orientation.Horizontal);

        void UpdateVerticalLowerBound(double newLowerBound);

        void UpdateVerticalOffset(double newOffset);

        void UpdateVerticalOffsetAndComplete(double newOffset);

        void UpdateVerticalResolution(double newResolution);

        #endregion
    }
}
