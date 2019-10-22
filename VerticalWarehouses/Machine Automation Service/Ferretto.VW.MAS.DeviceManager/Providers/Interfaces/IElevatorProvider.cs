﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Properties

        double HorizontalPosition { get; set; }

        double VerticalPosition { get; set; }

        #endregion

        #region Methods

        AxisBounds GetVerticalBounds();

        void MoveHorizontalAuto(
            HorizontalMovementDirection direction,
            bool isStartedOnBoard,
            int? loadingUnitId,
            double? loadingUnitGrossWeight,
            bool measure,
            BayNumber requestingBay,
            MessageActor sender);

        void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveToVerticalPosition(double targetPosition, double feedRate, bool measure, BayNumber requestingBay, MessageActor sender);

        void MoveVertical(VerticalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveVerticalOfDistance(double distance, BayNumber requestingBay, MessageActor sender, double feedRate = 1);

        void RepeatVerticalMovement(
            double upperBoundPosition,
            double lowerBoundPosition,
            int totalTestCycleCount,
            int delayStart, BayNumber
            requestingBay,
            MessageActor sender);

        void RunTorqueCurrentSampling(double displacement, double netWeight, int? loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        #endregion
    }
}
