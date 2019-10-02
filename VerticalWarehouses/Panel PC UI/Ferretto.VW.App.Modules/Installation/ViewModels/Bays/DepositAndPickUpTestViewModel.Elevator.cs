using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class DepositAndPickUpTestViewModel
    {

        #region Fields

        private readonly IMachineElevatorService machineElevatorService;

        private double? elevatorHorizontalPosition;

        private double? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private bool isElevatorDisembarking;

        private bool isElevatorEmbarking;

        private bool isTuningChain;

        private DelegateCommand tuningBayCommand;

        private DelegateCommand tuningChainCommand;

        private DepositAndPickUpState currentState;

        #endregion

        #region Properties

        public double? ElevatorHorizontalPosition
        {
            get => this.elevatorHorizontalPosition;
            protected set => this.SetProperty(ref this.elevatorHorizontalPosition, value);
        }

        public double? ElevatorVerticalPosition
        {
            get => this.elevatorVerticalPosition;
            protected set => this.SetProperty(ref this.elevatorVerticalPosition, value);
        }

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

            protected set => this.SetProperty(ref this.embarkedLoadingUnit, value);
        }

        public bool IsElevatorDisembarking
        {
            get => this.isElevatorDisembarking;
            private set
            {
                if (this.SetProperty(ref this.isElevatorDisembarking, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsElevatorMoving));
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
                    this.RaisePropertyChanged(nameof(this.IsElevatorMoving));
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
                    this.RaisePropertyChanged(nameof(this.IsElevatorMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsZeroChain
        {
            get => this.isZeroChain;
            set => this.SetProperty(ref this.isZeroChain, value);
        }

        public ICommand TuningBayCommand =>
                    this.tuningBayCommand
            ??
            (this.tuningBayCommand = new DelegateCommand(
                async () => await this.TuningBay(),
                this.CanTuningBay));

        #endregion

        #region Methods

        private bool CanEmbark()
        {
            return
                !this.IsElevatorMoving
                &&
                !this.Sensors.LuPresentInMachineSideBay1
                &&
                !this.Sensors.LuPresentInOperatorSideBay1
                &&
                this.IsZeroChain;
        }

        private bool CanTuningBay()
        {
            return true;
        }

        private bool CanTuningChain()
        {
            return !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving
                &&
                !this.IsTuningChain
                &&
                !this.Sensors.LuPresentInMachineSideBay1
                &&
                !this.Sensors.LuPresentInOperatorSideBay1
                &&
                !this.IsZeroChain;
        }

        private async Task RetrieveElevatorPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.ElevatorVerticalPosition = await this.machineElevatorService.GetVerticalPositionAsync();
                this.ElevatorHorizontalPosition = await this.machineElevatorService.GetHorizontalPositionAsync();
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

        private async Task StartMovementAsync()
        {
            try
            {
                if ((this.currentState == DepositAndPickUpState.GotoBay ||
                     this.currentState == DepositAndPickUpState.GotoBayAdjusted) == false)
                {
                    this.ShowNotification($"Stato dovrebbe essere in modalità {DepositAndPickUpState.GotoBay} o {DepositAndPickUpState.GotoBayAdjusted}");
                    return;
                }

                await this.machineElevatorService.MoveHorizontalAutoAsync(this.GetDirection(), this.embarkedLoadingUnit != null);

                if (this.currentState == DepositAndPickUpState.GotoBay)
                {
                    this.currentState = DepositAndPickUpState.Deposit;
                }
                else
                {
                    this.currentState = DepositAndPickUpState.EndLoaded;
                }
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private HorizontalMovementDirection GetDirection()
        {
            if (this.currentState == DepositAndPickUpState.Deposit)
            {
                return (this.bayManagerService.Bay.Side == WarehouseSide.Front) ? HorizontalMovementDirection.Backwards : HorizontalMovementDirection.Forwards;
            }
            else
            {
                return (this.bayManagerService.Bay.Side == WarehouseSide.Front) ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards;
            }
        }

        private async Task TuningBay()
        {
            await Task.Delay(1);
        }

        public async Task CheckStart()
        {
            await Task.Delay(this.inputDelay * 1000);
        }

        #endregion
    }
}
