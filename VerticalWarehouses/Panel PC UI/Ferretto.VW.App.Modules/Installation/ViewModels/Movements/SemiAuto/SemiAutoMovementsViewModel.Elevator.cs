using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;
using IDialogService = Ferretto.VW.App.Controls.Interfaces.IDialogService;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private LoadingUnit embarkedLoadingUnit;

        private bool isBusyLoadingFromBay;

        private bool isBusyLoadingFromCell;

        private bool isBusyUnloadingToBay;

        private bool isBusyUnloadingToCell;

        private bool isElevatorInBay;

        private bool isElevatorInCell;

        private bool isTuningBay;

        private bool isTuningChain;

        private DelegateCommand loadFromBayCommand;

        private ActionPolicy loadFromBayPolicy;

        private DelegateCommand loadFromCellCommand;

        private ActionPolicy loadFromCellPolicy;

        private ActionPolicy moveToBayPositionPolicy;

        private ActionPolicy moveToCellPolicy;

        private VerticalManualMovementsProcedure procedureParameters;

        private DelegateCommand tuningBayCommand;

        private DelegateCommand tuningChainCommand;

        private DelegateCommand unloadToBayCommand;

        private ActionPolicy unloadToBayPolicy;

        private DelegateCommand unloadToCellCommand;

        private ActionPolicy unloadToCellPolicy;

        #endregion

        #region Properties

        public LoadingUnit EmbarkedLoadingUnit
        {
            // TODO  for the moment we use only presence sensors
            // get => this.embarkedLoadingUnit;
            get
            {
                if (this.CanEmbark())
                {
                    this.embarkedLoadingUnit = new LoadingUnit();
                }
                else
                {
                    this.embarkedLoadingUnit = null;
                }

                return this.embarkedLoadingUnit;
            }

            private set => this.SetProperty(ref this.embarkedLoadingUnit, value);
        }

        public bool IsBusyLoadingFromBay
        {
            get => this.isBusyLoadingFromBay;
            private set
            {
                if (this.SetProperty(ref this.isBusyLoadingFromBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusyLoadingFromCell
        {
            get => this.isBusyLoadingFromCell;
            private set
            {
                if (this.SetProperty(ref this.isBusyLoadingFromCell, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusyUnloadingToBay
        {
            get => this.isBusyUnloadingToBay;
            private set
            {
                if (this.SetProperty(ref this.isBusyUnloadingToBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusyUnloadingToCell
        {
            get => this.isBusyUnloadingToCell;
            private set
            {
                if (this.SetProperty(ref this.isBusyUnloadingToCell, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorInBay
        {
            get => this.isElevatorInBay;
            private set => this.SetProperty(ref this.isElevatorInBay, value);
        }

        public bool IsElevatorInCell
        {
            get => this.isElevatorInCell;
            private set => this.SetProperty(ref this.isElevatorInCell, value);
        }

        public bool IsTuningBay
        {
            get => this.isTuningBay;
            private set
            {
                if (this.SetProperty(ref this.isTuningBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsTuningChain
        {
            get => this.isTuningChain;
            private set
            {
                if (this.SetProperty(ref this.isTuningChain, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand LoadFromBayCommand =>
            this.loadFromBayCommand
            ??
            (this.loadFromBayCommand = new DelegateCommand(
                async () => await this.LoadFromBayAsync(),
                this.CanLoadFromBay));

        public ICommand LoadFromCellCommand =>
            this.loadFromCellCommand
            ??
            (this.loadFromCellCommand = new DelegateCommand(
                async () => await this.LoadFromCellAsync(),
                this.CanLoadFromCell));

        public ICommand TuningBayCommand =>
            this.tuningBayCommand
            ??
            (this.tuningBayCommand = new DelegateCommand(
                async () => await this.TuneBayAsync(),
                this.CanTuneBay));

        public ICommand TuningChainCommand =>
            this.tuningChainCommand
            ??
            (this.tuningChainCommand = new DelegateCommand(
                async () => await this.TuningChain(),
                this.CanTuningChain));

        public ICommand UnloadToBayCommand =>
            this.unloadToBayCommand
            ??
            (this.unloadToBayCommand = new DelegateCommand(
                async () => await this.UnloadToBayAsync(),
                this.CanUnloadToBay));

        public ICommand UnloadToCellCommand =>
            this.unloadToCellCommand
            ??
            (this.unloadToCellCommand = new DelegateCommand(
                async () => await this.UnloadToCellAsync(),
                this.CanUnloadToCell));

        #endregion

        #region Methods

        private bool CanEmbark()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.sensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.sensorsService.Sensors.LuPresentInOperatorSide
                &&
                this.sensorsService.IsZeroChain;
        }

        private bool CanLoadFromBay()
        {
            return
                !this.KeyboardOpened
                &&
                this.SelectedBayPosition != null
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse
                &&
                this.loadFromBayPolicy?.IsAllowed == true;
        }

        private bool CanLoadFromCell()
        {
            return
                !this.KeyboardOpened
                &&
                this.SelectedCell != null
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse
                &&
                this.loadFromCellPolicy?.IsAllowed == true;
        }

        private bool CanTuneBay()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsTuningBay
                &&
                this.sensorsService.Sensors.ACUBay1S3IND;
        }

        private bool CanTuningChain()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                !this.IsTuningChain
                &&
                this.sensorsService.IsZeroChain
                &&
                !this.sensorsService.Sensors.LuPresentInMachineSide
                &&
                !this.sensorsService.Sensors.LuPresentInOperatorSide;
        }

        private bool CanUnloadToBay()
        {
            return
                !this.KeyboardOpened
                &&
                this.SelectedBayPosition != null
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse
                &&
                this.unloadToBayPolicy?.IsAllowed == true;
        }

        private bool CanUnloadToCell()
        {
            return
                !this.KeyboardOpened
                &&
                this.SelectedCell != null
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse
                &&
                this.unloadToCellPolicy?.IsAllowed == true;
        }

        private async Task LoadFromBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                System.Diagnostics.Debug.Assert(
                    this.SelectedBayPosition != null,
                    "A bay position should be selected");

                await this.machineElevatorWebService.LoadFromBayAsync(this.SelectedBayPosition.Id);

                this.IsBusyLoadingFromBay = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task LoadFromCellAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                System.Diagnostics.Debug.Assert(
                    this.SelectedCell != null,
                    "A cell should be selected");

                await this.machineElevatorWebService.LoadFromCellAsync(this.SelectedCell.Id);

                this.IsBusyLoadingFromCell = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnElevatorPositionChanged(ElevatorPositionChangedEventArgs e)
        {
            this.IsElevatorInBay = e.BayPositionId != null;
            this.IsElevatorInCell = e.CellId != null;
        }

        private async Task RefreshActionPoliciesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("*************");
                this.Log = $"*************{Environment.NewLine}{this.Log}";

                var selectedBayPosition = this.SelectedBayPosition;
                if (selectedBayPosition != null)
                {
                    this.loadFromBayPolicy = await this.machineElevatorWebService.CanLoadFromBayAsync(selectedBayPosition.Id);
                    this.loadFromBayCommand?.RaiseCanExecuteChanged();
                    if (!string.IsNullOrEmpty(this.loadFromBayPolicy.Reason))
                    {
                        this.Log = $"{DateTime.Now.ToLocalTime()} - LoadFromBay - {this.loadFromBayPolicy.Reason}{Environment.NewLine}{this.Log}";
                    }

                    System.Diagnostics.Debug.WriteLine($"ELEV <- BAY: {this.loadFromBayPolicy.IsAllowed} {this.loadFromBayPolicy.Reason}");

                    this.unloadToBayPolicy = await this.machineElevatorWebService.CanUnloadToBayAsync(selectedBayPosition.Id);
                    this.unloadToBayCommand?.RaiseCanExecuteChanged();
                    if (!string.IsNullOrEmpty(this.unloadToBayPolicy.Reason))
                    {
                        this.Log = $"{DateTime.Now.ToLocalTime()} - UnloadToBay - {this.unloadToBayPolicy.Reason}{Environment.NewLine}{this.Log}";
                    }

                    System.Diagnostics.Debug.WriteLine($"ELEV -> BAY: {this.unloadToBayPolicy.IsAllowed} {this.unloadToBayPolicy.Reason}");

                    this.moveToBayPositionPolicy = await this.machineElevatorWebService.CanMoveToBayPositionAsync(selectedBayPosition.Id);
                    this.moveToBayPositionCommand?.RaiseCanExecuteChanged();
                    if (!string.IsNullOrEmpty(this.moveToBayPositionPolicy.Reason))
                    {
                        this.Log = $"{DateTime.Now.ToLocalTime()} - MoveToBayPosition - {this.moveToBayPositionPolicy.Reason}{Environment.NewLine}{this.Log}";
                    }

                    System.Diagnostics.Debug.WriteLine($"ELEV ^ BAY: {this.moveToBayPositionPolicy.IsAllowed} {this.moveToBayPositionPolicy.Reason}");
                }

                var selectedCell = this.SelectedCell;
                if (selectedCell != null)
                {
                    this.loadFromCellPolicy = await this.machineElevatorWebService.CanLoadFromCellAsync(selectedCell.Id);
                    this.loadFromCellCommand?.RaiseCanExecuteChanged();
                    if (!string.IsNullOrEmpty(this.loadFromCellPolicy.Reason))
                    {
                        this.Log = $"{DateTime.Now.ToLocalTime()} - LoadFromCell - {this.loadFromCellPolicy.Reason}{Environment.NewLine}{this.Log}";
                    }

                    System.Diagnostics.Debug.WriteLine($"ELEV <- CELL: {this.loadFromCellPolicy.IsAllowed} {this.loadFromCellPolicy.Reason}");

                    this.unloadToCellPolicy = await this.machineElevatorWebService.CanUnloadToCellAsync(selectedCell.Id);
                    this.unloadToCellCommand?.RaiseCanExecuteChanged();
                    if (!string.IsNullOrEmpty(this.unloadToCellPolicy.Reason))
                    {
                        this.Log = $"{DateTime.Now.ToLocalTime()} - UnloadToCell - {this.unloadToCellPolicy.Reason}{Environment.NewLine}{this.Log}";
                    }

                    System.Diagnostics.Debug.WriteLine($"ELEV -> CELL: {this.unloadToCellPolicy.IsAllowed} {this.unloadToCellPolicy.Reason}");

                    this.moveToCellPolicy = await this.machineElevatorWebService.CanMoveToCellAsync(selectedCell.Id);
                    this.moveToCellHeightCommand?.RaiseCanExecuteChanged();
                    if (!string.IsNullOrEmpty(this.moveToCellPolicy.Reason))
                    {
                        this.Log = $"{DateTime.Now.ToLocalTime()} - MoveToCellHeight - {this.moveToCellPolicy.Reason}{Environment.NewLine}{this.Log}";
                    }

                    System.Diagnostics.Debug.WriteLine($"ELEV ^ CELL: {this.moveToCellPolicy.IsAllowed} {this.moveToCellPolicy.Reason}");
                }

                if (this.HasCarousel)
                {
                    this.moveCarouselUpPolicy = await this.machineCarouselWebService.CanMoveAsync(VerticalMovementDirection.Up);
                    this.moveCarouselUpCommand?.RaiseCanExecuteChanged();
                    if (!string.IsNullOrEmpty(this.moveCarouselUpPolicy.Reason))
                    {
                        this.Log = $"{DateTime.Now.ToLocalTime()} - MoveCarouselUp - {this.moveCarouselUpPolicy.Reason}{Environment.NewLine}{this.Log}";
                    }

                    this.moveCarouselDownPolicy = await this.machineCarouselWebService.CanMoveAsync(VerticalMovementDirection.Down);
                    this.moveCarouselDownCommand?.RaiseCanExecuteChanged();
                    if (!string.IsNullOrEmpty(this.moveCarouselDownPolicy.Reason))
                    {
                        this.Log = $"{DateTime.Now.ToLocalTime()} - MoveCarouselDown - {this.moveCarouselDownPolicy.Reason}{Environment.NewLine}{this.Log}";
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task TuningChain()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Movimenti semi-automatici", DialogType.Question, DialogButtons.YesNo);
            if (messageBoxResult is DialogResult.Yes)
            {
                try
                {
                    this.IsWaitingForResponse = true;
                    await this.machineElevatorWebService.SearchHorizontalZeroAsync();
                    this.IsTuningChain = true;
                }
                catch (Exception ex)
                {
                    this.IsTuningChain = false;

                    this.ShowNotification(ex);
                }
                finally
                {
                    this.IsWaitingForResponse = false;
                }
            }
        }

        private async Task UnloadToBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                System.Diagnostics.Debug.Assert(
                    this.SelectedBayPosition != null,
                    "A bay position should be selected");

                await this.machineElevatorWebService.UnloadToBayAsync(this.SelectedBayPosition.Id);

                this.IsBusyUnloadingToBay = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task UnloadToCellAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                System.Diagnostics.Debug.Assert(
                    this.SelectedCell != null,
                    "A cell should be selected");

                await this.machineElevatorWebService.UnloadToCellAsync(this.SelectedCell.Id);

                this.IsBusyUnloadingToCell = true;
            }
            catch (Exception ex)
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
