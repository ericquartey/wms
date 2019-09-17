using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Methods

        decimal? GetHorizontalPosition();

        decimal? GetVerticalPosition();

        void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isStartedOnBoard, decimal position);

        void MoveHorizontalManual(HorizontalMovementDirection direction);

        void MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory);

        void MoveVertical(VerticalMovementDirection direction);

        void MoveVerticalOfDistance(decimal distance);

        void RunTorqueCurrentSampling(decimal displacement, decimal netWeight, int? loadingUnitId);

        void Stop();

        void UpdateResolution(decimal newResolution);

        #endregion
    }
}
