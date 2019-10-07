using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.FiniteStateMachines.Providers
{
    public interface IElevatorProvider
    {
        #region Properties

        double HorizontalPosition { get; set; }

        double VerticalPosition { get; set; }

        #endregion

        #region Methods

        void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, int? LoadingUnitId, double? loadingUnitGrossWeight, BayNumber requestingBay);

        void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveToVerticalPosition(double targetPosition, FeedRateCategory feedRateCategory, BayNumber requestingBay,
            MessageActor sender);

        void MoveVertical(VerticalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveVerticalOfDistance(double distance, BayNumber requestingBay, MessageActor sender, double feedRate = 1);

        void RepeatVerticalMovement(double upperBoundPosition, double lowerBoundPosition, int totalTestCycleCount, int delayStart, BayNumber requestingBay, MessageActor sender);

        void RunTorqueCurrentSampling(double displacement, double netWeight, int? loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        void IncreaseDepositAndPickUpCycleQuantity();

        int GetDepositAndPickUpCycleQuantity();

        void ResetDepositAndPickUpCycleQuantity();
        #endregion
    }
}
