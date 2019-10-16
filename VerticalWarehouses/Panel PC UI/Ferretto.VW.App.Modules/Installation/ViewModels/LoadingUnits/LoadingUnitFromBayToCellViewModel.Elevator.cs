using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public partial class LoadingUnitFromBayToCellViewModel
    {
        #region Fields

        private readonly IMachineElevatorWebService machineElevatorWebService;

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

        private async Task RetrieveElevatorPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.ElevatorVerticalPosition = await this.machineElevatorWebService.GetVerticalPositionAsync();
                this.ElevatorHorizontalPosition = await this.machineElevatorWebService.GetHorizontalPositionAsync();
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

        #endregion
    }
}
