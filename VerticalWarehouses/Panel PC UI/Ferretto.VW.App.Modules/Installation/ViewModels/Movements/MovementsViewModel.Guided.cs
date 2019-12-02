using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class MovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private bool bayIsShutterThreeSensors;

        private DelegateCommand closedShutterCommand;

        private bool hasShutter;

        private int? inputCellId;

        private double? inputHeight;

        private int? inputLoadingUnitId;

        private DelegateCommand intermediateShutterCommand;

        private bool isElevatorMovingToCell;

        private bool isElevatorMovingToHeight;

        private bool isElevatorMovingToLoadingUnit;

        private bool isShutterMoving;

        private bool isUseWeightControl;

        private LoadingUnit loadingUnitInCell;

        private DelegateCommand openShutterCommand;

        private Cell selectedCell;

        private LoadingUnit selectedLoadingUnit;

        #endregion

        #region Properties

        public bool BayIsShutterThreeSensors
        {
            get => this.bayIsShutterThreeSensors;
            set => this.SetProperty(ref this.bayIsShutterThreeSensors, value);
        }

        public ICommand ClosedShutterCommand =>
            this.closedShutterCommand
            ??
            (this.closedShutterCommand = new DelegateCommand(
                async () => await this.ClosedShutterAsync(),
                this.CanCloseShutter));

        public bool HasShutter
        {
            get => this.hasShutter;
            set => this.SetProperty(ref this.hasShutter, value);
        }

        public int? InputCellId
        {
            get => this.inputCellId;
            set => this.SetProperty(ref this.inputCellId, value, this.InputCellIdPropertyChanged);
        }

        public double? InputHeight
        {
            get => this.inputHeight;
            set => this.SetProperty(ref this.inputHeight, value, this.RaiseCanExecuteChanged);
        }

        public int? InputLoadingUnitId
        {
            get => this.inputLoadingUnitId;
            set => this.SetProperty(ref this.inputLoadingUnitId, value, this.InputLoadingUnitIdPropertyChanged);
        }

        public ICommand IntermediateShutterCommand =>
                    this.intermediateShutterCommand
            ??
            (this.intermediateShutterCommand = new DelegateCommand(
                async () => await this.IntermediateShutterAsync(),
                this.CanExecuteIntermediateCommand));

        public bool IsElevatorMovingToCell
        {
            get => this.isElevatorMovingToCell;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToCell, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingToHeight
        {
            get => this.isElevatorMovingToHeight;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToHeight, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingToLoadingUnit
        {
            get => this.isElevatorMovingToLoadingUnit;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToLoadingUnit, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsShutterMoving
        {
            get => this.isShutterMoving;
            private set => this.SetProperty(ref this.isShutterMoving, value, this.RaiseCanExecuteChanged);
        }

        public bool IsUseWeightControl
        {
            get => this.isUseWeightControl;
            private set => this.SetProperty(ref this.isUseWeightControl, value);
        }

        public LoadingUnit LoadingUnitInCell
        {
            get => this.loadingUnitInCell;
            private set => this.SetProperty(ref this.loadingUnitInCell, value);
        }

        public ICommand OpenShutterCommand =>
            this.openShutterCommand
            ??
            (this.openShutterCommand = new DelegateCommand(
                async () => await this.OpenShutterAsync(),
                this.CanOpenShutter));

        public Cell SelectedCell
        {
            get => this.selectedCell;
            private set
            {
                if (this.SetProperty(ref this.selectedCell, value))
                {
                    if (this.selectedCell != null)
                    {
                        this.LoadingUnitInCell = this.loadingUnits.SingleOrDefault(l => l.CellId == this.selectedCell.Id);
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedLoadingUnit;
            private set
            {
                if (this.SetProperty(ref this.selectedLoadingUnit, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        private bool CanCloseShutter()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Open || this.sensorsService.ShutterSensors.MidWay));
        }

        private bool CanExecuteIntermediateCommand()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                this.BayIsShutterThreeSensors
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Open || this.sensorsService.ShutterSensors.Closed));
        }

        private bool CanOpenShutter()
        {
            return
                !this.IsKeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsShutterMoving
                &&
                (this.sensorsService.ShutterSensors != null && (this.sensorsService.ShutterSensors.Closed || this.sensorsService.ShutterSensors.MidWay));
        }

        private async Task ClosedShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Closed);
                this.IsShutterMoving = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void InputCellIdPropertyChanged()
        {
            if (this.cells is null)
            {
                return;
            }

            this.SelectedCell = this.inputCellId is null
                ? null
                : this.cells.SingleOrDefault(c => c.Id == this.inputCellId);

            if (this.SelectedCell != null)
            {
                this.InputHeight = this.SelectedCell.Position;
                this.InputLoadingUnitId = this.loadingUnits.SingleOrDefault(l => l.CellId == this.selectedCell.Id)?.Id;
            }

            if (this.SelectedLoadingUnit?.CellId is null)
            {
                this.LoadingUnitInCell = null;
            }
            else
            {
                this.LoadingUnitInCell = this.SelectedLoadingUnit;
            }

            this.RaiseCanExecuteChanged();
        }

        private void InputLoadingUnitIdPropertyChanged()
        {
            if (this.loadingUnits is null)
            {
                return;
            }

            this.SelectedLoadingUnit = this.inputLoadingUnitId == null
                ? null
                : this.loadingUnits.SingleOrDefault(c => c.Id == this.inputLoadingUnitId);

            if (this.SelectedLoadingUnit != null)
            {
                this.InputCellId = this.SelectedLoadingUnit.CellId;
            }
        }

        private async Task IntermediateShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Half);
                this.IsShutterMoving = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task OpenShutterAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.MoveToAsync(ShutterPosition.Opened);
                this.IsShutterMoving = true;
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
