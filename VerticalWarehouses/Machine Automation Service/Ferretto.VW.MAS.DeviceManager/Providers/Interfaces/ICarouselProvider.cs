using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ICarouselProvider
    {
        #region Methods

        ActionPolicy CanMove(VerticalMovementDirection direction, BayNumber bayNumber, MovementCategory movementCategory);

        double GetPosition(BayNumber bayNumber);

        void Homing(Calibration calibration, BayNumber bayNumber, MessageActor sender);

        bool IsOnlyUpperPositionOccupied(BayNumber bayNumber);

        void Move(VerticalMovementDirection direction, int? loadingUnitId, BayNumber bayNumber, MessageActor sender);

        void MoveAssisted(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void MoveManual(VerticalMovementDirection direction, double distance, BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
