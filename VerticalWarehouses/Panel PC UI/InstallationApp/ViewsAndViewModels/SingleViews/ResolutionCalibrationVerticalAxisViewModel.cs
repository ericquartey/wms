using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Events;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

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

        private bool isDesiredInitialPositionActive;

        private bool isDesiredInitialPositionHighlighted;

        private bool isMesuredMovementActive;

        private bool isMesuredMovementHighlighted;

        private bool isMoveButtonActive;

        private bool isReadInitialPositionActive;

        private bool isReadInitialPositionHighlighted;

        private bool isSetPositionButtonActive;

        private string mesuredLenght;

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

        public bool IsDesiredInitialPositionActive { get => this.isDesiredInitialPositionActive; set => this.SetProperty(ref this.isDesiredInitialPositionActive, value); }

        public bool IsDesiredInitialPositionHighlighted { get => this.isDesiredInitialPositionHighlighted; set => this.SetProperty(ref this.isDesiredInitialPositionHighlighted, value); }

        public bool IsMesuredInitialPositionActive { get => this.isReadInitialPositionActive; set => this.SetProperty(ref this.isReadInitialPositionActive, value); }

        public bool IsMesuredInitialPositionHighlighted { get => this.isReadInitialPositionHighlighted; set => this.SetProperty(ref this.isReadInitialPositionHighlighted, value); }

        public bool IsMesuredLenghtActive { get => this.isMesuredMovementActive; set => this.SetProperty(ref this.isMesuredMovementActive, value); }

        public bool IsMesuredMovementHighlighted { get => this.isMesuredMovementHighlighted; set => this.SetProperty(ref this.isMesuredMovementHighlighted, value); }

        public bool IsMoveButtonActive { get => this.isMoveButtonActive; set => this.SetProperty(ref this.isMoveButtonActive, value); }

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

        public string MesuredLenght
        {
            get => this.mesuredLenght;
            set
            {
                this.SetProperty(ref this.mesuredLenght, value);
                this.CheckMesuredRepositionLenghtCorrectness(value);
            }
        }

        public ICommand MoveButtonCommand => this.moveButtonCommand ?? (this.moveButtonCommand = new DelegateCommand(() => this.MoveButtonMethod()));

        public BindableBase NavigationViewModel { get; set; }

        public string NewResolution { get => this.newResolution; set => this.SetProperty(ref this.newResolution, value); }

        public string ReadFinalPosition { get => this.readFinalPosition; set => this.SetProperty(ref this.readFinalPosition, value); }

        public string ReadInitialPosition { get => this.readInitialPosition; set => this.SetProperty(ref this.readInitialPosition, value); }

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
            // TODO implement feature
            await this.GetParameterValuesAsync();

            this.receivedActionToken = this.eventAggregator.GetEvent<NotificationEventUI<ResolutionCalibrationMessageData>>()
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
            decimal.TryParse(this.MesuredLenght, out var ml);
            decimal.TryParse(this.RepositionLenght, out var rl);
            this.NewResolution = ((cr * ml) / rl).ToString("##.##");
            this.IsAcceptButtonActive = true;
        }

        private void CancelButtonMethod()
        {
            this.RepositionLenght = string.Empty;
            this.MesuredLenght = string.Empty;
            this.DesideredFinalPosition = string.Empty;
            this.NewResolution = string.Empty;
            this.IsAcceptButtonActive = false;
            this.IsMesuredInitialPositionHighlighted = false;
            this.IsMesuredInitialPositionActive = false;
            this.IsMesuredLenghtActive = false;
            this.IsMoveButtonActive = false;
            this.IsSetPositionButtonActive = false;
        }

        private void CheckDesideredInitialPositionCorrectness(string input)
        {
            this.IsSetPositionButtonActive = false;

            if (!string.IsNullOrEmpty(input) && decimal.TryParse(input, out var i) && i > 0)
            {
                this.IsSetPositionButtonActive = true;
            }
        }

        private void CheckMesuredInitialPositionCorrectness(string input)
        {
            if (!string.IsNullOrEmpty(input) && decimal.TryParse(input, out var i) && i > 0)
            {
                this.IsMoveButtonActive = true;
                this.IsMesuredInitialPositionActive = false;
                this.IsMesuredInitialPositionHighlighted = false;
                this.IsMesuredInitialPositionActive = true;
                this.IsMesuredInitialPositionHighlighted = true;
            }
        }

        private void CheckMesuredRepositionLenghtCorrectness(string input)
        {
            if (!string.IsNullOrEmpty(input) && decimal.TryParse(input, out var i) && i > 0)
            {
                this.IsMesuredMovementHighlighted = false;
                this.IsMesuredLenghtActive = false;
                this.CalculateNewResolutionMethod();
            }
        }

        private void MoveButtonMethod()
        {
            // TODO implement feature
        }

        private async Task SetPositionButtonMethodAsync()
        {
            decimal.TryParse(this.ReadInitialPosition, out var readInitialPosition);
            decimal.TryParse(this.ReadFinalPosition, out var readFinalPosition);

            await this.installationService.ExecuteResolutionCalibrationAsync(readInitialPosition, readFinalPosition);
            //TEMP
            //await this.testService.ExecuteResolutionCalibrationAsync(readInitialPosition, readFinalPosition);
        }

        private void UpdateResolution(MessageNotifiedEventArgs message)
        {
            // TODO implement feature
        }

        #endregion
    }
}
