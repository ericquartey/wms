using Ferretto.VW.CommonUtils.Messages.Enumerations;
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

        void ContinuePositioning(BayNumber requestingBay, MessageActor sender);

        AxisBounds GetVerticalBounds();

        void MoveHorizontalAuto(
            HorizontalMovementDirection direction,
            bool isStartedOnBoard,
            int? loadingUnitId,
            double? loadingUnitGrossWeight,
            bool waitContinue,
            bool measure,
            BayNumber requestingBay,
            MessageActor sender);

        void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveHorizontalProfileCalibration(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveToBayPosition(int bayPositionId, double feedRate, bool computeElongation, BayNumber bayNumber, MessageActor automationService);

        void MoveToCell(int cellId, double feedRate, bool computeElongation, BayNumber bayNumber, MessageActor automationService);

        void MoveToAbsoluteVerticalPosition(double targetPosition, double feedRate, bool measure, bool computeElongation, BayNumber requestingBay, MessageActor sender);

        void MoveVertical(VerticalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveToRelativeVerticalPosition(double distance, BayNumber requestingBay, MessageActor sender, double feedRate = 1);

        void RunTorqueCurrentSampling(double displacement, double netWeight, int? loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void StartBeltBurnishing(
            double upperBoundPosition,
            double lowerBoundPosition,
            int delayStart, BayNumber
            requestingBay,
            MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        #endregion
    }
}
