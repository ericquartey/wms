using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class VerticalOffsetCalibrationStep1ViewModel : BaseVerticalOffsetCalibrationViewModel
    {
        #region Fields

        private bool canInputCellId;

        private int? inputCellId;

        private bool isElevatorMoving;

        private bool isOperationCompleted;

        private DelegateCommand moveToCellHeightCommand;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationStep1ViewModel(
            IMachineCellsWebService machineCellsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalOffsetProcedureWebService verticalOffsetWebService,
            IMachineElevatorService machineElevatorService)
            : base(machineCellsWebService, machineElevatorWebService, verticalOffsetWebService, machineElevatorService)
        {
        }

        #endregion

        #region Properties

        public bool CanInputCellId
        {
            get => this.canInputCellId;
            private set => this.SetProperty(ref this.canInputCellId, value);
        }

        public int? InputCellId
        {
            get => this.inputCellId;
            set
            {
                if (this.SetProperty(ref this.inputCellId, value)
                    &&
                    this.Cells != null)
                {
                    this.SelectedCell = value == null
                        ? null
                        : this.Cells.SingleOrDefault(c => c.Id == value);
                }
            }
        }

        public bool IsElevatorMoving
        {
            get => this.isElevatorMoving;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMoving, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand MoveToCellHeightCommand =>
            this.moveToCellHeightCommand
            ??
            (this.moveToCellHeightCommand = new DelegateCommand(
                async () => await this.MoveToCellHeightAsync(),
                this.CanExecuteMoveToCellHeightCommand));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanExecuteStopCommand));

        #endregion

        #region Methods

        public override void InitializeSteps()
        {
            this.ShowSteps();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                var parameters = await this.VerticalOffsetWebService.GetParametersAsync();

                this.InputCellId = parameters.ReferenceCellId;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
                this.IsWaitingForResponse = false;
                this.IsElevatorMoving = false;
                this.CanInputCellId = true;
                this.isOperationCompleted = false;
            }
        }

        protected override void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            base.OnPositioningOperationChanged(message);

            switch (message?.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsElevatorMoving = false;

                        this.isOperationCompleted = true;

                        this.NavigateToNextStep();

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.IsElevatorMoving = false;

                        this.isOperationCompleted = false;

                        this.ShowNotification(
                            VW.App.Resources.InstallationApp.ProcedureWasStopped,
                            Services.Models.NotificationSeverity.Warning);

                        break;
                    }
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.moveToCellHeightCommand?.RaiseCanExecuteChanged();
            this.stopCommand?.RaiseCanExecuteChanged();

            this.CanInputCellId = this.Cells != null
                &&
                !this.IsElevatorMoving
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanExecuteMoveToCellHeightCommand()
        {
            return
                this.SelectedCell != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving;
        }

        private bool CanExecuteStopCommand()
        {
            return !this.IsWaitingForResponse && this.IsElevatorMoving;
        }

        private async Task MoveToCellHeightAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.MachineElevatorWebService.MoveToVerticalPositionAsync(
                    this.SelectedCell.Position,
                    this.ProcedureParameters.FeedRate,
                    false,
                    true);

                this.IsElevatorMoving = true;
            }
            catch (Exception ex)
            {
                this.IsElevatorMoving = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void NavigateToNextStep()
        {
            if (this.NavigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalOffsetCalibration.STEP1))
            {
                this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.VerticalOffsetCalibration.STEP2,
                this.SelectedCell,
                trackCurrentView: false);
            }
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, false);
            this.ShowNextStep(true, this.isOperationCompleted, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalOffsetCalibration.STEP2);
            this.ShowAbortStep(true, true);
        }

        private async Task StopAsync()
        {
            try
            {
                await this.MachineElevatorWebService.StopAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
