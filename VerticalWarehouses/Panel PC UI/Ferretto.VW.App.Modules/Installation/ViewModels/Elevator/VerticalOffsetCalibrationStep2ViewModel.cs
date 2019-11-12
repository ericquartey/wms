using System;
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
    internal sealed class VerticalOffsetCalibrationStep2ViewModel : BaseVerticalOffsetCalibrationViewModel
    {
        #region Fields

        private DelegateCommand applyCorrectionCommand;

        private Cell cell;

        private double? currentVerticalOffset;

        private double? displacement;

        private double inputStepValue;

        private bool isElevatorMovingDown;

        private bool isElevatorMovingUp;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationStep2ViewModel(
            IMachineCellsWebService machineCellsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalOffsetProcedureWebService verticalOffsetWebService,
            IMachineElevatorService machineElevatorService)
            : base(machineCellsWebService, machineElevatorWebService, verticalOffsetWebService, machineElevatorService)
        {
        }

        #endregion

        #region Properties

        public ICommand ApplyCorrectionCommand =>
            this.applyCorrectionCommand
            ??
            (this.applyCorrectionCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync(),
                this.CanExecuteApplyCorrectionCommand));

        public Cell Cell
        {
            get => this.cell;
            set => this.SetProperty(ref this.cell, value);
        }

        public double? CurrentVerticalOffset
        {
            get => this.currentVerticalOffset;
            set => this.SetProperty(ref this.currentVerticalOffset, value);
        }

        public double? Displacement
        {
            get => this.displacement;
            set
            {
                if (this.SetProperty(ref this.displacement, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.Displacement)]);

        public double InputStepValue
        {
            get => this.inputStepValue;
            set
            {
                if (this.SetProperty(ref this.inputStepValue, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingDown
        {
            get => this.isElevatorMovingDown;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingDown, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsElevatorMovingUp
        {
            get => this.isElevatorMovingUp;
            private set
            {
                if (this.SetProperty(ref this.isElevatorMovingUp, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand MoveDownCommand =>
            this.moveDownCommand
            ??
            (this.moveDownCommand = new DelegateCommand(
                async () => await this.MoveDownAsync(),
                this.CanExecuteMoveDownCommand));

        public ICommand MoveUpCommand =>
            this.moveUpCommand
            ??
            (this.moveUpCommand = new DelegateCommand(
                async () => await this.MoveUpAsync(),
                this.CanExecuteMoveUpCommand));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.Displacement):
                        if (!this.Displacement.HasValue)
                        {
                            return $"Displacement is required.";
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void InitializeSteps()
        {
            this.ShowSteps();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.ShowNotification(VW.App.Resources.InstallationApp.ElevatorIsCellPosition);

            this.Cell = this.Data as Cell ?? this.Cell;

            this.InputStepValue = this.ProcedureParameters.Step;

            await this.RetrieveVerticalOffset();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            if (e.MachinePowerState != MachinePowerState.Powered)
            {
                this.IsWaitingForResponse = false;
                this.IsElevatorMovingUp = false;
                this.IsElevatorMovingDown = false;
            }
        }

        protected override void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            base.OnPositioningOperationChanged(message);

            switch (message?.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    {
                        this.IsElevatorMovingDown = false;
                        this.IsElevatorMovingUp = false;

                        break;
                    }

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    {
                        this.IsElevatorMovingDown = false;
                        this.IsElevatorMovingUp = false;

                        this.ShowNotification(
                            VW.App.Resources.InstallationApp.ProcedureWasStopped,
                            Services.Models.NotificationSeverity.Warning);

                        break;
                    }
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.moveDownCommand?.RaiseCanExecuteChanged();
            this.moveUpCommand?.RaiseCanExecuteChanged();
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var newOffset = this.CurrentVerticalOffset.Value - this.Displacement.Value;
                await this.VerticalOffsetWebService.CompleteAsync(newOffset);

                this.CurrentVerticalOffset = newOffset;
                this.Displacement = null;

                this.ShowNotification("Offset asse verticale aggiornato.", Services.Models.NotificationSeverity.Success);
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

        private bool CanExecuteApplyCorrectionCommand()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMovingUp
                &&
                !this.IsElevatorMovingDown
                &&
                string.IsNullOrWhiteSpace(this[nameof(this.Displacement)])
                &&
                this.CurrentVerticalOffset.HasValue;
        }

        private bool CanExecuteMoveDownCommand()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMovingUp
                &&
                !this.IsElevatorMovingDown
                &&
                string.IsNullOrWhiteSpace(this[nameof(this.InputStepValue)]);
        }

        private bool CanExecuteMoveUpCommand()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMovingUp
                &&
                !this.IsElevatorMovingDown
                &&
                string.IsNullOrWhiteSpace(this[nameof(this.InputStepValue)]);
        }

        private async Task MoveDownAsync()
        {
            try
            {
                this.IsElevatorMovingDown = true;
                this.IsWaitingForResponse = true;

                await this.MachineElevatorWebService.MoveVerticalOfDistanceAsync(-this.InputStepValue);

                this.Displacement = (this.Displacement ?? 0) - this.InputStepValue;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingDown = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MoveUpAsync()
        {
            try
            {
                this.IsElevatorMovingUp = true;
                this.IsWaitingForResponse = true;

                await this.MachineElevatorWebService.MoveVerticalOfDistanceAsync(this.InputStepValue);

                this.Displacement = (this.Displacement ?? 0) + this.InputStepValue;
            }
            catch (Exception ex)
            {
                this.IsElevatorMovingUp = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task RetrieveVerticalOffset()
        {
            try
            {
                this.CurrentVerticalOffset = await this.MachineElevatorWebService.GetVerticalOffsetAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalOffsetCalibration.STEP1);
            this.ShowNextStep(true, false);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
