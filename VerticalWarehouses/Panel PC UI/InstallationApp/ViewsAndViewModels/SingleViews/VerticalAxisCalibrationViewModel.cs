using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalAxisCalibrationViewModel : BindableBase, IVerticalAxisCalibrationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private IInstallationService installationService;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive = true;

        private string lowerBound;

        private string noteString = VW.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;

        private string offset;

        private SubscriptionToken receivedActionUpdateToken;

        private string resolution;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private string upperBound;

        #endregion

        #region Constructors

        public VerticalAxisCalibrationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.InputsCorrectionControlEventHandler += this.CheckInputsCorrectness;
        }

        #endregion

        #region Delegates

        public delegate void CheckCorrectnessOnPropertyChangedEventHandler();

        #endregion

        #region Events

        public event CheckCorrectnessOnPropertyChangedEventHandler InputsCorrectionControlEventHandler;

        #endregion

        #region Properties

        public bool IsStartButtonActive { get => this.isStartButtonActive; set => this.SetProperty(ref this.isStartButtonActive, value); }

        public bool IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public string LowerBound
        {
            get => this.lowerBound;
            set
            {
                this.SetProperty(ref this.lowerBound, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public string Offset
        {
            get => this.offset;
            set
            {
                this.SetProperty(ref this.offset, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

        public string Resolution
        {
            get => this.resolution;
            set
            {
                this.SetProperty(ref this.resolution, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(async () => await this.ExecuteStartButtonCommandAsync()));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopButtonMethodAsync()));

        public string UpperBound
        {
            get => this.upperBound;
            set
            {
                this.SetProperty(ref this.upperBound, value);
                this.InputsCorrectionControlEventHandler();
            }
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task GetParameterValuesAsync()
        {
            try
            {
                this.UpperBound = (await this.installationService.GetDecimalConfigurationParameterAsync("GeneralInfo", "Height")).ToString();
                this.LowerBound = (await this.installationService.GetDecimalConfigurationParameterAsync("GeneralInfo", "LowerBound")).ToString();
                this.Offset = (await this.installationService.GetDecimalConfigurationParameterAsync("GeneralInfo", "Offset")).ToString();
                this.Resolution = (await this.installationService.GetDecimalConfigurationParameterAsync("GeneralInfo", "Resolution")).ToString();
            }
            catch (SwaggerException ex)
            {
                this.NoteString = VW.Resources.InstallationApp.ErrorRetrievingConfigurationData;
            }
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.installationService = this.container.Resolve<IInstallationService>();
        }

        public async Task OnEnterViewAsync()
        {
            await this.GetParameterValuesAsync();
            this.receivedActionUpdateToken = this.eventAggregator.GetEvent<MAS_Event>().Subscribe(
                (msg) => this.UpdateCurrentActionStatus(msg),
                ThreadOption.PublisherThread,
                false,
                message => message.NotificationType == NotificationType.CurrentActionStatus &&
                (message.ActionType == ActionType.Homing
                || message.ActionType == ActionType.HorizontalHoming
                || message.ActionType == ActionType.VerticalHoming
                || message.ActionType == ActionType.SwitchEngine));
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.receivedActionUpdateToken);
        }

        public void UpdateCurrentActionStatus(MAS_EventMessage message)
        {
            if (message != null)
            {
                switch (message.ActionType)
                {
                    case ActionType.Homing:
                        switch (message.ActionStatus)
                        {
                            case ActionStatus.Start:
                                this.NoteString = VW.Resources.InstallationApp.HomingStarted;
                                break;

                            case ActionStatus.Completed:
                                this.NoteString = VW.Resources.InstallationApp.HomingCompleted;
                                this.IsStartButtonActive = true;
                                this.IsStopButtonActive = false;
                                break;

                            case ActionStatus.Error:
                                this.NoteString = VW.Resources.InstallationApp.HomingError;
                                this.IsStartButtonActive = true;
                                this.IsStopButtonActive = false;
                                break;
                        }
                        break;

                    case ActionType.HorizontalHoming:
                        switch (message.ActionStatus)
                        {
                            case ActionStatus.Start:
                                this.NoteString = VW.Resources.InstallationApp.HorizontalHomingStarted;
                                break;

                            case ActionStatus.Executing:
                                this.NoteString = VW.Resources.InstallationApp.HorizontalHomingExecuting;
                                break;

                            case ActionStatus.Completed:
                                this.NoteString = VW.Resources.InstallationApp.HorizontalHomingCompleted;
                                break;

                            case ActionStatus.Error:
                                this.NoteString = VW.Resources.InstallationApp.HorizontalHomingError;
                                this.IsStartButtonActive = true;
                                this.IsStopButtonActive = false;
                                break;
                        }
                        break;

                    case ActionType.VerticalHoming:
                        switch (message.ActionStatus)
                        {
                            case ActionStatus.Start:
                                this.NoteString = VW.Resources.InstallationApp.VerticalHomingStarted;
                                break;

                            case ActionStatus.Executing:
                                this.NoteString = VW.Resources.InstallationApp.VerticalHomingExecuting;
                                break;

                            case ActionStatus.Completed:
                                this.NoteString = VW.Resources.InstallationApp.VerticalHomingCompleted;
                                break;

                            case ActionStatus.Error:
                                this.NoteString = VW.Resources.InstallationApp.VerticalHomingError;
                                this.IsStartButtonActive = true;
                                this.IsStopButtonActive = false;
                                break;
                        }
                        break;

                    case ActionType.SwitchEngine:
                        switch (message.ActionStatus)
                        {
                            case ActionStatus.Start:
                                this.NoteString = VW.Resources.InstallationApp.SwitchEngineStarted;
                                break;

                            case ActionStatus.Completed:
                                this.NoteString = VW.Resources.InstallationApp.SwitchEngineCompleted;
                                break;

                            case ActionStatus.Error:
                                this.NoteString = VW.Resources.InstallationApp.SwitchEngineError;
                                this.IsStartButtonActive = true;
                                this.IsStopButtonActive = false;
                                break;
                        }
                        break;
                }
            }
            else
            {
                throw new ArgumentNullException("Message is null");
            }
        }

        private void CheckInputsCorrectness()
        {
            if (int.TryParse(this.LowerBound, out var _lowerBound) &&
                int.TryParse(this.Offset, out var _offset) &&
                int.TryParse(this.Resolution, out var _resolution) &&
                int.TryParse(this.UpperBound, out var _upperBound))
            { // TODO: DEFINE AND INSERT VALIDATION LOGIC IN HERE. THESE PROPOSITIONS ARE TEMPORARY
                this.IsStartButtonActive = (_lowerBound > 0 && _lowerBound < _upperBound && _upperBound > 0 && _resolution > 0 && _offset > 0) ? true : false;
            }
            else
            {
                this.IsStartButtonActive = false;
            }
        }

        private async Task ExecuteStartButtonCommandAsync()
        {
            try
            {
                this.IsStartButtonActive = false;
                this.IsStopButtonActive = true;

                await this.installationService.ExecuteHomingAsync();
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
        }

        private async Task StopButtonMethodAsync()
        {
            try
            {
                await this.installationService.StopCommandAsync();
                this.IsStartButtonActive = true;
                this.IsStopButtonActive = false;
                this.NoteString = VW.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
        }

        #endregion
    }
}
