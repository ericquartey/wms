using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Methods

        decimal GetHorizontalPosition();

        decimal GetVerticalPosition();

        void MoveHorizontalAuto(HorizontalMovementDirection direction, bool isOnBoard);

        void MoveHorizontalManual(HorizontalMovementDirection direction);

        void MoveToVerticalPosition(decimal targetPosition, FeedRateCategory feedRateCategory);

        void MoveVertical(VerticalMovementDirection direction);

        void MoveVerticalOfDistance(decimal distance);

        void Stop();

        void UpdateResolution(decimal newResolution);

        #endregion
    }
}
