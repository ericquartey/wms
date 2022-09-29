using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IExternalBayProvider
    {
        #region Methods

        MachineErrorCode CanElevatorDeposit(BayNumber bayNumber, bool isPositionUpper);

        MachineErrorCode CanElevatorDepositExternal(BayNumber bayNumber, bool isPositionUpper);

        MachineErrorCode CanElevatorPickup(Bay bay, bool isPositionUpper);

        ActionPolicy CanMove(ExternalBayMovementDirection direction, Bay bay, MovementCategory movementCategory);

        ActionPolicy CanMoveExtDouble(ExternalBayMovementDirection direction, Bay bay, MovementCategory movementCategory, bool isPositionUp);

        double GetPosition(BayNumber bayNumber);

        void Homing(Calibration calibration, int? loadingUnitId, bool showErrors, bool turnBack, BayNumber bayNumber, MessageActor sender);

        bool IsExternalPositionOccupied(Bay bay);

        bool IsExternalPositionOccupied(BayNumber bayNumber, LoadingUnitLocation loadingUnitLocation);

        bool IsInternalPositionOccupied(Bay bay);

        bool IsInternalPositionOccupied(BayNumber bayNumber, LoadingUnitLocation loadingUnitLocation);

        void Move(ExternalBayMovementDirection direction, int? loadUnitId, Bay bay, MessageActor sender);

        void MoveAssisted(ExternalBayMovementDirection direction, BayNumber bayNumber, MessageActor sender);

        void MoveAssistedExtDouble(ExternalBayMovementDirection direction, BayNumber bayNumber, MessageActor sender, bool isUpper);

        void MoveExtDouble(ExternalBayMovementDirection direction, int? loadUnitId, BayNumber bayNumber, MessageActor sender, bool isUpper);

        void MoveManual(ExternalBayMovementDirection direction, double distance, int? loadUnitId, bool bypassConditions, Bay bay, MessageActor sender);

        void MoveManualExtDouble(ExternalBayMovementDirection direction, double distance, int? loadUnitId, bool bypassConditions, BayNumber bayNumber, MessageActor sender);

        void MovementForExtraction(int? loadUnitId, BayNumber bayNumber, MessageActor sender, bool isUpper);

        void MovementForInsertion(BayNumber bayNumber, MessageActor sender, bool isUpper);

        void StartDoubleExtBayTest(ExternalBayMovementDirection direction, BayNumber bayNumber, MessageActor sender, bool isUpper);

        void StartTest(BayNumber bayNumber, MessageActor sender);

        void Stop(BayNumber bayNumber, MessageActor sender);

        void StopTest(BayNumber bayNumber, MessageActor sender);

        #endregion
    }
}
