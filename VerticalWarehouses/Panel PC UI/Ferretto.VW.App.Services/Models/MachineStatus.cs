﻿using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Mvvm;

namespace Ferretto.VW.App.Services.Models
{
    public class MachineStatus : BindableBase
    {
        #region Fields

        private double? bayChainPosition;

        private double? bayChainTargetPosition;

        private int? bayPositionId;

        private bool? bayPositionUpper;

        private MoveLoadingUnitMessageData currentMission;

        private string currentMissionDescription;

        private int? currentMissionId;

        private double? elevatorHorizontalPosition;

        private string elevatorLogicalPosition;

        private LoadingUnit elevatorPositionLoadingUnit;

        private ElevatorPositionType elevatorPositionType;

        private double? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private int? embarkedLoadingUnitId;

        private string errorDescription;

        private bool flagForNotification;

        private double? horizontalTargetPosition;

        private bool isDepositAndPickUpRunning;

        private bool isError;

        private bool isMoving;

        private bool isMovingElevator;

        private bool isMovingLoadingUnit;

        private bool isMovingShutter;

        private bool isStopped;

        private LoadingUnit loadingUnitPositionDownInBay;

        private LoadingUnit loadingUnitPositionUpInBay;

        private string logicalPosition;

        private int? logicalPositionId;

        private MessageStatus messageStatus;

        private double? verticalSpeed;

        private double? verticalTargetPosition;

        #endregion

        #region Properties

        public double? BayChainPosition
        {
            get => this.bayChainPosition;
            set => this.SetProperty(ref this.bayChainPosition, value);
        }

        public double? BayChainTargetPosition
        {
            get => this.bayChainTargetPosition;
            set => this.SetProperty(ref this.bayChainTargetPosition, value);
        }

        public int? BayPositionId
        {
            get => this.bayPositionId;
            set => this.SetProperty(ref this.bayPositionId, value);
        }

        public bool? BayPositionUpper
        {
            get => this.bayPositionUpper;
            set => this.SetProperty(ref this.bayPositionUpper, value);
        }

        public MoveLoadingUnitMessageData CurrentMission
        {
            get => this.currentMission;
            set => this.SetProperty(ref this.currentMission, value);
        }

        public string CurrentMissionDescription
        {
            get => this.currentMissionDescription;
            set => this.SetProperty(ref this.currentMissionDescription, value);
        }

        public int? CurrentMissionId
        {
            get => this.currentMissionId;
            set => this.SetProperty(ref this.currentMissionId, value);
        }

        public double? ElevatorHorizontalPosition
        {
            get => this.elevatorHorizontalPosition;
            set => this.SetProperty(ref this.elevatorHorizontalPosition, value);
        }

        public string ElevatorLogicalPosition
        {
            get => this.elevatorLogicalPosition;
            set => this.SetProperty(ref this.elevatorLogicalPosition, value);
        }

        public LoadingUnit ElevatorPositionLoadingUnit
        {
            get => this.elevatorPositionLoadingUnit;
            set => this.SetProperty(ref this.elevatorPositionLoadingUnit, value);
        }

        public ElevatorPositionType ElevatorPositionType
        {
            get => this.elevatorPositionType;
            set => this.SetProperty(ref this.elevatorPositionType, value);
        }

        public double? ElevatorVerticalPosition
        {
            get => this.elevatorVerticalPosition;
            set => this.SetProperty(ref this.elevatorVerticalPosition, value);
        }

        public LoadingUnit EmbarkedLoadingUnit
        {
            get => this.embarkedLoadingUnit;
            set => this.SetProperty(ref this.embarkedLoadingUnit, value);
        }

        public int? EmbarkedLoadingUnitId
        {
            get => this.embarkedLoadingUnitId;
            set => this.SetProperty(ref this.embarkedLoadingUnitId, value);
        }

        public string ErrorDescription
        {
            get => this.errorDescription;
            set => this.SetProperty(ref this.errorDescription, value);
        }

        public bool FlagForNotification
        {
            get => this.flagForNotification;
            set => this.SetProperty(ref this.flagForNotification, value);
        }

        public double? HorizontalTargetPosition
        {
            get => this.horizontalTargetPosition;
            set => this.SetProperty(ref this.horizontalTargetPosition, value);
        }

        public bool IsDepositAndPickUpRunning
        {
            get => this.isDepositAndPickUpRunning;
            set => this.SetProperty(ref this.isDepositAndPickUpRunning, value);
        }

        public bool IsError
        {
            get => this.isError;
            set => this.SetProperty(ref this.isError, value);
        }

        public bool IsMoving
        {
            get => this.isMoving;
            set => this.SetProperty(ref this.isMoving, value);
        }

        public bool IsMovingElevator
        {
            get => this.isMovingElevator;
            set => this.SetProperty(ref this.isMovingElevator, value);
        }

        public bool IsMovingLoadingUnit
        {
            get => this.isMovingLoadingUnit;
            set
            {
                if (this.SetProperty(ref this.isMovingLoadingUnit, value))
                {
                    System.Diagnostics.Debug.WriteLine("****** isMovingLoadingUnit " + value);
                }
            }
        }

        public bool IsMovingShutter
        {
            get => this.isMovingShutter;
            set => this.SetProperty(ref this.isMovingShutter, value);
        }

        public bool IsStopped
        {
            get => this.isStopped;
            set => this.SetProperty(ref this.isStopped, value);
        }

        public LoadingUnit LoadingUnitPositionDownInBay
        {
            get => this.loadingUnitPositionDownInBay;
            set => this.SetProperty(ref this.loadingUnitPositionDownInBay, value);
        }

        public LoadingUnit LoadingUnitPositionUpInBay
        {
            get => this.loadingUnitPositionUpInBay;
            set => this.SetProperty(ref this.loadingUnitPositionUpInBay, value);
        }

        public string LogicalPosition
        {
            get => this.logicalPosition;
            set => this.SetProperty(ref this.logicalPosition, value);
        }

        public int? LogicalPositionId
        {
            get => this.logicalPositionId;
            set => this.SetProperty(ref this.logicalPositionId, value);
        }

        public MessageStatus MessageStatus
        {
            get => this.messageStatus;
            set => this.SetProperty(ref this.messageStatus, value);
        }

        public double? VerticalSpeed
        {
            get => this.verticalSpeed;
            set => this.SetProperty(ref this.verticalSpeed, value);
        }

        public double? VerticalTargetPosition
        {
            get => this.verticalTargetPosition;
            set => this.SetProperty(ref this.verticalTargetPosition, value);
        }

        #endregion
    }
}
