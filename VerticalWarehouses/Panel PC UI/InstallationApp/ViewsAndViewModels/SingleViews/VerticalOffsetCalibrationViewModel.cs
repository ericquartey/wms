using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class VerticalOffsetCalibrationViewModel : BindableBase, IVerticalOffsetCalibrationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IOffsetCalibrationMachineService offsetCalibrationService;

        private ICommand acceptOffsetButtonCommand;

        private string correctOffset;

        private ICommand correctOffsetButtonCommand;

        private string currentHeight;

        private decimal currentOffset;

        private ICommand exitFromViewCommand;

        private bool isAcceptOffsetButtonActive = true;

        private bool isCorrectOffsetButtonActive;

        private bool isSetPositionButtonActive = true;

        private bool isStepDownButtonActive = true;

        private bool isStepUpButtonActive = true;

        private string noteString = VW.App.Resources.InstallationApp.VerticalOffsetCalibration;

        private SubscriptionToken receivePositioningUpdateToken;

        private string referenceCellHeight;

        private string referenceCellNumber;

        private ICommand setPositionButtonCommand;

        private ICommand stepDownButtonCommand;

        private ICommand stepUpButtonCommand;

        private string stepValue;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationViewModel(
            IEventAggregator eventAggregator,
            IOffsetCalibrationMachineService offsetCalibrationService)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (offsetCalibrationService == null)
            {
                throw new ArgumentNullException(nameof(offsetCalibrationService));
            }

            this.eventAggregator = eventAggregator;
            this.offsetCalibrationService = offsetCalibrationService;

            this.NoteString = VW.App.Resources.InstallationApp.VerticalOffsetCalibration;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand AcceptOffsetButtonCommand => this.acceptOffsetButtonCommand ?? (
            this.acceptOffsetButtonCommand = new DelegateCommand(async () => await this.AcceptOffsetButtonCommandMethodAsync()));

        public string CorrectOffset { get => this.correctOffset; set => this.SetProperty(ref this.correctOffset, value); }

        public ICommand CorrectOffsetButtonCommand => this.correctOffsetButtonCommand ?? (
            this.correctOffsetButtonCommand = new DelegateCommand(async () => await this.CorrectOffsetButtonCommandMethodAsync()));

        public string CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public decimal CurrentOffset { get => this.currentOffset; set => this.SetProperty(ref this.currentOffset, value); }

        public ICommand ExitFromViewCommand => this.exitFromViewCommand ?? (this.exitFromViewCommand = new DelegateCommand(this.ExitFromViewMethod));

        public bool IsAcceptOffsetButtonActive { get => this.isAcceptOffsetButtonActive; set => this.SetProperty(ref this.isAcceptOffsetButtonActive, value); }

        public bool IsCorrectOffsetButtonActive { get => this.isCorrectOffsetButtonActive; set => this.SetProperty(ref this.isCorrectOffsetButtonActive, value); }

        public bool IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }

        public bool IsStepDownButtonActive { get => this.isStepDownButtonActive; set => this.SetProperty(ref this.isStepDownButtonActive, value); }

        public bool IsStepUpButtonActive { get => this.isStepUpButtonActive; set => this.SetProperty(ref this.isStepUpButtonActive, value); }

        public BindableBase NavigationViewModel { get; set; }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public string ReferenceCellHeight { get => this.referenceCellHeight; set => this.SetProperty(ref this.referenceCellHeight, value); }

        public string ReferenceCellNumber
        {
            get => this.referenceCellNumber;
            set
            {
                this.SetProperty(ref this.referenceCellNumber, value);
                this.CheckReferenceCellNumberCorrectness(value);
                this.TriggerSearchAsync().GetAwaiter();
            }
        }

        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(async () => await this.SetPositionButtonCommandMethod()));

        public ICommand StepDownButtonCommand => this.stepDownButtonCommand ?? (this.stepDownButtonCommand = new DelegateCommand(this.StepDownButtonCommandMethod));

        public ICommand StepUpButtonCommand => this.stepUpButtonCommand ?? (this.stepUpButtonCommand = new DelegateCommand(this.StepUpButtonCommandMethod));

        public string StepValue { get => this.stepValue; set => this.SetProperty(ref this.stepValue, value); }

        #endregion

        #region Methods

        public async Task AcceptOffsetButtonCommandMethodAsync()
        {
            if (decimal.TryParse(this.CorrectOffset, out var decCorrectOffset))
            {
                var result = await this.offsetCalibrationService.SetOffsetParameterAsync(decCorrectOffset);

                if (result)
                {
                    this.IsAcceptOffsetButtonActive = false;
                    this.IsCorrectOffsetButtonActive = true;
                    this.IsStepDownButtonActive = false;
                    this.IsStepUpButtonActive = false;
                    this.CurrentOffset = decCorrectOffset;
                }
            }
        }

        public void CheckReferenceCellNumberCorrectness(string input)
        {
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out var i) && i > 0)
            {
                this.IsSetPositionButtonActive = true;
            }
            else
            {
                this.IsSetPositionButtonActive = false;
                this.ReferenceCellHeight = string.Empty;
            }
        }

        public async Task CorrectOffsetButtonCommandMethodAsync()
        {
            var result = await this.offsetCalibrationService.ExecuteCompletedAsync();

            if (result)
            {
                this.IsCorrectOffsetButtonActive = false;
            }
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task GetParameterValuesAsync()
        {
            try
            {
                const string Category = "OffsetCalibration";
                this.referenceCellNumber = (await this.offsetCalibrationService.GetIntegerConfigurationParameterAsync(Category, "ReferenceCell")).ToString();
                this.stepValue = (await this.offsetCalibrationService.GetDecimalConfigurationParameterAsync(Category, "StepValue")).ToString();
            }
            catch (SwaggerException)
            {
                this.NoteString = VW.App.Resources.InstallationApp.ErrorRetrievingConfigurationData;
            }
        }

        public async Task OnEnterViewAsync()
        {
            await this.GetParameterValuesAsync();

            this.receivePositioningUpdateToken = this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.tokenSource = new CancellationTokenSource();
            await this.ReferenceCellNumberAsync(this.referenceCellNumber);
        }

        public void PositioningDone(bool result)
        {
            // TODO implement missing feature
        }

        public async Task ReferenceCellNumberAsync(string referenceCellNumber)
        {
            this.ReferenceCellHeight = await this.offsetCalibrationService.GetLoadingUnitPositionParameterAsync(referenceCellNumber);
        }

        public async Task SetPositionButtonCommandMethod()
        {
            try
            {
                await this.offsetCalibrationService.ExecutePositioningAsync(this.referenceCellHeight);
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http request.";
                throw; // TEMP Define a better throw exception
            }
        }

        public void StepDownButtonCommandMethod()
        {
            // TODO implement missing feature
        }

        public void StepUpButtonCommandMethod()
        {
            // TODO implement missing feature
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>().Unsubscribe(this.receivePositioningUpdateToken);
        }

        private async Task TriggerSearchAsync()
        {
            this.tokenSource?.Cancel(false);

            this.tokenSource = new CancellationTokenSource();

            try
            {
                const int callDelayMilliseconds = 500;

                await Task.Delay(callDelayMilliseconds, this.tokenSource.Token)
                    .ContinueWith(
                        async t => await this.ReferenceCellNumberAsync(this.referenceCellNumber),
                        this.tokenSource.Token,
                        TaskContinuationOptions.NotOnCanceled,
                        TaskScheduler.Current);
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        private void UpdateCurrentActionStatus(MessageNotifiedEventArgs messageUI)
        {
            if (messageUI.NotificationMessage is NotificationMessageUI<PositioningMessageData> p)
            {
                switch (p.Status)
                {
                    case MessageStatus.OperationStart:
                        this.NoteString = VW.App.Resources.InstallationApp.GoToInitialPosition;
                        break;

                    case MessageStatus.OperationEnd:
                        this.NoteString = VW.App.Resources.InstallationApp.GoToInitialPosition;
                        break;

                    case MessageStatus.OperationError:
                        this.NoteString = VW.App.Resources.InstallationApp.Error;
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
