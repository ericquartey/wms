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

        private bool isPosition1Selected;

        private bool isPosition2Selected;

        private DelegateCommand moveToBayHeightCommand;

        private DelegateCommand selectBayPosition1Command;

        private DelegateCommand selectBayPosition2Command;

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

        public bool IsPosition1Selected
        {
            get => this.isPosition1Selected;
            private set
            {
                if (this.SetProperty(ref this.isPosition1Selected, value))
                {
                    this.IsPosition2Selected = !this.IsPosition1Selected;
                }
            }
        }

        public bool IsPosition2Selected
        {
            get => this.isPosition2Selected;
            private set
            {
                if (this.SetProperty(ref this.isPosition2Selected, value))
                {
                    this.IsPosition1Selected = !this.IsPosition2Selected;
                }
            }
        }

        public ICommand MoveToBayHeightCommand =>
            this.moveToBayHeightCommand
            ??
            (this.moveToBayHeightCommand = new DelegateCommand(
                async () => await this.MoveToBayHeightAsync(),
                this.CanMoveToBayHeight));

        public ICommand SelectBayPosition1Command =>
            this.selectBayPosition1Command
            ??
            (this.selectBayPosition1Command = new DelegateCommand(() => this.SelectBayPosition1(), this.CanSelectBayPosition));

        public ICommand SelectBayPosition2Command =>
            this.selectBayPosition2Command
            ??
            (this.selectBayPosition2Command = new DelegateCommand(() => this.SelectBayPosition2(), this.CanSelectBayPosition));

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
                (this.sensorsService.IsZeroChain || (this.sensorsService.Sensors.LuPresentInMachineSideBay1 && this.sensorsService.Sensors.LuPresentInOperatorSideBay1));
        }

        private bool CanSelectBayPosition()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                (this.IsZeroChain || (this.Sensors.LuPresentInMachineSideBay1 && this.Sensors.LuPresentInOperatorSideBay1));
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
                    false);

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

        private void SelectBayPosition1()
        {
            this.IsPosition1Selected = true;
            this.BayPositionHeight = this.bay.Positions.First().Height;
        }

        private void SelectBayPosition2()
        {
            this.IsPosition2Selected = true;
            this.BayPositionHeight = this.bay.Positions.Last().Height;
        }

        #endregion
    }
}
