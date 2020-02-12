using System.Threading.Tasks;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed partial class DepositAndPickUpTestViewModel
    {
        #region Fields

        private DepositAndPickUpState currentState;

        private bool isElevatorDisembarking;

        private bool isElevatorEmbarking;

        #endregion

        #region Properties

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
                    this.ShowNotification(string.Format(InstallationApp.StateShouldBeInMode, DepositAndPickUpState.GotoBay, DepositAndPickUpState.GotoBayAdjusted));
                    return;
                }

                if (this.currentState == DepositAndPickUpState.GotoBay
#if CHECK_BAY_SENSOR
                    && !this.sensorsService.IsLoadingUnitInBay
#endif
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
                    )
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
                {
                    this.IsExecutingProcedure = false;
                    this.ShowNotification(InstallationApp.BoardingNotExecutedMissingDrawer);
                    return;
                }

                if (this.currentState == DepositAndPickUpState.GotoBayAdjusted
                    &&
                    !this.sensorsService.IsLoadingUnitOnElevator)
                {
                    this.IsExecutingProcedure = false;
                    this.ShowNotification(InstallationApp.LandingNotExecutedMissingDrawer);
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

                // await this.machineElevatorWebService.MoveHorizontalAutoAsync(this.GetDirection(), true, loadingUnitId, this.GrossWeight);
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
