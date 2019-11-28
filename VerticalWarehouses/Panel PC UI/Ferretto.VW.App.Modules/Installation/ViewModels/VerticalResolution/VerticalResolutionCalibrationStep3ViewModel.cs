using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class VerticalResolutionCalibrationStep3ViewModel : BaseVerticalResolutionCalibrationViewModel, IDataErrorInfo
    {
        #region Fields

        private DelegateCommand applyCorrectionCommand;

        private double? finalPosition;

        private double? initialPosition;

        private double? inputMeasuredFinalPosition;

        private bool isProcedureCompleted;

        private bool isRetrievingNewResolution;

        private double? measuredDistance;

        private double? measuredInitialPosition;

        private DelegateCommand moveToInitialPositionCommand;

        private decimal? newResolution;

        private CancellationTokenSource tokenSource;

        private VerticalResolutionWizardData wizardData;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationStep3ViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorWebService machineElevatorWebService,
            IMachineVerticalResolutionCalibrationProcedureWebService resolutionCalibrationWebService,
            IMachineElevatorService machineElevatorService,
            IHealthProbeService healthProbeService)
            : base(eventAggregator, machineElevatorWebService, resolutionCalibrationWebService, machineElevatorService, healthProbeService)
        {
        }

        #endregion

        #region Properties

        public ICommand ApplyCorrectionCommand =>
            this.applyCorrectionCommand
            ??
            (this.applyCorrectionCommand = new DelegateCommand(
                async () => await this.ApplyCorrectionAsync(),
                this.CanApplyCorrection));

        public bool CanInputFinalPosition => !this.IsExecutingProcedure && !this.IsProcedureCompleted;

        public string Error => string.Join(
            Environment.NewLine,
            this[nameof(this.InputMeasuredFinalPosition)],
            this[nameof(this.NewResolution)]);

        public double? FinalPosition
        {
            get => this.finalPosition;
            set
            {
                if (this.SetProperty(ref this.finalPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

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

        public double? InputMeasuredFinalPosition
        {
            get => this.inputMeasuredFinalPosition;
            set
            {
                if (this.SetProperty(ref this.inputMeasuredFinalPosition, value))
                {
                    this.RaiseCanExecuteChanged();

                    this.RetrieveNewResolutionAsync();
                }
            }
        }

        public bool IsProcedureCompleted
        {
            get => this.isProcedureCompleted;
            set
            {
                if (this.SetProperty(ref this.isProcedureCompleted, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsRetrievingNewResolution
        {
            get => this.isRetrievingNewResolution;
            set => this.SetProperty(ref this.isRetrievingNewResolution, value);
        }

        public double? MeasuredDistance
        {
            get => this.measuredDistance;
            set => this.SetProperty(ref this.measuredDistance, value);
        }

        public double? MeasuredInitialPosition
        {
            get => this.measuredInitialPosition;
            set
            {
                if (this.SetProperty(ref this.measuredInitialPosition, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand MoveToInitialPositionCommand =>
            this.moveToInitialPositionCommand
            ??
            (this.moveToInitialPositionCommand = new DelegateCommand(
                async () => await this.MoveToInitialPositionAsync(),
                this.CanMoveToInitialPosition));

        public decimal? NewResolution
        {
            get => this.newResolution;
            set
            {
                if (this.SetProperty(ref this.newResolution, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputMeasuredFinalPosition):
                        if (!this.InputMeasuredFinalPosition.HasValue)
                        {
                            return $"InputMeasuredFinalPosition is required.";
                        }

                        if (this.InputMeasuredFinalPosition.Value <= 0)
                        {
                            return "InputMeasuredFinalPosition must be strictly positive.";
                        }

                        break;

                    case nameof(this.NewResolution):
                        if (!this.NewResolution.HasValue)
                        {
                            return $"NewResolution is required.";
                        }

                        if (this.NewResolution.Value <= 0)
                        {
                            return "NewResolution must be strictly positive.";
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.CurrentPosition = null;
            this.CurrentResolution = null;
            this.FinalPosition = null;
            this.InitialPosition = null;
            this.InputMeasuredFinalPosition = null;
            this.IsExecutingProcedure = false;
            this.IsProcedureCompleted = false;
            this.IsRetrievingNewResolution = false;
            this.IsWaitingForResponse = false;
            this.MeasuredDistance = null;
            this.MeasuredInitialPosition = null;
            this.NewResolution = null;
        }

        public override void InitializeSteps()
        {
            this.ShowSteps();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.RetrieveWizardData();

            this.ShowNotification(VW.App.Resources.InstallationApp.ElevatorIsInFinalPosition);
        }

        protected override void OnPositioningOperationChanged(NotificationMessageUI<PositioningMessageData> message)
        {
            if (this.IsExecutingProcedure)
            {
                if (message.Data.AxisMovement == Axis.Vertical
                    &&
                    message.Status == MessageStatus.OperationEnd)
                {
                    this.ShowNotification(VW.App.Resources.InstallationApp.ElevatorIsInInitialPosition);

                    this.IsProcedureCompleted = true;
                }
            }

            base.OnPositioningOperationChanged(message);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.applyCorrectionCommand?.RaiseCanExecuteChanged();
            this.moveToInitialPositionCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.CanInputFinalPosition));
        }

        private async Task ApplyCorrectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                await this.MachineElevatorWebService.UpdateVerticalResolutionAsync(this.NewResolution.Value);

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.InformationSuccessfullyUpdated,
                    Services.Models.NotificationSeverity.Success);

                this.CurrentResolution = this.NewResolution;
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

        private bool CanApplyCorrection()
        {
            return
               !this.IsExecutingProcedure
               &&
               !this.IsWaitingForResponse
               &&
               !this.IsProcedureCompleted
               &&
               string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanMoveToInitialPosition()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsProcedureCompleted;
        }

        private async Task MoveToInitialPositionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                await this.MachineElevatorWebService.MoveManualToVerticalPositionAsync(
                    this.InitialPosition.Value,
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

        private async void RetrieveNewResolutionAsync()
        {
            if (this.InputMeasuredFinalPosition.HasValue
                &&
                this.MeasuredInitialPosition.HasValue)
            {
                this.MeasuredDistance = this.InputMeasuredFinalPosition - this.MeasuredInitialPosition;

                this.tokenSource?.Cancel(false);
                this.tokenSource = new CancellationTokenSource();

                try
                {
                    const int callDelayMilliseconds = 300;

                    await Task
                        .Delay(callDelayMilliseconds, this.tokenSource.Token)
                        .ContinueWith(
                            async t => await this.RetrieveNewResolutionAsync(this.tokenSource.Token),
                            this.tokenSource.Token,
                            TaskContinuationOptions.NotOnCanceled,
                            TaskScheduler.Current)
                        .ConfigureAwait(true);
                }
                catch (TaskCanceledException)
                {
                    this.IsRetrievingNewResolution = false;
                }
            }
        }

        private async Task RetrieveNewResolutionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var exepectedDistance = this.FinalPosition.Value - this.InitialPosition.Value;

                this.IsRetrievingNewResolution = true;

                this.ClearNotifications();

                this.NewResolution = await this.ResolutionCalibrationService
                    .GetAdjustedResolutionAsync(
                        this.MeasuredDistance.Value,
                        exepectedDistance,
                        cancellationToken);

                this.IsRetrievingNewResolution = false;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.NewResolution = null;
            }
        }

        private void RetrieveWizardData()
        {
            if (this.Data is VerticalResolutionWizardData data)
            {
                this.wizardData = data;

                this.InitialPosition = this.wizardData.InitialPosition;
                this.FinalPosition = this.wizardData.FinalPosition;
                this.MeasuredInitialPosition = this.wizardData.MeasuredInitialPosition;
                this.CurrentResolution = this.wizardData.CurrentResolution;
            }
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalResolutionCalibration.STEP2);
            this.ShowNextStep(true, false);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
