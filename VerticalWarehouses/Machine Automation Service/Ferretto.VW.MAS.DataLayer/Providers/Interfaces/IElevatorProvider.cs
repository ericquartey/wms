using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Models;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Methods

        decimal? GetHorizontalPosition(BayNumber requestingBay);

        decimal GetVerticalPosition(BayNumber requestingBay);

        void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, BayNumber requestingBay);

        void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay);

        void MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory, BayNumber requestingBay);

        void MoveVertical(VerticalMovementDirection direction, BayNumber requestingBay);

        void MoveVerticalOfDistance(decimal distance, BayNumber requestingBay, decimal feedRate = 1);

        void RepeatVerticalMovement(decimal upperBoundPosition, decimal lowerBoundPosition, int totalTestCycleCount, int delayStart, BayNumber requestingBay);

        void RunTorqueCurrentSampling(decimal displacement, decimal netWeight, int? loadingUnitId, BayNumber requestingBay);

        void Stop(BayNumber requestingBay);

        #endregion
    }
}
