using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class DepositAndPickUpTestViewModel
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

        private DepositAndPickUpState currentState;

        private double? elevatorHorizontalPosition;

        private double? elevatorVerticalPosition;

        private bool isElevatorDisembarking;

        private bool isElevatorEmbarking;

        #endregion

        #region Properties

        public double? ElevatorHorizontalPosition
        {
            get => this.elevatorHorizontalPosition;
            set => this.SetProperty(ref this.elevatorHorizontalPosition, value);
        }

        public double? ElevatorVerticalPosition
        {
            get => this.elevatorVerticalPosition;
            set => this.SetProperty(ref this.elevatorVerticalPosition, value);
        }

        public bool IsElevatorDisembarking
        {
            get => this.isElevatorDisembarking;
            set
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

        public bool IsZeroChain
        {
            get => this.isZeroChain;
            set => this.SetProperty(ref this.isZeroChain, value);
        }

        #endregion

        #region Methods

        public async Task Restart()
        {
            await Task.Delay(this.inputDelay * 1000);
            this.currentState = DepositAndPickUpState.None;
            await this.ExecuteNextStateAsync();
        }

        private HorizontalMovementDirection GetDirection()
        {
            if (this.currentState == DepositAndPickUpState.Deposit)
            {
                return this.bay.Side == WarehouseSide.Front
                    ? HorizontalMovementDirection.Backwards
                    : HorizontalMovementDirection.Forwards;
            }
            else
            {
                return this.bay.Side == WarehouseSide.Front
                    ? HorizontalMovementDirection.Forwards
                    : HorizontalMovementDirection.Backwards;
            }
        }

        private async Task StartMovementAsync()
        {
            try
            {
                if ((this.currentState == DepositAndPickUpState.GotoBay ||
                     this.currentState == DepositAndPickUpState.GotoBayAdjusted) == false)
                {
                    this.IsExecutingProcedure = false;
                    this.ShowNotification($"Stato dovrebbe essere in modalità {DepositAndPickUpState.GotoBay} o {DepositAndPickUpState.GotoBayAdjusted}");
                    return;
                }

                if (this.currentState == DepositAndPickUpState.GotoBay
                    &&
                    !this.IsLoadingUnitInBay)
                {
                    this.IsExecutingProcedure = false;
                    this.ShowNotification($"Imbarco non eseguito causa Cassetto mancante");
                    return;
                }

                if (this.currentState == DepositAndPickUpState.GotoBayAdjusted
                    &&
                    !this.IsLoadingUnitOnElevator)
                {
                    this.IsExecutingProcedure = false;
                    this.ShowNotification($"Sbarco non eseguito causa Cassetto mancante");
                    return;
                }

                var loadingUnitId = this.loadingUnitInBay.Id;

                this.IsWaitingForResponse = true;

                if (this.currentState == DepositAndPickUpState.GotoBay)
                {
                    this.currentState = DepositAndPickUpState.Deposit;
                }
                else
                {
                    this.currentState = DepositAndPickUpState.PickUp;
                }

                await this.machineElevatorWebService.MoveHorizontalAutoAsync(this.GetDirection(), true, loadingUnitId, this.GrossWeight);
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
