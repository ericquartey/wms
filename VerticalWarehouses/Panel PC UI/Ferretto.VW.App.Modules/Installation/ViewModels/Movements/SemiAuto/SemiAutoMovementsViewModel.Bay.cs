using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private BayNumber bayNumber;

        private double? bayPositionHeight;

        private bool isElevatorMovingToBay;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private DelegateCommand moveToBayHeightCommand;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        #endregion

        #region Properties

        public BayNumber BayNumber
        {
            get => this.bayNumber;
            set => this.SetProperty(ref this.bayNumber, value);
        }

        public double? BayPositionHeight
        {
            get => this.bayPositionHeight;
            private set => this.SetProperty(ref this.bayPositionHeight, value);
        }

        public bool IsElevatorMovingToBay
        {
            get => this.isElevatorMovingToBay;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsPositionDownSelected
        {
            get => this.isPositionDownSelected;
            private set
            {
                if (this.SetProperty(ref this.isPositionDownSelected, value))
                {
                    this.IsPositionUpSelected = !this.IsPositionDownSelected;
                }
            }
        }

        public bool IsPositionUpSelected
        {
            get => this.isPositionUpSelected;
            private set
            {
                if (this.SetProperty(ref this.isPositionUpSelected, value))
                {
                    this.IsPositionDownSelected = !this.IsPositionUpSelected;
                }
            }
        }

        public ICommand MoveToBayHeightCommand =>
            this.moveToBayHeightCommand
            ??
            (this.moveToBayHeightCommand = new DelegateCommand(
                async () => await this.MoveToBayHeightAsync(),
                this.CanMoveToBayHeight));

        public ICommand SelectBayPositionDownCommand =>
            this.selectBayPositionDownCommand
            ??
            (this.selectBayPositionDownCommand = new DelegateCommand(() => this.SelectBayPositionDown(), this.CanSelectBayPosition));

        public ICommand SelectBayPositionUpCommand =>
            this.selectBayPositionUpCommand
            ??
            (this.selectBayPositionUpCommand = new DelegateCommand(() => this.SelectBayPositionUp(), this.CanSelectBayPosition));

        #endregion

        #region Methods

        private bool CanMoveToBayHeight()
        {
            return this.BayPositionHeight.HasValue
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                (this.sensorsService.IsZeroChain || (this.sensorsService.Sensors.LuPresentInMachineSide && this.sensorsService.Sensors.LuPresentInOperatorSide));
        }

        private bool CanSelectBayPosition()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                (this.sensorsService.IsZeroChain || (this.sensorsService.Sensors.LuPresentInMachineSide && this.sensorsService.Sensors.LuPresentInOperatorSide));
        }

        private async Task MoveToBayHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.InputHeight = this.BayPositionHeight.HasValue ? this.BayPositionHeight.Value : 0;

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                    this.BayPositionHeight.Value,
                    this.procedureParameters.FeedRateAfterZero,
                    false,
                    true);

                this.IsElevatorMovingToBay = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingToBay = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void SelectBayPositionDown()
        {
            this.IsPositionDownSelected = true;
            this.BayPositionHeight = this.bay.Positions.First().Height;
        }

        private void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.BayPositionHeight = this.bay.Positions.Last().Height;
        }

        #endregion
    }
}
