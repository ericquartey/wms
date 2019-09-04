using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalOffsetCalibrationStep2ViewModel : BaseVerticalOffsetCalibrationViewModel
    {
        #region Fields

        private readonly IMachineVerticalOffsetProcedureService verticalOffsetService;

        private DelegateCommand applyCorrectionCommand;

        private Cell cell;

        private decimal? currentVerticalOffset;

        private decimal? inputDisplacement;

        private decimal inputStepValue;

        private bool isElevatorMovingDown;

        private bool isElevatorMovingUp;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationStep2ViewModel(
            IMachineCellsService machineCellsService,
            IMachineElevatorService machineElevatorService,
            IMachineVerticalOffsetProcedureService verticalOffsetService)
            : base(machineCellsService, machineElevatorService, verticalOffsetService)
        {
            if (verticalOffsetService is null)
            {
                throw new ArgumentNullException(nameof(verticalOffsetService));
            }

            this.verticalOffsetService = verticalOffsetService;
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

        public decimal? CurrentVerticalOffset
        {
            get => this.currentVerticalOffset;
            set => this.SetProperty(ref this.currentVerticalOffset, value);
        }

        public string Error => string.Join(
              Environment.NewLine,
              this[nameof(this.InputDisplacement)]);

        public decimal? InputDisplacement
        {
            get => this.inputDisplacement;
            set
            {
                if (this.SetProperty(ref this.inputDisplacement, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal InputStepValue
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
                    case nameof(this.InputDisplacement):
                        if (!this.InputDisplacement.HasValue)
                        {
                            return $"InputOffset is required.";
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            if (this.Data is Cell cell)
            {
                this.Cell = cell;
            }

            try
            {
                var parameters = await this.VerticalOffsetService.GetParametersAsync();

                this.InputStepValue = parameters.StepValue;
                this.CurrentVerticalOffset = parameters.VerticalOffset;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }

            this.ShowSteps();
        }

        protected override void OnCurrentPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            base.OnCurrentPositionChanged(message);

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
                            "Procedura di posizionamento interrotta.",
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

                var newOffset = this.CurrentVerticalOffset.Value - this.InputDisplacement.Value;
                await this.verticalOffsetService.CompleteAsync(newOffset);

                this.CurrentVerticalOffset = newOffset;
                this.InputDisplacement = null;

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
                string.IsNullOrWhiteSpace(this[nameof(this.InputDisplacement)])
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

                await this.MachineElevatorService.MoveVerticalOfDistanceAsync(-this.InputStepValue);

                this.InputDisplacement = this.InputDisplacement ?? 0 - this.InputStepValue;
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

                await this.MachineElevatorService.MoveVerticalOfDistanceAsync(this.InputStepValue);

                this.InputDisplacement = this.InputDisplacement ?? 0 + this.InputStepValue;
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

        private void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalOffsetCalibration.STEP1);
            this.ShowNextStep(true, false);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
