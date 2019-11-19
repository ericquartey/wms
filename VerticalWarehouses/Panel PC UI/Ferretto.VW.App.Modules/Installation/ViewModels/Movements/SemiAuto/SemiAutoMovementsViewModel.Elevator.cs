using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using IDialogService = Ferretto.VW.App.Controls.Interfaces.IDialogService;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private DelegateCommand disembarkBackwardsCommand;

        private DelegateCommand disembarkForwardsCommand;

        private double? elevatorHorizontalPosition;

        private double? elevatorVerticalPosition;

        private DelegateCommand embarkBackwardsCommand;

        private LoadingUnit embarkedLoadingUnit;

        private DelegateCommand embarkForwardsCommand;

        private bool isElevatorDisembarking;

        private bool isElevatorEmbarking;

        private bool isTuningBay;

        private bool isTuningChain;

        private DelegateCommand tuningBayCommand;

        private DelegateCommand tuningChainCommand;

        #endregion

        #region Properties

        public ICommand DisembarkBackwardsCommand =>
            this.disembarkBackwardsCommand
            ??
            (this.disembarkBackwardsCommand = new DelegateCommand(async () => await this.Disembark(HorizontalMovementDirection.Backwards), this.CanDisembark));

        public ICommand DisembarkForwardsCommand =>
            this.disembarkForwardsCommand
            ??
            (this.disembarkForwardsCommand = new DelegateCommand(async () => await this.Disembark(HorizontalMovementDirection.Forwards), this.CanDisembark));

        public double? ElevatorHorizontalPosition
        {
            get => this.elevatorHorizontalPosition;
            private set => this.SetProperty(ref this.elevatorHorizontalPosition, value);
        }

        public double? ElevatorVerticalPosition
        {
            get => this.elevatorVerticalPosition;
            private set => this.SetProperty(ref this.elevatorVerticalPosition, value);
        }

        public ICommand EmbarkBackwardsCommand =>
            this.embarkBackwardsCommand
            ??
            (this.embarkBackwardsCommand = new DelegateCommand(async () => await this.Embark(HorizontalMovementDirection.Backwards), this.CanEmbark));

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

        public ICommand EmbarkForwardsCommand =>
                    this.embarkForwardsCommand
            ??
            (this.embarkForwardsCommand = new DelegateCommand(async () => await this.Embark(HorizontalMovementDirection.Forwards), this.CanEmbark));

        public bool IsElevatorDisembarking
        {
            get => this.isElevatorDisembarking;
            private set
            {
                if (this.SetProperty(ref this.isElevatorDisembarking, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorEmbarking
        {
            get => this.isElevatorEmbarking;
            private set
            {
                if (this.SetProperty(ref this.isElevatorEmbarking, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
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

        #endregion

        #region Methods

        private bool CanDisembark()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                this.sensorsService.Sensors.LuPresentInMachineSide
                &&
                this.sensorsService.Sensors.LuPresentInOperatorSide;
        }

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

        private async Task Disembark(HorizontalMovementDirection direction)
        {
            this.IsElevatorDisembarking = true;
            await this.StartMovementAsync(direction, true);
        }

        private async Task Embark(HorizontalMovementDirection direction)
        {
            this.IsElevatorEmbarking = true;
            await this.StartMovementAsync(direction, false);
        }

        private async Task StartMovementAsync(HorizontalMovementDirection direction, bool isOnBoard)
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineElevatorWebService.MoveHorizontalAutoAsync(direction, isOnBoard, null, null);
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

        private async Task TuningChain()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
            var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Movimenti semi-automatici", DialogType.Question, DialogButtons.YesNo);
            if (messageBoxResult == DialogResult.Yes)
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

        #endregion
    }
}
