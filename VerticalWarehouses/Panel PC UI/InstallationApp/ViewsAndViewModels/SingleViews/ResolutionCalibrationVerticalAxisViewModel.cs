using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;
using MessageStatus = Ferretto.VW.Common_Utils.Messages.Enumerations.MessageStatus;

namespace Ferretto.VW.InstallationApp
{
    public class ResolutionCalibrationVerticalAxisViewModel : BindableBase, IResolutionCalibrationVerticalAxisViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand acceptButtonCommand;

        private ICommand cancelButtonCommand;

        private string currentResolution;

        private string desideredFinalPosition;

        private string desiredInitialPosition;

        private IInstallationService installationService;

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

        private string mesuredMovement;

        private ICommand moveButtonCommand;

        private string newResolution;

        private string readFinalPosition;

        private string readInitialPosition;

        private SubscriptionToken receivedActionToken;

        private string repositionLenght;

        private ICommand setPositionButtonCommand;

        private ITestService testService;

        #endregion

        #region Constructors

        public ResolutionCalibrationVerticalAxisViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand AcceptButtonCommand => this.acceptButtonCommand ?? (this.acceptButtonCommand = new DelegateCommand(() => this.AcceptButtonMethod()));

        public ICommand CancelButtonCommand => this.cancelButtonCommand ?? (this.cancelButtonCommand = new DelegateCommand(() => this.CancelButtonMethod()));

        public string CurrentResolution { get => this.currentResolution; set => this.SetProperty(ref this.currentResolution, value); }

        public string DesideredFinalPosition
        {
            get => this.desideredFinalPosition;
            set
            {
                this.SetProperty(ref this.desideredFinalPosition, value);
                this.CheckMesuredInitialPositionCorrectness(value);
            }
        }

        public string DesiredInitialPosition
        {
            get => this.desiredInitialPosition;
            set
            {
                this.SetProperty(ref this.desiredInitialPosition, value);
                this.CheckDesideredInitialPositionCorrectness(value);
            }
        }

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

