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

        private decimal? elevatorHorizontalPosition;

        private decimal? elevatorVerticalPosition;

        private LoadingUnit embarkedLoadingUnit;

        private bool isElevatorDisembarking;

        private bool isElevatorEmbarking;

        private bool isTuningChain;

        private DelegateCommand tuningBayCommand;

        private DelegateCommand tuningChainCommand;

        private bool direction;

        #endregion

        #region Properties

        public decimal? ElevatorHorizontalPosition
        {
            get => this.elevatorHorizontalPosition;
            protected set => this.SetProperty(ref this.elevatorHorizontalPosition, value);
        }

        public decimal? ElevatorVerticalPosition
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

        private bool CanDisembark()
        {
            return

                // !this.IsWaitingForResponse
                // &&
                !this.IsElevatorMoving
                &&
                this.Sensors.LuPresentInMachineSideBay1
                &&
                this.Sensors.LuPresentInOperatorSideBay1;
        }

        private bool CanEmbark()
        {
            return

                // !this.IsWaitingForResponse
                // &&
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

        private async Task StartMovementAsync(HorizontalMovementDirection direction, bool isOnBoard)
        {
            try
            {
                this.ShowNotification(string.Empty);
                await this.machineElevatorService.MoveHorizontalAutoAsync(direction, isOnBoard);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task TuningBay()
        {
            await Task.Delay(1);
        }

        #endregion
    }
}
