using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ILoadingUnitMovementProvider
    {
        #region Methods

        double AdjustHeightWithBayChainWeight(double height, double weightUp, double weightDown);

        MessageStatus CarouselStatus(NotificationMessage message);

        MachineErrorCode CheckBaySensors(Bay bay, LoadingUnitLocation loadingUnitPosition, bool deposit);

        void CloseShutter(MessageActor sender, BayNumber requestingBay, bool restore, ShutterPosition shutterPosition = ShutterPosition.Closed);

        void ContinuePositioning(MessageActor sender, BayNumber requestingBay);

        MessageStatus ExternalBayStatus(NotificationMessage message);

        bool FilterNotifications(NotificationMessage notification, MessageActor destination);

        BayNumber GetBayByCell(int cellId);

        double GetCurrentHorizontalPosition();

        double GetCurrentVerticalPosition();

        int GetCyclesFromCalibration();

        double? GetDestinationHeight(Mission moveData, out int? targetBayPositionId, out int? targetCellId);

        double GetLastVerticalPosition();

        ShutterPosition GetShutterClosedPosition(Bay bay, LoadingUnitLocation location);

        ShutterPosition GetShutterOpenPosition(Bay bay, LoadingUnitLocation location);

        double? GetSourceHeight(Mission moveData, out int? targetBayPositionId, out int? targetCellId);

        void Homing(Axis axis, Calibration calibration, int loadingUnitId, bool showErrors, BayNumber requestingBay, MessageActor sender);

        bool IsExternalPositionOccupied(BayNumber bayNumber);

        bool IsInternalPositionOccupied(BayNumber bayNumber);

        bool IsOnlyBottomPositionOccupied(BayNumber bayNumber);

        bool IsOnlyTopPositionOccupied(BayNumber bayNumber);

        void MoveCarousel(int? loadUnitId, MessageActor sender, BayNumber requestingBay, bool restore);

        bool MoveExternalBay(int? loadUnitId, ExternalBayMovementDirection direction, MessageActor sender, BayNumber requestingBay, bool restore);

        void MoveLoadingUnit(HorizontalMovementDirection direction, bool moveToCradle, ShutterPosition moveShutter, bool measure, MessageActor sender, BayNumber requestingBay, int? loadUnitId, int? positionId, bool fastDeposit = true);

        MessageStatus MoveLoadingUnitStatus(NotificationMessage message);

        bool MoveManualLoadingUnitBackward(HorizontalMovementDirection direction, int? loadUnitId, MessageActor sender, BayNumber requestingBay);

        bool MoveManualLoadingUnitForward(HorizontalMovementDirection direction, bool isLoadingUnitOnBoard, bool measure, int? loadUnitId, int? positionId, MessageActor sender, BayNumber requestingBay);

        void NotifyAssignedMissionOperationChanged(BayNumber bayNumber, int missionId);

        void OpenShutter(MessageActor sender, ShutterPosition openShutter, BayNumber requestingBay, bool restore);

        void PositionElevatorToPosition(double targetHeight,
            BayNumber shutterBay,
            ShutterPosition shutterPosition,
            bool measure,
            MessageActor sender,
            BayNumber requestingBay,
            bool restore,
            int? targetBayPositionId,
            int? targetCellId,
            bool waitContinue = false);

        MessageStatus PositionElevatorToPositionStatus(NotificationMessage message);

        void ResumeOperation(int missionId, LoadingUnitLocation loadUnitSource, LoadingUnitLocation loadUnitDestination, int? wmsId, MissionType missionType, BayNumber targetBay, MessageActor sender);

        MessageStatus ShutterStatus(NotificationMessage message);

        void StopOperation(IStopMessageData messageData, BayNumber targetBay, MessageActor sender, BayNumber requestingBay);

        MessageStatus StopOperationStatus(NotificationMessage message);

        void UpdateLastBayChainPosition(BayNumber requestingBay);

        void UpdateLastIdealPosition(HorizontalMovementDirection direction, bool isLoadingUnitOnBoard);

        #endregion
    }
}
