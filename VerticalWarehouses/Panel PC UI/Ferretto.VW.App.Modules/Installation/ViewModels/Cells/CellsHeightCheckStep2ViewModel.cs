using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class CellsHeightCheckStep2ViewModel : BaseCellsHeightCheckViewModel, IDataErrorInfo
    {
        #region Fields

        private DelegateCommand applyCorrectionCommand;

        private Cell cell;

        private double? initialPosition;

        private double? inputCellHeight;

        private double inputStepValue;

        private bool isElevatorMovingDown;

        private bool isElevatorMovingUp;

        private DelegateCommand moveDownCommand;

        private DelegateCommand moveUpCommand;

        #endregion

        #region Constructors

        public CellsHeightCheckStep2ViewModel(
            IMachineCellsWebService machineCellsWebService,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineElevatorService machineElevatorService)
            : base(machineCellsWebService, machineElevatorWebService, machineElevatorService)
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

        public string Error => string.Join(
              Environment.NewLine,
              this[nameof(this.InputCellHeight)]);

        public double? InitialPosition
        {
            get => this.initialPosition;
            private set => this.SetProperty(ref this.initialPosition, value);
        }

        public double? InputCellHeight
        {
            get => this.inputCellHeight;
            set
            {
                if (this.SetProperty(ref this.inputCellHeight, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

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
                    case nameof(this.InputCellHeight):
                        if (!this.InputCellHeight.HasValue)
                        {
                            return $"InputCellHeight is required.";
                        }

                        if (this.InputCellHeight.Value < 0)
                        {
                            return "InputFinalPosition must be positive.";
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            if (this.Data is Cell cell)
            {
                this.Cell = cell;
            }

            this.InitialPosition = this.CurrentPosition;

            this.InputStepValue = this.ProcedureParameters.Step;
        }

        protected override void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            this.InputCellHeight = this.CurrentPosition - this.initialPosition;

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
            base.RaiseCanExecuteChanged();

            this.moveDownCommand?.RaiseCanExecuteChanged();
            this.moveUpCommand?.RaiseCanExecuteChanged();
            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.Cell = await this.MachineCellsWebService.UpdateHeightAsync(this.Cell.Id, this.CurrentPosition.Value);

                this.ShowNotification("Altezza cella aggiornata.", Services.Models.NotificationSeverity.Success);
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
                string.IsNullOrWhiteSpace(this[nameof(this.InputCellHeight)]);
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
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.CellsHeightCheck.STEP1);
            this.ShowNextStep(true, false);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
