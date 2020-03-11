﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface IElevatorProvider
    {
        #region Properties

        double HorizontalPosition { get; set; }

        double VerticalPosition { get; set; }

        #endregion

        #region Methods

        ActionPolicy CanExtractFromBay(int bayPositionId, BayNumber bayNumber);

        ActionPolicy CanLoadFromBay(int bayPositionId, BayNumber bayNumber, bool isGuided, bool enforceLoadUnitPresent = true);

        ActionPolicy CanLoadFromCell(int cellId, BayNumber bayNumber);

        ActionPolicy CanMoveToBayPosition(int bayPositionId, BayNumber bayNumber);

        ActionPolicy CanMoveToCell(int cellId);

        ActionPolicy CanUnloadToBay(int bayPositionId, BayNumber bayNumber, bool isGuided);

        ActionPolicy CanUnloadToCell(int cellId);

        void ContinuePositioning(BayNumber requestingBay, MessageActor sender);

        AxisBounds GetVerticalBounds();

        void Homing(Axis calibrateAxis, Calibration calibration, int? loadUnitId, bool showErrors, BayNumber bayNumber, MessageActor sender);

        bool IsZeroChainSensor();

        void LoadFromBay(int bayPositionId, BayNumber bayNumber, MessageActor automationService);

        void LoadFromCell(int cellId, BayNumber bayNumber, MessageActor automationService);

        void MoveHorizontalAuto(
            HorizontalMovementDirection direction,
            bool elevatorHasLoadingUnit,
            int? loadingUnitId,
            double? loadingUnitNetWeight,
            bool waitContinue,
            bool measureProfile,
            BayNumber requestingBay,
            MessageActor sender,
            int? targetCellId = null,
            int? targetBayPositionId = null,
            int? sourceCellId = null,
            int? sourceBayPositionId = null);

        void MoveHorizontalManual(HorizontalMovementDirection direction, double distance, bool measure, int? loadingUnitId, int? positionId, bool bypassConditions, BayNumber requestingBay, MessageActor sender);

        void MoveHorizontalProfileCalibration(int bayPositionId, BayNumber requestingBay, MessageActor sender);

        void MoveToAbsoluteVerticalPosition(
            bool manualMovment,
            double targetPosition,
            bool computeElongation,
            bool performWeighting,
            int? targetBayPositionId,
            int? targetCellId,
            bool checkHomingDone,
            bool waitContinue,
            bool isPickupMission,
            BayNumber requestingBay,
            MessageActor sender);

        void MoveToBayPosition(int bayPositionId, bool computeElongation, bool performWeighting, BayNumber bayNumber, MessageActor sender);

        void MoveToCell(int cellId, bool computeElongation, bool performWeighting, BayNumber requestingBay, MessageActor sender);

        void MoveToFreeCell(int loadUnitId, bool computeElongation, bool performWeighting, BayNumber requestingBay, MessageActor sender);

        void MoveToRelativeVerticalPosition(double distance, BayNumber requestingBay, MessageActor sender);

        void MoveVerticalManual(VerticalMovementDirection direction, BayNumber requestingBay, MessageActor sender);

        void ResetBeltBurnishing();

        void RunTorqueCurrentSampling(double displacement, double netWeight, int? loadingUnitId, BayNumber requestingBay, MessageActor sender);

        MovementProfileType SelectProfileType(HorizontalMovementDirection direction, bool elevatorHasLoadingUnit);

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
