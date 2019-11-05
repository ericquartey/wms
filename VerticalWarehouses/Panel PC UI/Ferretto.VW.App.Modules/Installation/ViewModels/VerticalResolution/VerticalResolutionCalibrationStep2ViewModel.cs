using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class VerticalResolutionCalibrationStep2ViewModel : BaseVerticalResolutionCalibrationViewModel, IDataErrorInfo
    {
        #region Fields

        private double? initialPosition;

        private double? inputFinalPosition;

        private double? inputMeasuredInitialPosition;

        private bool isOperationCompleted;

        private DelegateCommand moveToPositionCommand;

        private VerticalResolutionWizardData wizardData;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationStep2ViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationService)
            : base(eventAggregator, machineElevatorWebService, resolutionCalibrationService)
        {
        }

        #endregion

        #region Properties

        public string Error => string.Join(
              System.Environment.NewLine,
              this[nameof(this.InputFinalPosition)],
              this[nameof(this.InputMeasuredInitialPosition)]);

        public double? InitialPosition
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

        public double? InputFinalPosition
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

        public double? InputMeasuredInitialPosition
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

        public override async Task OnAppearedAsync()
        {
            this.ShowSteps();

            await base.OnAppearedAsync();

            this.RetrieveWizardData();

            this.ShowNotification(VW.App.Resources.InstallationApp.ElevatorIsInInitialPosition);
        }

        protected override void OnElevatorPositionChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            base.OnElevatorPositionChanged(message);

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
                this.IsExecutingProcedure = true;

                await this.MachineElevatorWebService.MoveToVerticalPositionAsync(
                    this.InputFinalPosition.Value,
                    this.ProcedureParameters.FeedRate,
                    false,
                    false);
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
            if (this.NavigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalResolutionCalibration.STEP2))
            {
                this.wizardData.FinalPosition = this.InputFinalPosition.Value;
                this.wizardData.MeasuredInitialPosition = this.InputMeasuredInitialPosition.Value;

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.VerticalResolutionCalibration.STEP3,
                    this.wizardData,
                    trackCurrentView: false);
            }
        }

        private void RetrieveWizardData()
        {
            if (this.Data is VerticalResolutionWizardData data)
            {
                this.wizardData = data;

                this.InputFinalPosition = this.wizardData.FinalPosition;
                this.InitialPosition = this.wizardData.InitialPosition;
                this.CurrentResolution = this.wizardData.CurrentResolution;
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
