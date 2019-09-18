using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Methods

        decimal GetHorizontalPosition(BayNumber bayNumber);

        decimal GetVerticalPosition(BayNumber bayNumber);

        void MoveHorizontal(HorizontalMovementDirection direction, BayNumber bayNumber);

        void MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory, BayNumber bayNumber);

        void MoveVertical(VerticalMovementDirection direction, BayNumber bayNumber);

        void MoveVerticalOfDistance(decimal distance, BayNumber bayNumber);

        void RunInMotionCurrentSampling(decimal displacement, decimal netWeight);

        void RunInPlaceCurrentSampling(TimeSpan inPlaceSamplingDuration, decimal netWeight);

        void RunTorqueCurrentSampling(decimal displacement, decimal netWeight, int? loadingUnitId);

        void Stop(BayNumber bayNumber);

        void UpdateResolution(decimal newResolution);

        #endregion
    }
}
