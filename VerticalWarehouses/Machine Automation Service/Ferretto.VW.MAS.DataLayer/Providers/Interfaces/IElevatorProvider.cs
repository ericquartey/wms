using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Methods

        double? GetHorizontalPosition(BayNumber requestingBay);

        double GetVerticalPosition(BayNumber requestingBay);

        void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, BayNumber requestingBay);

        void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay);

        void MoveToVerticalPosition(double targetPosition, FeedRateCategory feedRateCategory, BayNumber requestingBay);

        void MoveVertical(VerticalMovementDirection direction, BayNumber requestingBay);

        void MoveVerticalOfDistance(double distance, BayNumber requestingBay, double feedRate = 1);

        void RepeatVerticalMovement(double upperBoundPosition, double lowerBoundPosition, int totalTestCycleCount, int delayStart, BayNumber requestingBay);

        void RunTorqueCurrentSampling(double displacement, double netWeight, int? loadingUnitId, BayNumber requestingBay);

        void Stop(BayNumber requestingBay);

        #endregion
    }
}
