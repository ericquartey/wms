using System;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorProvider
    {


        #region Methods

        decimal GetHorizontalPosition();

        decimal GetVerticalPosition();

        void MoveHorizontal(HorizontalMovementDirection direction);

        void MoveToVerticalPosition(double targetPosition, FeedRateCategory feedRateCategory);

        void MoveVertical(VerticalMovementDirection direction);

        void MoveVerticalOfDistance(double distance);

        void RunInMotionCurrentSampling(double displacement, double netWeight);

        void RunInPlaceCurrentSampling(TimeSpan inPlaceSamplingDuration, double netWeight);

        void Stop();

        void UpdateResolution(decimal newResolution);

        #endregion
    }
}
