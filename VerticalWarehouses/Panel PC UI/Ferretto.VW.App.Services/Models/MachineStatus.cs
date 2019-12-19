using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Mvvm;

namespace Ferretto.VW.App.Services.Models
{
    public class MachineStatus : BindableBase, ICloneable
    {
        #region Fields

        private double? bayChainPosition;

        private Guid? currentMissionId;

        private double? elevatorHorizontalPosition;

        private string elevatorLogicalPosition;

        private LoadingUnit elevatorPositionLoadingUnit;

        private double? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private string errorDescription;

        private bool isError;

        private bool isMoving;

        private bool isMovingElevator;

        private bool isMovingLoadingUnit;

        private bool isMovingShutter;

        private string loadingUnitPositionDownInBayCode;

        private string loadingUnitPositionUpInBayCode;

        private string logicalPosition;

        private string logicalPositionId;

        #endregion

        #region Properties

        public double? BayChainPosition
        {
            get => this.bayChainPosition;
            set => this.SetProperty(ref this.bayChainPosition, value);
        }

        public Guid? CurrentMissionId
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

        public string ErrorDescription
        {
            get => this.errorDescription;
            set => this.SetProperty(ref this.errorDescription, value);
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
            set => this.SetProperty(ref this.isMovingLoadingUnit, value);
        }

        public bool IsMovingShutter
        {
            get => this.isMovingShutter;
            set => this.SetProperty(ref this.isMovingShutter, value);
        }

        public string LoadingUnitPositionDownInBayCode
        {
            get => this.loadingUnitPositionDownInBayCode;
            set => this.SetProperty(ref this.loadingUnitPositionDownInBayCode, value);
        }

        public string LoadingUnitPositionUpInBayCode
        {
            get => this.loadingUnitPositionUpInBayCode;
            set => this.SetProperty(ref this.loadingUnitPositionUpInBayCode, value);
        }

        public string LogicalPosition
        {
            get => this.logicalPosition;
            set => this.SetProperty(ref this.logicalPosition, value);
        }

        public string LogicalPositionId
        {
            get => this.logicalPositionId;
            set => this.SetProperty(ref this.logicalPositionId, value);
        }

        #endregion

        #region Methods

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
