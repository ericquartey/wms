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

        void MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory);

        void MoveVertical(VerticalMovementDirection direction);

        void MoveVerticalOfDistance(decimal distance);

        void RunInMotionCurrentSampling(decimal displacement, decimal netWeight);

        void RunInPlaceCurrentSampling(TimeSpan inPlaceSamplingDuration, decimal netWeight);

        void Stop();

        void UpdateResolution(decimal newResolution);

        #endregion
    }
}
