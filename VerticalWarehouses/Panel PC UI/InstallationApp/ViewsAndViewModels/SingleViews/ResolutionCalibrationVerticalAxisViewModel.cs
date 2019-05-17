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

        private bool isAcceptButtonActive = true;

        private bool isMesuredInitialPositionHighlighted;

        private bool isMesuredInitialPositionTextInputActive = true;

        private bool isMesuredMovementHighlighted;

        private bool isMesuredMovementTextInputActive = true;

        private bool isMoveButtonActive = true;

        private bool isSetPositionButtonActive = true;

        private string mesuredInitialPosition;

        private string mesuredLenght;

        private ICommand moveButtonCommand;

        private string newResolution;

        private string noteString = VW.Resources.InstallationApp.MoveToInitialPosition;

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

        public bool IsMesuredInitialPositionHighlighted { get => this.isMesuredInitialPositionHighlighted; set => this.SetProperty(ref this.isMesuredInitialPositionHighlighted, value); }

        public bool IsMesuredInitialPositionTextInputActive { get => this.isMesuredInitialPositionTextInputActive; set => this.SetProperty(ref this.isMesuredInitialPositionTextInputActive, value); }

        public bool IsMesuredLenghtTextInputActive { get => this.isMesuredMovementTextInputActive; set => this.SetProperty(ref this.isMesuredMovementTextInputActive, value); }

        public bool IsMesuredMovementHighlighted { get => this.isMesuredMovementHighlighted; set => this.SetProperty(ref this.isMesuredMovementHighlighted, value); }

        public bool IsMoveButtonActive { get => this.isMoveButtonActive; set => this.SetProperty(ref this.isMoveButtonActive, value); }

        public bool IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }

        public string MesuredInitialPosition
        {
            get => this.mesuredInitialPosition;
            set
            {
                this.SetProperty(ref this.mesuredInitialPosition, value);
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

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public string ReadFinalPosition { get; set; }

        public string ReadInitialPosition { get; set; }

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
            // TODO implement feature
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
            this.NoteString = Ferretto.VW.Resources.InstallationApp.MoveToInitialPosition;
            this.IsAcceptButtonActive = false;
            this.IsMesuredInitialPositionHighlighted = false;
            this.IsMesuredInitialPositionTextInputActive = false;
            this.IsMesuredLenghtTextInputActive = false;
            this.IsMoveButtonActive = false;
            this.IsSetPositionButtonActive = false;
        }

        private void CheckDesideredInitialPositionCorrectness(string input)
        {
            this.IsSetPositionButtonActive = false;

            if (input != "")
            {
                if (decimal.TryParse(input, out var i))
                {
                    if (i > 0)
                    {
                        this.IsSetPositionButtonActive = true;
                    }
                }
            }
        }

        private void CheckMesuredInitialPositionCorrectness(string input)
        {
            if (input != "")
            {
                if (int.TryParse(input, out var i))
                {
                    if (i > 0)
                    {
                        this.IsMoveButtonActive = true;
                        this.IsMesuredInitialPositionTextInputActive = false;
                        this.IsMesuredInitialPositionHighlighted = false;
                        this.NoteString = Ferretto.VW.Resources.InstallationApp.MoveToPosition;
                        this.IsMesuredInitialPositionTextInputActive = true;
                        this.IsMesuredInitialPositionHighlighted = true;
                    }
                }
            }
        }

        private void CheckMesuredRepositionLenghtCorrectness(string input)
        {
            if (input != "")
            {
                if (int.TryParse(input, out var i))
                {
                    if (i > 0)
                    {
                        this.IsMesuredMovementHighlighted = false;
                        this.IsMesuredLenghtTextInputActive = false;
                        this.CalculateNewResolutionMethod();
                        this.NoteString = Ferretto.VW.Resources.InstallationApp.ConfirmResolution;
                    }
                }
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
