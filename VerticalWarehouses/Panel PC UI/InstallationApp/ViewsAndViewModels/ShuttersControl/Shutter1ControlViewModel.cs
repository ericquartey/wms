using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.CommonUtils;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.ShuttersControl
{
    public class Shutter1ControlViewModel : BindableBase, IShutter1ControlViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private string completedCycles;

        private string delayBetweenCycles;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private SubscriptionToken receivedActionUpdateCompletedToken;

        private SubscriptionToken receivedActionUpdateErrorToken;

        private string requiredCycles;

        private IViewModel sensorRegion;

        private readonly IShutterMachineService shutterService;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private readonly ITestMachineService testService;

        private readonly ICustomShutterControlSensorsTwoPositionsViewModel customShutterControlSensorsTwoPositionsViewModel;

        #endregion

        #region Constructors

        public Shutter1ControlViewModel(
            IEventAggregator eventAggregator,
            ITestMachineService testService,
            IShutterMachineService shutterService,
            ICustomShutterControlSensorsTwoPositionsViewModel customShutterControlSensorsTwoPositionsViewModel)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (testService == null)
            {
                throw new System.ArgumentNullException(nameof(testService));
            }

            if (shutterService == null)
            {
                throw new System.ArgumentNullException(nameof(shutterService));
            }

            if (customShutterControlSensorsTwoPositionsViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(customShutterControlSensorsTwoPositionsViewModel));
            }

            this.eventAggregator = eventAggregator;
            this.testService = testService;
            this.shutterService = shutterService;
            this.customShutterControlSensorsTwoPositionsViewModel = customShutterControlSensorsTwoPositionsViewModel;
            this.InputsAccuracyControlEventHandler += this.CheckInputsAccuracy;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Delegates

        public delegate void CheckAccuracyOnPropertyChangedEventHandler();

        #endregion

        #region Events

        public event CheckAccuracyOnPropertyChangedEventHandler InputsAccuracyControlEventHandler;

        #endregion

        #region Properties

        public string CompletedCycles { get => this.completedCycles; set => this.SetProperty(ref this.completedCycles, value); }

        public string DelayBetweenCycles
        {
            get => this.delayBetweenCycles;
            set
            {
                this.SetProperty(ref this.delayBetweenCycles, value);
                this.InputsAccuracyControlEventHandler();
            }
        }

        public bool IsStartButtonActive
        {
            get => this.isStartButtonActive;
            set => this.SetProperty(ref this.isStartButtonActive, value);
        }

        public bool IsStopButtonActive
        {
            get => this.isStopButtonActive;
            set => this.SetProperty(ref this.isStopButtonActive, value);
        }

        public BindableBase NavigationViewModel { get; set; }

        public string RequiredCycles
        {
            get => this.requiredCycles;
            set
            {
                this.SetProperty(ref this.requiredCycles, value);
                this.InputsAccuracyControlEventHandler();
            }
        }

        public IViewModel SensorRegion
        {
            get => this.sensorRegion;
            set => this.SetProperty(ref this.sensorRegion, value);
        }

        public ICommand StartButtonCommand =>
            this.startButtonCommand ??
            (this.startButtonCommand = new DelegateCommand(
                async () => await this.ExecuteStartButtonCommandAsync()));

        public ICommand StopButtonCommand =>
            this.stopButtonCommand ??
            (this.stopButtonCommand = new DelegateCommand(
                async () => await this.ExecuteStopButtonCommandAsync()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task GetIntegerParametersAsync()
        {
            const string Category = "GeneralInfo";
            this.RequiredCycles = (await this.shutterService.GetIntegerConfigurationParameterAsync(Category, "RequiredCycles")).ToString();
            this.DelayBetweenCycles = (await this.shutterService.GetIntegerConfigurationParameterAsync(Category, "DelayBetweenCycles")).ToString();

#if !DEBUG
            /*    var client = new System.Net.HttpClient();
                var response = await client.GetAsync(new Uri(this.installationController + this.getIntegerValuesController + "RequiredCycles"));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    this.RequiredCycles = response.Content.ReadAsAsync<int>().Result.ToString();
                }
                response = null;
                response = await client.GetAsync(new System.Uri(this.installationController + this.getIntegerValuesController + "DelayBetweenCycles"));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    this.DelayBetweenCycles = response.Content.ReadAsAsync<int>().Result.ToString();
                }*/
#endif
        }

        public Task OnEnterViewAsync()
        {
            if (false /* Bay with three positions */) // TODO
            {
                // this.sensorRegion = (CustomShutterControlSensorsThreePositionsViewModel)this.container.Resolve<ICustomShutterControlSensorsThreePositionsViewModel>();
            }
            else
            {
                this.sensorRegion = this.customShutterControlSensorsTwoPositionsViewModel;
            }

            this.receivedActionUpdateCompletedToken = this.eventAggregator
                .GetEvent<NotificationEventUI<ShutterControlMessageData>>()
                .Subscribe(
                    message =>
                    {
                        this.UpdateCompletedCycles(new MessageNotifiedEventArgs(message));
                    },
                    ThreadOption.PublisherThread,
                    false);

            this.receivedActionUpdateErrorToken = this.eventAggregator
                .GetEvent<MAS_ErrorEvent>()
                .Subscribe(
                    msg => this.UpdateError(),
                    ThreadOption.PublisherThread,
                    false,
                    message =>
                    message.NotificationType == NotificationType.Error &&
                    message.ActionType == ActionType.ShutterControl &&
                    message.ActionStatus == ActionStatus.Error);

            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.receivedActionUpdateErrorToken);
            this.eventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>().Unsubscribe(this.receivedActionUpdateCompletedToken);
        }

        private void CheckInputsAccuracy()
        {
            if (int.TryParse(this.RequiredCycles, out var requiredCycles) &&
                int.TryParse(this.DelayBetweenCycles, out var delayBetweenCycles))
            {
                this.IsStartButtonActive = (requiredCycles > 0 && delayBetweenCycles > 0) ? true : false;
            }
            else
            {
                this.IsStartButtonActive = false;
            }
        }

        private async Task ExecuteStartButtonCommandAsync()
        {
            this.IsStartButtonActive = false;
            this.IsStopButtonActive = true;

            int.TryParse(this.DelayBetweenCycles, out var delay);
            int.TryParse(this.RequiredCycles, out var reqCycles);

            const int bayNumber = 1; //TEMP: Set the value bay index hardcoded
            await this.shutterService.ExecuteControlTestAsync(bayNumber, delay, reqCycles);
        }

        private async Task ExecuteStopButtonCommandAsync()
        {
            this.IsStartButtonActive = true;
            this.IsStopButtonActive = false;

            await this.shutterService.StopAsync();
        }

        private void UpdateCompletedCycles(MessageNotifiedEventArgs data)
        {
            if (data.NotificationMessage is NotificationMessageUI<ShutterControlMessageData> parsedData)
            {
                if (int.TryParse(this.RequiredCycles, out var value)
                    &&
                    value == parsedData.Data.CurrentShutterPosition)
                {
                    this.IsStartButtonActive = true;
                    this.IsStopButtonActive = false;
                }

                this.CompletedCycles = parsedData.Data.CurrentShutterPosition.ToString();
            }
        }

        private void UpdateError()
        {
            this.IsStartButtonActive = false;
            this.IsStopButtonActive = false;
        }

        #endregion
    }
}
