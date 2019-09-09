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

        private DelegateCommand disembarkCommand;

        private decimal? elevatorHorizontalPosition;

        private decimal? elevatorVerticalPosition;

        private DelegateCommand embarkCommand;

        private LoadingUnit embarkedLoadingUnit;

        private bool isElevatorDisembarking;

        private bool isElevatorEmbarking;

        #endregion

        #region Properties

        public ICommand DisembarkCommand =>
            this.disembarkCommand
            ??
            (this.disembarkCommand = new DelegateCommand(this.Disembark, this.CanDisembark));

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

        public ICommand EmbarkCommand =>
            this.embarkCommand
            ??
            (this.embarkCommand = new DelegateCommand(this.Embark, this.CanEmbark));

        public LoadingUnit EmbarkedLoadingUnit
        {
            get => this.embarkedLoadingUnit;
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

        #endregion

        #region Methods

        private bool CanDisembark()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving;
        }

        private bool CanEmbark()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving;
        }

        private void Disembark()
        {
            this.ShowNotification("Non ancora implementata :P", Services.Models.NotificationSeverity.Warning);
        }

        private void Embark()
        {
            this.ShowNotification("Non ancora implementata :P", Services.Models.NotificationSeverity.Warning);
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

        #endregion
    }
}
