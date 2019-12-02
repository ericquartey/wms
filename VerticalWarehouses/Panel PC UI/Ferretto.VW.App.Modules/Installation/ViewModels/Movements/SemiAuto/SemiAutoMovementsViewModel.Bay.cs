using System;
using System.Diagnostics;
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

        private Bay bay;

        private bool bayIsMultiPosition;

        private bool isElevatorMovingToBay;

        private bool isPositionDownSelected;

        private bool isPositionUpSelected;

        private DelegateCommand moveToBayPositionCommand;

        private DelegateCommand selectBayPositionDownCommand;

        private DelegateCommand selectBayPositionUpCommand;

        private BayPosition selectedBayPosition;

        #endregion

        #region Properties

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

        public ICommand MoveToBayPositionCommand =>
            this.moveToBayPositionCommand
            ??
            (this.moveToBayPositionCommand = new DelegateCommand(
                async () => await this.MoveToBayPositionAsync(),
                this.CanMoveToBayPosition));

        public ICommand SelectBayPositionDownCommand =>
            this.selectBayPositionDownCommand
            ??
            (this.selectBayPositionDownCommand = new DelegateCommand(
                this.SelectBayPositionDown,
                this.CanSelectBayPosition));

        public ICommand SelectBayPositionUpCommand =>
            this.selectBayPositionUpCommand
            ??
            (this.selectBayPositionUpCommand = new DelegateCommand(
                this.SelectBayPositionUp,
                this.CanSelectBayPosition));

        public BayPosition SelectedBayPosition
        {
            get => this.selectedBayPosition;
            private set
            {
                if (this.SetProperty(ref this.selectedBayPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        private bool CanMoveToBayPosition()
        {
            return
                !this.KeyboardOpened
                &&
                this.SelectedBayPosition != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving
                &&
                this.moveToBayPositionPolicy?.IsAllowed == true;
        }

        private bool CanSelectBayPosition()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsMoving;
        }

        private async Task MoveToBayPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                Debug.Assert(
                    this.SelectedBayPosition != null,
                    "A bay position should be selected.");

                this.InputHeight = this.SelectedBayPosition.Height;

                await this.machineElevatorWebService.MoveToBayPositionAsync(
                    this.SelectedBayPosition.Id,
                    computeElongation: true,
                    performWeighting: false);

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
            this.SelectedBayPosition = this.bay.Positions.Single(p => p.Height == this.bay.Positions.Min(pos => pos.Height));
        }

        private void SelectBayPositionUp()
        {
            this.IsPositionUpSelected = true;
            this.SelectedBayPosition = this.bay.Positions.Single(p => p.Height == this.bay.Positions.Max(pos => pos.Height));
        }

        #endregion
    }
}
