using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using MessageStatus = Ferretto.VW.CommonUtils.Messages.Enumerations.MessageStatus;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class ResolutionCalibrationVerticalAxisViewModel : BindableBase, IResolutionCalibrationVerticalAxisViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineResolutionCalibrationProcedureService resolutionCalibrationService;

        private readonly IStatusMessageService statusMessageService;

        private readonly IMachineTestService testService;

        private ICommand acceptButtonCommand;

        private ICommand cancelButtonCommand;

        private ICommand closeProcedureButtonCommand;

        private decimal? currentResolution;

        private decimal? desiredFinalPosition;

        private decimal desiredInitialPosition;

        private ICommand initialPositionButtonCommand;

        private bool isAcceptButtonActive;

        private bool isCloseProcedureButtonActive;

        private bool isDesiredFinalPositionActive;

        private bool isDesiredFinalPositionHighlighted;

        private bool isDesiredInitialPositionActive = true;

        private bool isDesiredInitialPositionHighlighted = true;

        private bool isGoToInitialPositionButtonActive;

        private bool isMesuredMovementActive;

        private bool isMesuredMovementHighlighted;

        private bool isMoveButtonActive;

        private bool isReadFinalPositionActive;

        private bool isReadFinalPositionHighlighted;

        private bool isReadInitialPositionActive;

        private bool isReadInitialPositionHighlighted;

        private bool isSetPositionButtonActive;

        private bool isUpdateResolutionButtonActive;

        private decimal? mesuredMovement;

        private ICommand moveButtonCommand;

        private decimal? newResolution;

        private decimal? readFinalPosition;

        private decimal? readInitialPosition;

        private SubscriptionToken receivedActionToken;

        private string repositionLenght;

        private ResolutionCalibrationStep resolutionCalibrationStep;

        private ICommand setPositionButtonCommand;

        private ICommand updateResolutionCommand;

        #endregion

        #region Constructors

        public ResolutionCalibrationVerticalAxisViewModel(
            IEventAggregator eventAggregator,
            IMachineResolutionCalibrationProcedureService resolutionCalibrationService,
            IMachineTestService testService,
            IStatusMessageService statusMessageService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (resolutionCalibrationService == null)
            {
                throw new System.ArgumentNullException(nameof(resolutionCalibrationService));
            }

            if (testService == null)
            {
                throw new System.ArgumentNullException(nameof(testService));
            }

            if (statusMessageService == null)
            {
                throw new System.ArgumentNullException(nameof(statusMessageService));
            }

            this.eventAggregator = eventAggregator;
            this.resolutionCalibrationService = resolutionCalibrationService;
            this.testService = testService;
            this.statusMessageService = statusMessageService;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand AcceptButtonCommand =>
            this.acceptButtonCommand
            ??
            (this.acceptButtonCommand = new DelegateCommand(async () => await this.AcceptButtonMethodAsync()));

        public ICommand CancelButtonCommand =>
            this.cancelButtonCommand
            ??
            (this.cancelButtonCommand = new DelegateCommand(() => this.CancelButtonMethod()));

        public ICommand CloseProcedureButtonCommand =>
            this.closeProcedureButtonCommand
            ??
            (this.closeProcedureButtonCommand = new DelegateCommand(
                async () => await this.CloseProcedureButtonMethodAsync()));

        public decimal? CurrentResolution
        {
            get => this.currentResolution;
            set => this.SetProperty(ref this.currentResolution, value);
        }

        public decimal? DesiredFinalPosition
        {
            get => this.desiredFinalPosition;
            set
            {
                this.SetProperty(ref this.desiredFinalPosition, value);
                this.CheckDesiredFinalPositionCorrectness(value);
            }
        }

        public decimal DesiredInitialPosition
        {
            get => this.desiredInitialPosition;
            set
            {
                this.SetProperty(ref this.desiredInitialPosition, value);
                this.CheckDesiredInitialPositionCorrectness(value);
            }
        }

        public ICommand InitialPositionButtonCommand =>
            this.initialPositionButtonCommand
            ??
            (this.initialPositionButtonCommand = new DelegateCommand(
                async () => await this.SetPositionButtonMethodAsync(ResolutionCalibrationStep.InitialPosition)));

        public bool IsAcceptButtonActive { get => this.isAcceptButtonActive; set => this.SetProperty(ref this.isAcceptButtonActive, value); }

        public bool IsCloseProcedureButtonActive { get => this.isCloseProcedureButtonActive; set => this.SetProperty(ref this.isCloseProcedureButtonActive, value); }

        public bool IsDesiredFinalPositionActive { get => this.isDesiredFinalPositionActive; set => this.SetProperty(ref this.isDesiredFinalPositionActive, value); }

        public bool IsDesiredFinalPositionHighlighted { get => this.isDesiredFinalPositionHighlighted; set => this.SetProperty(ref this.isDesiredFinalPositionHighlighted, value); }

        public bool IsDesiredInitialPositionActive { get => this.isDesiredInitialPositionActive; set => this.SetProperty(ref this.isDesiredInitialPositionActive, value); }

        public bool IsDesiredInitialPositionHighlighted { get => this.isDesiredInitialPositionHighlighted; set => this.SetProperty(ref this.isDesiredInitialPositionHighlighted, value); }

        public bool IsGoToInitialPositionButtonActive { get => this.isGoToInitialPositionButtonActive; set => this.SetProperty(ref this.isGoToInitialPositionButtonActive, value); }

        public bool IsMesuredInitialPositionHighlighted { get => this.isReadInitialPositionHighlighted; set => this.SetProperty(ref this.isReadInitialPositionHighlighted, value); }

        public bool IsMesuredMovementActive { get => this.isMesuredMovementActive; set => this.SetProperty(ref this.isMesuredMovementActive, value); }

        public bool IsMesuredMovementHighlighted { get => this.isMesuredMovementHighlighted; set => this.SetProperty(ref this.isMesuredMovementHighlighted, value); }

        public bool IsMoveButtonActive { get => this.isMoveButtonActive; set => this.SetProperty(ref this.isMoveButtonActive, value); }

        public bool IsReadFinalPositionActive { get => this.isReadFinalPositionActive; set => this.SetProperty(ref this.isReadFinalPositionActive, value); }

        public bool IsReadFinalPositionHighlighted { get => this.isReadFinalPositionHighlighted; set => this.SetProperty(ref this.isReadFinalPositionHighlighted, value); }

        public bool IsReadInitialPositionActive
        {
            get => this.isReadInitialPositionActive;
            set => this.SetProperty(ref this.isReadInitialPositionActive, value);
        }

        public bool IsSetPositionButtonActive
        {
            get => this.isSetPositionButtonActive;
            set => this.SetProperty(ref this.isSetPositionButtonActive, value);
        }

        public bool IsUpdateResolutionButtonActive
        {
            get => this.isUpdateResolutionButtonActive;
            set => this.SetProperty(ref this.isUpdateResolutionButtonActive, value);
        }

        public decimal? MesuredInitialPosition
        {
            get => this.readInitialPosition;
            set
            {
                this.SetProperty(ref this.readInitialPosition, value);
                this.CheckMesuredInitialPositionCorrectness(value);
            }
        }

        public decimal? MesuredMovement
        {
            get => this.mesuredMovement;
            set
            {
                this.SetProperty(ref this.mesuredMovement, value);
                this.CheckMesuredRepositionLenghtCorrectness(value);
            }
        }

        public ICommand MoveButtonCommand =>
            this.moveButtonCommand
            ??
            (this.moveButtonCommand = new DelegateCommand(
                async () => await this.MoveButtonMethodAsync()));

        public BindableBase NavigationViewModel { get; set; }

        public decimal? NewResolution
        {
            get => this.newResolution;
            set => this.SetProperty(ref this.newResolution, value);
        }

        public decimal? ReadFinalPosition
        {
            get => this.readFinalPosition;
            set
            {
                this.SetProperty(ref this.readFinalPosition, value);
                this.CheckMesuredFinalPositionCorrectness(value);
            }
        }

        public decimal? ReadInitialPosition
        {
            get => this.readInitialPosition;
            set
            {
                this.SetProperty(ref this.readInitialPosition, value);
                this.CheckMesuredInitialPositionCorrectness(value);
            }
        }

        public string RepositionLenght
        {
            get => this.repositionLenght;
            set => this.SetProperty(ref this.repositionLenght, value);
        }

        public decimal? Resolution
        {
            get => this.currentResolution;
            set => this.SetProperty(ref this.currentResolution, value);
        }

        public ICommand SetPositionButtonCommand =>
            this.setPositionButtonCommand
            ?? (this.setPositionButtonCommand = new DelegateCommand(
                async () => await this.SetPositionButtonMethodAsync(ResolutionCalibrationStep.StartProcedure)));

        public ICommand UpdateResolutionCommand =>
            this.updateResolutionCommand
            ??
            (this.updateResolutionCommand = new DelegateCommand(
                async () => await this.UpdateResolutionMethodAsync()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO implement feature
        }

        public async Task GetParameterValuesAsync()
        {
            try
            {
                this.Resolution = await this.resolutionCalibrationService.GetDecimalConfigurationParameterAsync("VerticalAxis", "Resolution");
                this.DesiredInitialPosition = await this.resolutionCalibrationService.GetDecimalConfigurationParameterAsync("ResolutionCalibration", "InitialPosition");
                this.DesiredFinalPosition = await this.resolutionCalibrationService.GetDecimalConfigurationParameterAsync("ResolutionCalibration", "FinalPosition");
            }
            catch (SwaggerException)
            {
                // TODO
            }
        }

        public async Task OnEnterViewAsync()
        {
            this.receivedActionToken = this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>()
                .Subscribe(
                message =>
                {
                    this.UpdateResolution(new MessageNotifiedEventArgs(message));
                },
                ThreadOption.PublisherThread,
                false);

            this.CancelButtonMethod();
            await this.GetParameterValuesAsync();
            this.resolutionCalibrationStep = ResolutionCalibrationStep.None;
        }

        public void PositioningDone(bool result)
        {
            // TODO implement feature
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<PositioningMessageData>>().Unsubscribe(this.receivedActionToken);
        }

        private async Task AcceptButtonMethodAsync()
        {
            await this.resolutionCalibrationService.SetResolutionParameterAsync(this.NewResolution.Value);

            this.IsAcceptButtonActive = false;
            this.IsUpdateResolutionButtonActive = false;
            this.IsGoToInitialPositionButtonActive = true;
            this.CurrentResolution = this.NewResolution;
        }

        private void CancelButtonMethod()
        {
            this.RepositionLenght = string.Empty;
            this.MesuredMovement = null;
            this.DesiredFinalPosition = null;
            this.NewResolution = null;
            this.ReadInitialPosition = null;
            this.ReadFinalPosition = null;
            this.IsAcceptButtonActive = false;
            this.IsMesuredInitialPositionHighlighted = false;
            this.IsReadFinalPositionHighlighted = false;
            this.IsReadInitialPositionActive = false;
            this.IsReadFinalPositionActive = false;
            this.IsMesuredMovementActive = false;
            this.IsMoveButtonActive = false;
            this.IsUpdateResolutionButtonActive = false;
            this.IsSetPositionButtonActive = false;
            this.IsGoToInitialPositionButtonActive = false;
            this.IsCloseProcedureButtonActive = false;
        }

        private void CheckDesiredFinalPositionCorrectness(decimal? input)
        {
            if (input.HasValue && input > 0)
            {
                this.IsDesiredFinalPositionHighlighted = false;
                this.statusMessageService.Clear();

                this.IsMoveButtonActive = this.ReadInitialPosition > 0;
            }
            else
            {
                this.IsMoveButtonActive = false;
                this.IsDesiredFinalPositionHighlighted = true;
                this.statusMessageService.Notify(VW.App.Resources.InstallationApp.ValueNotValid);
            }
        }

        private void CheckDesiredInitialPositionCorrectness(decimal input)
        {
            if (input > 0)
            {
                this.IsSetPositionButtonActive = true;
                this.IsDesiredInitialPositionHighlighted = false;
                this.statusMessageService.Clear();
            }
            else
            {
                this.IsSetPositionButtonActive = false;
                this.IsDesiredInitialPositionHighlighted = true;
                this.statusMessageService.Notify(VW.App.Resources.InstallationApp.ValueNotValid);
            }
        }

        private void CheckMesuredFinalPositionCorrectness(decimal? input)
        {
            if (input.HasValue && input > 0)
            {
                this.IsAcceptButtonActive = true;

                this.IsUpdateResolutionButtonActive = true;
                this.IsReadFinalPositionHighlighted = false;
                this.statusMessageService.Clear();
            }
            else
            {
                this.IsReadFinalPositionHighlighted = true;
                this.statusMessageService.Notify(VW.App.Resources.InstallationApp.ValueNotValid);
            }
        }

        private void CheckMesuredInitialPositionCorrectness(decimal? input)
        {
            if (input.HasValue && input > 0)
            {
                this.IsMoveButtonActive = true;
                this.IsMesuredInitialPositionHighlighted = false;
                this.IsDesiredFinalPositionActive = true;
            }
            else
            {
                this.IsMoveButtonActive = false;
                this.IsMesuredInitialPositionHighlighted = true;
                this.IsDesiredFinalPositionActive = false;
                this.statusMessageService.Notify(VW.App.Resources.InstallationApp.ValueNotValid);
            }
        }

        private void CheckMesuredRepositionLenghtCorrectness(decimal? input)
        {
            this.IsMesuredMovementHighlighted = input.HasValue && input.Value > 0;
        }

        private async Task CloseProcedureButtonMethodAsync()
        {
            await this.resolutionCalibrationService.MarkAsCompletedAsync();

            this.IsCloseProcedureButtonActive = false;
        }

        private async Task MoveButtonMethodAsync()
        {
            this.resolutionCalibrationStep = ResolutionCalibrationStep.Move;

            try
            {
                await this.resolutionCalibrationService.StartAsync(this.DesiredFinalPosition.Value, ResolutionCalibrationStep.Move);
            }
            catch (SwaggerException)
            {
                // do nothing
            }
        }

        private async Task SetPositionButtonMethodAsync(ResolutionCalibrationStep resolutionCalibrationSteps)
        {
            this.resolutionCalibrationStep = resolutionCalibrationSteps;

            try
            {
                await this.resolutionCalibrationService.StartAsync(this.DesiredInitialPosition, resolutionCalibrationSteps);
            }
            catch (SwaggerException)
            {
                // do nothing
            }
        }

        private void UpdateResolution(MessageNotifiedEventArgs messageUI)
        {
            if (messageUI.NotificationMessage is NotificationMessageUI<PositioningMessageData> cp)
            {
                if ((cp.Status == MessageStatus.OperationEnd || cp.Status == MessageStatus.OperationStop) && this.resolutionCalibrationStep == ResolutionCalibrationStep.StartProcedure)
                {
                    this.IsSetPositionButtonActive = false;
                    this.IsReadInitialPositionActive = true;
                }
                if ((cp.Status == MessageStatus.OperationEnd || cp.Status == MessageStatus.OperationStop) && this.resolutionCalibrationStep == ResolutionCalibrationStep.Move)
                {
                    this.IsReadFinalPositionActive = true;
                    this.IsMoveButtonActive = false;
                }
                if ((cp.Status == MessageStatus.OperationEnd || cp.Status == MessageStatus.OperationStop) && this.resolutionCalibrationStep == ResolutionCalibrationStep.InitialPosition)
                {
                    this.IsGoToInitialPositionButtonActive = false;
                    this.IsCloseProcedureButtonActive = true;
                }
            }
        }

        private async Task UpdateResolutionMethodAsync()
        {
            decimal readDistance;

            readDistance = this.ReadFinalPosition.Value - this.ReadInitialPosition.Value;
            this.MesuredMovement = readDistance;
            this.IsMesuredMovementActive = true;

            this.NewResolution = await this.resolutionCalibrationService
                .GetComputedResolutionAsync(readDistance, this.desiredInitialPosition, this.desiredFinalPosition.Value, this.Resolution.Value);

            this.IsAcceptButtonActive = this.NewResolution > 0;
        }

        #endregion
    }
}
