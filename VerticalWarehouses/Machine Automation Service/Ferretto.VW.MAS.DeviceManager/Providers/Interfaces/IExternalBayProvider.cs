using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IExternalBayProvider
    {
        #region Methods

        MachineErrorCode CanElevatorDeposit(BayNumber bayNumber);

        MachineErrorCode CanElevatorPickup(BayNumber bayNumber);

        ActionPolicy CanMove(ExternalBayMovementDirection direction, BayNumber bayNumber, MovementCategory movementCategory);

        double GetPosition(BayNumber bayNumber);

        void Homing(Calibration calibration, int? loadingUnitId, bool showErrors, BayNumber bayNumber, MessageActor sender);

        bool IsExternalPositionOccupied(BayNumber bayNumber);

        bool IsInternalPositionOccupied(BayNumber bayNumber);

        void Move(ExternalBayMovementDirection direction, int? loadUnitId, BayNumber bayNumber, MessageActor sender);

        void MoveAssisted(ExternalBayMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void MoveManual(ExternalBayMovementDirection direction, double distance, int? loadUnitId, bool bypassConditions, BayNumber bayNumber, MessageActor sender);

        void MovementForExtraction(int? loadUnitId, BayNumber bayNumber, MessageActor sender);

        void MovementForInsertion(BayNumber bayNumber, MessageActor sender);

        void StartTest(BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        void StopTest(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