        public bool IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }

        public string MesuredInitialPosition
        {
            get => this.readInitialPosition;
            set
            {
                this.SetProperty(ref this.readInitialPosition, value);
                this.CheckMesuredInitialPositionCorrectness(value);
            }
        }

        public string MesuredMovement
        {
            get => this.mesuredMovement;
            set
            {
                this.SetProperty(ref this.mesuredMovement, value);
                this.CheckMesuredRepositionLenghtCorrectness(value);
            }
        }

        public ICommand MoveButtonCommand => this.moveButtonCommand ?? (this.moveButtonCommand = new DelegateCommand(() => this.MoveButtonMethod()));

        public BindableBase NavigationViewModel { get; set; }

        public string NewResolution { get => this.newResolution; set => this.SetProperty(ref this.newResolution, value); }

        public string ReadFinalPosition { get => this.readFinalPosition; set => this.SetProperty(ref this.readFinalPosition, value); }

        public string ReadInitialPosition
        {
            get => this.readInitialPosition;
            set
            {
                this.SetProperty(ref this.readInitialPosition, value);
                this.CheckMesuredInitialPositionCorrectness(value);
            }
        }

        public string RepositionLenght { get => this.repositionLenght; set => this.SetProperty(ref this.repositionLenght, value); }

        public string Resolution
        {
            get => this.currentResolution;
            set => this.SetProperty(ref this.currentResolution, value);
        }

        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(async () => await this.SetPositionButtonMethodAsync()));

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
                this.Resolution = (await this.installationService.GetDecimalConfigurationParameterAsync("VerticalAxis", "Resolution")).ToString();
                this.DesiredInitialPosition = (await this.installationService.GetDecimalConfigurationParameterAsync("ResolutionCalibration", "InitialPosition")).ToString();
                this.DesideredFinalPosition = (await this.installationService.GetDecimalConfigurationParameterAsync("ResolutionCalibration", "FinalPosition")).ToString();
            }
            catch (SwaggerException)
            {
                // TODO
            }
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.installationService = container.Resolve<IInstallationService>();
            this.testService = container.Resolve<ITestService>();
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
        }

        public void PositioningDone(bool result)
        {
            // TODO implement feature
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<ResolutionCalibrationMessageData>>().Unsubscribe(this.receivedActionToken);
        }

        private void AcceptButtonMethod()
        {
            // TODO implement feature
        }

        private void CalculateNewResolutionMethod()
        {
            decimal.TryParse(this.CurrentResolution, out var cr);
            decimal.TryParse(this.MesuredMovement, out var ml);
            decimal.TryParse(this.RepositionLenght, out var rl);
            this.NewResolution = ((cr * ml) / rl).ToString("##.##");
            this.IsAcceptButtonActive = true;
        }

        private void CancelButtonMethod()
        {
            this.RepositionLenght = string.Empty;
            this.MesuredMovement = string.Empty;
            this.DesideredFinalPosition = string.Empty;
            this.NewResolution = string.Empty;
            this.IsAcceptButtonActive = false;
            this.IsMesuredInitialPositionHighlighted = false;
            this.IsReadInitialPositionActive = false;
            this.IsMesuredMovementActive = false;
            this.IsMoveButtonActive = false;
            this.IsSetPositionButtonActive = false;
        }

        private void CheckDesideredInitialPositionCorrectness(string input)
        {
            if (!string.IsNullOrEmpty(input) && decimal.TryParse(input, out var i) && i > 0)
            {
                this.IsSetPositionButtonActive = true;
                this.IsDesiredInitialPositionHighlighted = false;
            }
            else
            {
                this.IsSetPositionButtonActive = false;
                this.IsDesiredInitialPositionHighlighted = true;
            }
        }

        private void CheckMesuredInitialPositionCorrectness(string input)
        {
            if (!string.IsNullOrEmpty(input) && decimal.TryParse(input, out var i) && i > 0)
            {
                this.IsMoveButtonActive = true;
                this.IsReadInitialPositionActive = false;
                this.IsMesuredInitialPositionHighlighted = false;
                this.IsReadInitialPositionActive = true;
                this.IsMesuredInitialPositionHighlighted = true;
                this.IsDesiredFinalPositionActive = true;
                this.IsSetPositionButtonActive = false;
            }
            else
            {
                this.IsMoveButtonActive = false;
                this.IsReadInitialPositionActive = true;
                this.IsMesuredInitialPositionHighlighted = true;
                this.IsReadInitialPositionActive = false;
                this.IsMesuredInitialPositionHighlighted = false;
                this.IsDesiredFinalPositionActive = false;
                this.IsSetPositionButtonActive = true;
            }
        }

        private void CheckMesuredRepositionLenghtCorrectness(string input)
        {
            if (!string.IsNullOrEmpty(input) && decimal.TryParse(input, out var i) && i > 0)
            {
                this.IsMesuredMovementHighlighted = false;
                this.IsMesuredMovementActive = false;
                this.CalculateNewResolutionMethod();
            }
        }

        private void MoveButtonMethod()
        {
            // TODO implement feature
        }

        private async Task SetPositionButtonMethodAsync()
        {
            decimal.TryParse(this.DesiredInitialPosition, out var position);
            await this.installationService.ExecuteResolutionAsync(position, ResolutionCalibrationSteps.StartProcedure);
        }

        private void UpdateResolution(MessageNotifiedEventArgs messageUI)
        {
            if (messageUI.NotificationMessage is NotificationMessageUI<PositioningMessageData> cp)
            {
                Enum.TryParse(cp.Data.ResolutionCalibrationSteps.ToString(), out ResolutionCalibrationSteps resolutionCalibrationSteps);
                if ((cp.Status == MessageStatus.OperationEnd || cp.Status == MessageStatus.OperationStop) && resolutionCalibrationSteps == ResolutionCalibrationSteps.StartProcedure)
                {
                    this.IsReadInitialPositionActive = true;
                }
            }
        }

        #endregion
    }
}
