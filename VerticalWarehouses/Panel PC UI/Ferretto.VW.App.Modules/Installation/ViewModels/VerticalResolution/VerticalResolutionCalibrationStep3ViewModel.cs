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

        private DelegateCommand acceptCommand;

        private decimal? finalPosition;

        private decimal? initialPosition;

        private decimal? inputMeasuredFinalPosition;

        private bool isRetrievingNewResolution;

        private decimal? measuredDistance;

        private decimal? measuredInitialPosition;

        private decimal? newResolution;

        private VerticalResolutionCalibrationData procedureParameters;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public VerticalResolutionCalibrationStep3ViewModel(
                    IEventAggregator eventAggregator,
            IMachineResolutionCalibrationProcedureService resolutionCalibrationService)
            : base(eventAggregator, resolutionCalibrationService)
        {
        }

        #endregion

        #region Properties

        public ICommand AcceptCommand =>
            this.acceptCommand
            ??
            (this.acceptCommand = new DelegateCommand(
                async () => await this.AcceptAsync(),
                this.CanExecuteAcceptCommand));

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

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.RetrieveInputData();
        }

        protected override void OnAutomationMessageReceived(NotificationMessageUI<PositioningMessageData> message)
        {
            if (message.Status == MessageStatus.OperationEnd
                ||
                message.Status == MessageStatus.OperationStop) // TODO why OperationStop as well and not only OperationEnd?
            {
                this.IsExecutingProcedure = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.acceptCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteAcceptCommand()
        {
            return !this.IsExecutingProcedure
               && !this.IsWaitingForResponse
               && string.IsNullOrWhiteSpace(this.Error);
        }

        private async Task AcceptAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.IsExecutingProcedure = true;

                await this.ResolutionCalibrationService.CompleteAsync(this.NewResolution.Value);

                this.NavigationService.GoBack();
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

                this.NewResolution = await this.ResolutionCalibrationService
                    .GetAdjustedResolutionAsync(
                        this.MeasuredDistance.Value,
                        exepectedDistance,
                        cancellationToken);

                this.IsRetrievingNewResolution = false;
            }
            catch
            {
                this.NewResolution = null;
            }
        }

        #endregion
    }
}
