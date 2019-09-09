using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private readonly IMachineElevatorService machineElevatorService;

        private DelegateCommand disembarkBackwardsCommand;

        private DelegateCommand disembarkForwardsCommand;

        private decimal? elevatorHorizontalPosition;

        private decimal? elevatorVerticalPosition;

        private DelegateCommand embarkBackwardsCommand;

        private LoadingUnit embarkedLoadingUnit;

        private DelegateCommand embarkForwardsCommand;

        private bool isElevatorDisembarking;

        private bool isElevatorEmbarking;

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

        public ICommand EmbarkBackwardsCommand =>
            this.embarkBackwardsCommand
            ??
            (this.embarkBackwardsCommand = new DelegateCommand(async () => await this.Embark(HorizontalMovementDirection.Backwards), this.CanEmbark));

        public LoadingUnit EmbarkedLoadingUnit
        {
            // TODO  for the moment we use only presence sensors
            //get => this.embarkedLoadingUnit;
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

        private bool CanDisembark()
        {
            return
                //!this.IsWaitingForResponse
                //&&
                !this.IsElevatorMoving
                &&
                this.Sensors.LuPresentInMachineSideBay1
                &&
                this.Sensors.LuPresentInOperatorSideBay1;
        }

        private bool CanEmbark()
        {
            return
                //!this.IsWaitingForResponse
                //&&
                !this.IsElevatorMoving
                &&
                !this.Sensors.LuPresentInMachineSideBay1
                &&
                !this.Sensors.LuPresentInOperatorSideBay1;
        }

        private async Task Disembark(HorizontalMovementDirection direction)
        {
            await this.StartMovementAsync(direction, true);
        }

        private async Task Embark(HorizontalMovementDirection direction)
        {
            await this.StartMovementAsync(direction, false);
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

        #endregion
    }
}
