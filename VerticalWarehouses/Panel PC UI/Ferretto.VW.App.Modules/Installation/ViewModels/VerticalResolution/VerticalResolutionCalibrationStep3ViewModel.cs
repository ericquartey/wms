using System;
using System.ComponentModel;
using System.Threading;
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
    public class VerticalResolutionCalibrationStep3ViewModel : BaseVerticalResolutionCalibrationViewModel, IDataErrorInfo
    {
        #region Fields

        private DelegateCommand applyCorrectionCommand;

        private decimal? finalPosition;

        private decimal? initialPosition;

        private decimal? inputMeasuredFinalPosition;

        private bool isProcedureCompleted;

        private bool isRetrievingNewResolution;

        private decimal? measuredDistance;

        private decimal? measuredInitialPosition;

        private DelegateCommand moveToInitialPositionCommand;

        private decimal? newResolution;

        private VerticalResolutionCalibrationData procedureParameters;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationStep3ViewModel(
            IEventAggregator eventAggregator,
            IMachineElevatorService machineElevatorService,
            IMachineResolutionCalibrationProcedureService resolutionCalibrationService)
            : base(eventAggregator, machineElevatorService, resolutionCalibrationService)
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

        public decimal? FinalPosition
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

        public decimal? InputMeasuredFinalPosition
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

        public decimal? MeasuredDistance
        {
            get => this.measuredDistance;
            set => this.SetProperty(ref this.measuredDistance, value);
        }

        public decimal? MeasuredInitialPosition
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

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.RetrieveInputData();

            this.ShowNotification(VW.App.Resources.InstallationApp.ElevatorIsInFinalPosition);

            this.ShowSteps();
        }

        protected override void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message)
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

            base.OnAutomationMessageReceived(message);
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

                await this.MachineElevatorService.UpdateResolutionAsync(this.NewResolution.Value);

                this.ShowNotification(
                    VW.App.Resources.InstallationApp.VerticalAxisResolutionUpdated,
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

                await this.MachineElevatorService.MoveToVerticalPositionAsync(
                    this.InitialPosition.Value,
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

        private void RetrieveInputData()
        {
            if (this.Data is VerticalResolutionCalibrationData data)
            {
                this.procedureParameters = data;

                this.InitialPosition = this.procedureParameters.InitialPosition;
                this.FinalPosition = this.procedureParameters.FinalPosition;
                this.MeasuredInitialPosition = this.procedureParameters.MeasuredInitialPosition;
                this.CurrentResolution = this.procedureParameters.CurrentResolution;
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

        private void ShowSteps()
        {
            this.ShowPrevStep(true, true, nameof(Utils.Modules.Installation), Utils.Modules.Installation.VerticalResolutionCalibration.STEP2);
            this.ShowNextStep(true, false);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
