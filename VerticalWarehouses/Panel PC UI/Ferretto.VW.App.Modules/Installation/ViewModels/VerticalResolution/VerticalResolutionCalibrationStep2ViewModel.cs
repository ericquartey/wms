using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalResolutionCalibrationStep2ViewModel : BaseVerticalResolutionCalibrationViewModel, IDataErrorInfo
    {
        #region Fields

        private decimal? initialPosition;

        private decimal? inputFinalPosition;

        private decimal? inputMeasuredInitialPosition;

        private bool isOperationCompleted;

        private DelegateCommand moveToPositionCommand;

        private VerticalResolutionCalibrationData procedureParameters;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationStep2ViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorService machineElevatorService,
            IMachineResolutionCalibrationProcedureService resolutionCalibrationService)
            : base(eventAggregator, machineElevatorService, resolutionCalibrationService)
        {
        }

        #endregion

        #region Properties

        public string Error => string.Join(
              System.Environment.NewLine,
              this[nameof(this.InputFinalPosition)],
              this[nameof(this.InputMeasuredInitialPosition)]);

        public decimal? InitialPosition
        {
            get => this.initialPosition;
            set
            {
                if (this.SetProperty(ref this.initialPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal? InputFinalPosition
        {
            get => this.inputFinalPosition;
            set
            {
                if (this.SetProperty(ref this.inputFinalPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public decimal? InputMeasuredInitialPosition
        {
            get => this.inputMeasuredInitialPosition;
            set
            {
                if (this.SetProperty(ref this.inputMeasuredInitialPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand MoveToPositionCommand =>
           this.moveToPositionCommand
           ??
           (this.moveToPositionCommand = new DelegateCommand(
               async () => await this.MoveToPositionAsync(),
               this.CanMoveToPosition));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputFinalPosition):
                        if (!this.InputFinalPosition.HasValue)
                        {
                            return $"InputFinalPosition is required.";
                        }

                        if (this.InputFinalPosition.Value <= 0)
                        {
                            return "InputFinalPosition must be strictly positive.";
                        }

                        break;

                    case nameof(this.InputMeasuredInitialPosition):
                        if (!this.InputMeasuredInitialPosition.HasValue)
                        {
                            return $"InputMeasuredInitialPosition is required.";
                        }

                        if (this.InputMeasuredInitialPosition.Value <= 0)
                        {
                            return "InputMeasuredInitialPosition must be strictly positive.";
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
            this.ShowSteps();

            await base.OnNavigatedAsync();

            this.RetrieveInputData();

            this.ShowNotification(VW.App.Resources.InstallationApp.ElevatorIsInInitialPosition);
        }

        protected override void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            base.OnAutomationMessageReceived(message);

            if (message.Status == MessageStatus.OperationEnd)
            {
                this.isOperationCompleted = true;
                this.NavigateToNextStep();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.moveToPositionCommand?.RaiseCanExecuteChanged();
        }

        private bool CanMoveToPosition()
        {
            return
               !this.IsExecutingProcedure
               &&
               !this.IsWaitingForResponse
               &&
               string.IsNullOrWhiteSpace(this.Error);
        }

        private async Task MoveToPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = false;

                await this.MachineElevatorService.MoveToVerticalPositionAsync(
                    this.InputFinalPosition.Value,
                    FeedRateCategory.VerticalResolutionCalibration);
            }
            catch (Exception ex)
            {
                this.IsExecutingProcedure = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void NavigateToNextStep()
        {
            this.procedureParameters.FinalPosition = this.InputFinalPosition.Value;
            this.procedureParameters.MeasuredInitialPosition = this.InputMeasuredInitialPosition.Value;

            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.VerticalResolutionCalibration.STEP3,
                this.procedureParameters,
                trackCurrentView: false);
        }

        private void RetrieveInputData()
        {
            if (this.Data is VerticalResolutionCalibrationData data)
            {
                this.procedureParameters = data;

                this.InputFinalPosition = this.procedureParameters.FinalPosition;
                this.InitialPosition = this.procedureParameters.InitialPosition;
                this.CurrentResolution = this.procedureParameters.CurrentResolution;
            }
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalResolutionCalibration.STEP1);
            this.ShowNextStep(true, this.isOperationCompleted, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalResolutionCalibration.STEP3);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
