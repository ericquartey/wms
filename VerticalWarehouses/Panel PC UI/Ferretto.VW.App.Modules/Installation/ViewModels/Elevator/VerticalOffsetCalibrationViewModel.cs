using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class VerticalOffsetCalibrationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineVerticalOffsetProcedureService verticalOffsetService;

        private ICommand acceptOffsetCommand;

        private string correctOffset;

        private ICommand correctOffsetCommand;

        private string currentHeight;

        private decimal currentOffset;

        private decimal inputStepValue;

        private string noteString = VW.App.Resources.InstallationApp.VerticalOffsetCalibration;

        private SubscriptionToken receivePositioningUpdateToken;

        private decimal? referenceCellHeight;

        private int referenceCellNumber;

        private ICommand setPositionCommand;

        private ICommand stepDownCommand;

        private ICommand stepUpCommand;

        private CancellationTokenSource tokenSource;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationViewModel(IMachineVerticalOffsetProcedureService verticalOffsetService)
            : base(Services.PresentationMode.Installer)
        {
            if (verticalOffsetService is null)
            {
                throw new ArgumentNullException(nameof(verticalOffsetService));
            }

            this.verticalOffsetService = verticalOffsetService;
        }

        #endregion

        #region Properties

        public ICommand AcceptOffsetButtonCommand =>
            this.acceptOffsetCommand
            ??
            (this.acceptOffsetCommand = new DelegateCommand(async () => await this.AcceptOffsetButtonCommandMethodAsync()));

        public string CorrectOffset
        {
            get => this.correctOffset;
            set => this.SetProperty(ref this.correctOffset, value);
        }

        public ICommand CorrectOffsetButtonCommand =>
            this.correctOffsetCommand
            ??
            (this.correctOffsetCommand = new DelegateCommand(async () => await this.CorrectOffsetButtonCommandMethodAsync()));

        public string CurrentHeight
        {
            get => this.currentHeight;
            set => this.SetProperty(ref this.currentHeight, value);
        }

        public decimal CurrentOffset
        {
            get => this.currentOffset;
            set => this.SetProperty(ref this.currentOffset, value);
        }

        public decimal InputStepValue
        {
            get => this.inputStepValue;
            set => this.SetProperty(ref this.inputStepValue, value);
        }

        public string NoteString
        {
            get => this.noteString;
            set => this.SetProperty(ref this.noteString, value);
        }

        public decimal? ReferenceCellHeight
        {
            get => this.referenceCellHeight;
            set => this.SetProperty(ref this.referenceCellHeight, value);
        }

        public int ReferenceCellNumber
        {
            get => this.referenceCellNumber;
            set
            {
                this.SetProperty(ref this.referenceCellNumber, value);
                this.CheckReferenceCellNumberCorrectness(value);
                this.TriggerSearchAsync().GetAwaiter();
            }
        }

        public ICommand SetPositionButtonCommand =>
            this.setPositionCommand
            ??
            (this.setPositionCommand = new DelegateCommand(async () => await this.ExecuteSetPositionCommand()));

        public ICommand StepDownButtonCommand =>
            this.stepDownCommand
            ??
            (this.stepDownCommand = new DelegateCommand(this.ExecuteStepDownCommand));

        public ICommand StepUpButtonCommand =>
            this.stepUpCommand
            ??
            (this.stepUpCommand = new DelegateCommand(this.ExecuteStepUpCommand));

        #endregion

        #region Methods

        public async Task AcceptOffsetButtonCommandMethodAsync()
        {
            if (decimal.TryParse(this.CorrectOffset, out var offset))
            {
                try
                {
                    await this.verticalOffsetService.SetAsync(offset);

                    this.CurrentOffset = offset;
                }
                catch (Exception ex)
                {
                    this.ShowNotification(ex);
                }
            }
        }

        public void CheckReferenceCellNumberCorrectness(int input)
        {
            if (input > 0)
            {
                // this.IsSetPositionButtonActive = true;
            }
            else
            {
                // this.IsSetPositionButtonActive = false;
                this.ReferenceCellHeight = null;
            }
        }

        public async Task CorrectOffsetButtonCommandMethodAsync()
        {
            try
            {
                await this.verticalOffsetService.MarkAsCompletedAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override void Disappear()
        {
            if (this.receivePositioningUpdateToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<PositioningMessageData>>()
                    .Unsubscribe(this.receivePositioningUpdateToken);
            }

            base.Disappear();
        }

        public async Task ExecuteSetPositionCommand()
        {
            System.Diagnostics.Debug.Assert(this.ReferenceCellHeight.HasValue);

            try
            {
                await this.verticalOffsetService.StartAsync(this.ReferenceCellHeight.Value);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public void ExecuteStepDownCommand()
        {
            throw new NotImplementedException();
        }

        public void ExecuteStepUpCommand()
        {
            throw new NotImplementedException();
        }

        public async Task GetParameterValuesAsync()
        {
            try
            {
                const string category = "OffsetCalibration";

                this.referenceCellNumber = await this.verticalOffsetService
                    .GetIntegerConfigurationParameterAsync(category, "ReferenceCell");

                this.inputStepValue = await this.verticalOffsetService
                    .GetDecimalConfigurationParameterAsync(category, "StepValue");
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public async Task LoadReferenceCellHeightAsync(int cellNumber)
        {
            this.ReferenceCellHeight = await this.verticalOffsetService
                .GetLoadingUnitPositionParameterAsync(cellNumber);
        }

        public async Task OnEnterViewAsync()
        {
            await this.GetParameterValuesAsync();

            this.receivePositioningUpdateToken = this.EventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateCurrentActionStatus(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.tokenSource = new CancellationTokenSource();

            await this.LoadReferenceCellHeightAsync(this.referenceCellNumber);
        }

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.NoteString = VW.App.Resources.InstallationApp.VerticalOffsetCalibration;

            this.IsBackNavigationAllowed = true;
        }

        public void PositioningDone(bool result)
        {
            throw new NotImplementedException();
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
                        async t => await this.LoadReferenceCellHeightAsync(this.referenceCellNumber),
                        this.tokenSource.Token,
                        TaskContinuationOptions.NotOnCanceled,
                        TaskScheduler.Current);
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        private void UpdateCurrentActionStatus(MessageNotifiedEventArgs message)
        {
            if (message.NotificationMessage is NotificationMessageUI<PositioningMessageData> p)
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
                }
            }
        }

        #endregion
    }
}
