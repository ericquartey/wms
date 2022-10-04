using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ICarouselProvider
    {
        #region Methods

        MachineErrorCode CanElevatorDeposit(BayPosition bayPosition);

        MachineErrorCode CanElevatorPickup(BayPosition bayPosition);

        ActionPolicy CanMove(VerticalMovementDirection direction, Bay bay, MovementCategory movementCategory);

        double GetPosition(BayNumber bayNumber);

        void Homing(Calibration calibration, int? loadingUnitId, bool showErrors, BayNumber bayNumber, MessageActor sender, bool bypassSensor);

        bool IsOnlyBottomPositionOccupied(BayNumber bayNumber);

        bool IsOnlyTopPositionOccupied(BayNumber bayNumber);

        void Move(VerticalMovementDirection direction, int? loadUnitId, Bay bay, MessageActor sender);

        void MoveAssisted(VerticalMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void MoveFindZero(BayNumber requestingBay, MessageActor sender);

        void MoveManual(VerticalMovementDirection direction, double distance, int? loadUnitId, bool bypassConditions, Bay bay, MessageActor sender);

        void StartTest(BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        void StopTest(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
