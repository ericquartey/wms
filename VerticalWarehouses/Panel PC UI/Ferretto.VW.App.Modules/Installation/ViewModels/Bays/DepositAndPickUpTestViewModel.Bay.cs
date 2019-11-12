using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class DepositAndPickUpTestViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private bool bayIsMultiPosition;

        private double? bayPositionHeight;

        private bool isElevatorMovingToBay;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private LoadingUnit loadingUnitInBay;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        #endregion

        #region Properties

        public bool BayIsMultiPosition
        {
            get => this.bayIsMultiPosition;
            set => this.SetProperty(ref this.bayIsMultiPosition, value);
        }

        public double? BayPositionHeight
        {
            get => this.bayPositionHeight;
            set => this.SetProperty(ref this.bayPositionHeight, value);
        }

        public bool IsElevatorMovingToBay
        {
            get => this.isElevatorMovingToBay;
            set
            {
                if (this.SetProperty(ref this.isElevatorMovingToBay, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsElevatorMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsPositionDownSelected
        {
            get => this.isPositionDownSelected;
            set
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
            set
            {
                if (this.SetProperty(ref this.isPositionUpSelected, value))
                {
                    this.IsPositionDownSelected = !this.IsPositionUpSelected;
                }
            }
        }

        public LoadingUnit LoadingUnitInBay
        {
            get => this.loadingUnitInBay;
            set => this.SetProperty(ref this.loadingUnitInBay, value);
        }

        public ICommand SelectBayPosition1Command =>
            this.selectBayPositionDownCommand
            ??
            (this.selectBayPositionDownCommand = new DelegateCommand(this.SelectBayPositionDown));

        public ICommand SelectBayPosition2Command =>
            this.selectBayPositionUpCommand
            ??
            (this.selectBayPositionUpCommand = new DelegateCommand(this.SelectBayPositionUp));

        #endregion

        #region Methods

        private async Task MoveToBayHeightAsync()
        {
            try
            {
                if (!(this.currentState == DepositAndPickUpState.None
                    ||
                    this.currentState == DepositAndPickUpState.PickUp
                    ||
                    this.currentState == DepositAndPickUpState.Deposit))
                {
                    this.ShowNotification($"Vai a baia non eseguito, lo stato corrente è {this.currentState.ToString()}");
                    this.IsExecutingProcedure = false;
                    return;
                }

                this.IsWaitingForResponse = true;

                if (this.currentState == DepositAndPickUpState.None)
                {
                    this.currentState = DepositAndPickUpState.GotoBay;
                }
                else
                {
                    this.currentState = DepositAndPickUpState.GotoBayAdjusted;
                }

                await this.machineElevatorWebService.MoveToVerticalPositionAsync(
                        this.BayPositionHeight.Value,
                        this.procedureParameters.FeedRate,
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
