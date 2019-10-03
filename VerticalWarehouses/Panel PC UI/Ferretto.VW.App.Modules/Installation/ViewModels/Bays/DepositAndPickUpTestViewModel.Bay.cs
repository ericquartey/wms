using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public partial class DepositAndPickUpTestViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private double? bayPositionHeight;

        private bool isElevatorMovingToBay;

        private bool isPosition1Selected;

        private bool isPosition2Selected;

        private LoadingUnit loadingUnitInBay;

        private DelegateCommand selectBayPosition1Command;

        private DelegateCommand selectBayPosition2Command;

        #endregion

        #region Properties

        public bool BayIsMultiPosition => this.bayManagerService.Bay.Positions.Count() > 1;

        public double? BayPositionHeight
        {
            get => this.bayPositionHeight;
            protected set => this.SetProperty(ref this.bayPositionHeight, value);
        }

        public bool IsElevatorMovingToBay
        {
            get => this.isElevatorMovingToBay;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingToBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsElevatorMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsPosition1Selected
        {
            get => this.isPosition1Selected;
            protected set
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
            protected set
            {
                if (this.SetProperty(ref this.isPosition2Selected, value))
                {
                    this.IsPosition1Selected = !this.IsPosition2Selected;
                }
            }
        }

        public LoadingUnit LoadingUnitInBay
        {
            get => this.loadingUnitInBay;
            protected set => this.SetProperty(ref this.loadingUnitInBay, value);
        }

        public ICommand SelectBayPosition1Command =>
            this.selectBayPosition1Command
            ??
            (this.selectBayPosition1Command = new DelegateCommand(this.SelectBayPosition1));

        public ICommand SelectBayPosition2Command =>
            this.selectBayPosition2Command
            ??
            (this.selectBayPosition2Command = new DelegateCommand(this.SelectBayPosition2));

        #endregion

        #region Methods

        private bool CanMoveToBayHeight()
        {
            return this.BayPositionHeight.HasValue
              &&
              !this.IsWaitingForResponse
              &&
              !this.IsElevatorMoving;
        }

        private async Task MoveToBayHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.machineElevatorService.MoveToVerticalPositionAsync(
                    this.BayPositionHeight.Value,
                    FeedRateCategory.VerticalManualMovementsAfterZero);

                this.IsElevatorMovingToBay = true;

                this.currentState = DepositAndPickUpState.GotoBay;
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
            this.BayPositionHeight = this.bayManagerService.Bay.Positions.First().Height;
        }

        private void SelectBayPosition2()
        {
            this.IsPosition2Selected = true;
            this.BayPositionHeight = this.bayManagerService.Bay.Positions.Last().Height;
        }

        #endregion
    }
}
