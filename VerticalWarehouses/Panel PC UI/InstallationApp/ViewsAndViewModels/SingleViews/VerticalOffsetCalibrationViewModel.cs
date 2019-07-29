using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class VerticalOffsetCalibrationViewModel : BindableBase, IVerticalOffsetCalibrationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand acceptOffsetButtonCommand;

        private IUnityContainer container;

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

        private IOffsetCalibrationMachineService offsetCalibrationService;

        private SubscriptionToken receivePositioningUpdateToken;

        private string referenceCellHeight;

        private string referenceCellNumber;

        private ICommand setPositionButtonCommand;

        private ICommand stepDownButtonCommand;

        private ICommand stepUpButtonCommand;

        private string stepValue;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
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

        public string ReferenceCellNumber { get => this.referenceCellNumber; set => this.SetProperty(ref this.referenceCellNumber, value); }

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

                //TEMP temporary commented because there is not a cell map
                //this.referenceCellHeight = (await this.offsetCalibrationService.GetLoadingUnitPositionParameterAsync(Category, "CellReference")).ToString();
                this.stepValue = (await this.offsetCalibrationService.GetDecimalConfigurationParameterAsync(Category, "StepValue")).ToString();
            }
            catch (SwaggerException)
            {
                this.NoteString = VW.App.Resources.InstallationApp.ErrorRetrievingConfigurationData;
            }
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.offsetCalibrationService = container.Resolve<IOffsetCalibrationMachineService>();
            this.container = container;
            this.offsetCalibrationService = this.container.Resolve<IOffsetCalibrationMachineService>();
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
        }

        public void PositioningDone(bool result)
        {
            // TODO implement missing feature
        }

        public async Task SetPositionButtonCommandMethod()
        {
            try
            {
                await this.offsetCalibrationService.ExecutePositioningAsync();
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
