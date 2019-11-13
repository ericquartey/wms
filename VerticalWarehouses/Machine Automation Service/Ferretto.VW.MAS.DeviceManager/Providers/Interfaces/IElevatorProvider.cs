using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Properties

        double HorizontalPosition { get; set; }

        double VerticalPosition { get; set; }

        #endregion

        #region Methods

        ActionPolicy CanLoadFromBay(int bayPositionId, BayNumber bayNumber);

        ActionPolicy CanLoadFromCell(int cellId, BayNumber bayNumber);

        ActionPolicy CanUnloadToBay(int bayPositionId, BayNumber bayNumber);

        ActionPolicy CanUnloadToCell(int cellId, BayNumber bayNumber);

        void ContinuePositioning(BayNumber requestingBay, MessageActor sender);

        AxisBounds GetVerticalBounds();

        void LoadFromBay(int bayPositionId, BayNumber bayNumber, MessageActor automationService);

        void LoadFromCell(int cellId, BayNumber bayNumber, MessageActor automationService);

        void MoveHorizontalAuto(
                                                                            HorizontalMovementDirection direction,
            bool elevatorHasLoadingUnit,
            int? loadingUnitId,
            double? loadingUnitNetWeight,
            bool waitContinue,
            bool measure,
            BayNumber requestingBay,
            MessageActor sender);

        void MoveHorizontalManual(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveHorizontalProfileCalibration(HorizontalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void MoveToAbsoluteVerticalPosition(double targetPosition, double feedRate, bool measure, bool computeElongation, BayNumber requestingBay, MessageActor sender);

        void MoveToBayPosition(int bayPositionId, double feedRate, bool computeElongation, BayNumber bayNumber, MessageActor automationService);

        void MoveToCell(int cellId, double feedRate, bool computeElongation, BayNumber bayNumber, MessageActor automationService);

        void MoveToRelativeVerticalPosition(double distance, BayNumber requestingBay, MessageActor sender, double feedRate = 1);

        void MoveVertical(VerticalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void RunTorqueCurrentSampling(double displacement, double netWeight, int? loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void StartBeltBurnishing(
            double upperBoundPosition,
            double lowerBoundPosition,
            int delayStart, BayNumber
            requestingBay,
            MessageActor sender);

        void Stop(BayNumber requestingBay, MessageActor sender);

        void UnloadToBay(int bayPosition, BayNumber bayNumber, MessageActor automationService);

        void UnloadToCell(int cellId, BayNumber bayNumber, MessageActor automationService);

        #endregion
    }
}
