using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterEnduranceTestViewModel : BaseMainViewModel
    {
        #region Fields

        private int completedCycles;

        private int delayBetweenCycles;

        private bool canExecuteStartCommand = true;

        private bool canExecuteStopCommand;

        private SubscriptionToken receivedActionUpdateCompletedToken;

        private SubscriptionToken receivedActionUpdateErrorToken;

        private int requiredCycles;

        private IViewModel sensorRegion;

        private readonly IMachineShutterService shutterService;

        private ICommand startCommand;

        private ICommand stopCommand;

        private readonly IMachineTestService testService;

        private bool closeSensorState;

        private bool openSensorState;

        public bool CloseSensorState { get => this.closeSensorState; set => this.SetProperty(ref this.closeSensorState, value); }

        public bool OpenSensorState { get => this.openSensorState; set => this.SetProperty(ref this.openSensorState, value); }

        #endregion

        #region Constructors

        public ShutterEnduranceTestViewModel(
            IMachineTestService testService,
            IMachineShutterService shutterService)
            : base(Services.PresentationMode.Installator)
        {
            if (testService == null)
            {
                throw new System.ArgumentNullException(nameof(testService));
            }

            if (shutterService == null)
            {
                throw new System.ArgumentNullException(nameof(shutterService));
            }

            this.testService = testService;
            this.shutterService = shutterService;
            this.InputsAccuracyControlEventHandler += this.CheckInputsAccuracy;
        }

        #endregion

        #region Delegates

        public delegate void CheckAccuracyOnPropertyChangedEventHandler();

        #endregion

        #region Events

        public event CheckAccuracyOnPropertyChangedEventHandler InputsAccuracyControlEventHandler;

        #endregion

        #region Properties

        public int CompletedCycles
        {
            get => this.completedCycles;
            set => this.SetProperty(ref this.completedCycles, value);
        }

        public int DelayBetweenCycles
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
            get => this.canExecuteStartCommand;
            set => this.SetProperty(ref this.canExecuteStartCommand, value);
        }

        public bool IsStopButtonActive
        {
            get => this.canExecuteStopCommand;
            set => this.SetProperty(ref this.canExecuteStopCommand, value);
        }

        public int RequiredCycles
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
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.ExecuteStartCommandAsync()));

        public ICommand StopButtonCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.ExecuteStopCommandAsync()));

        #endregion

        #region Methods

        public async Task GetIntegerParametersAsync()
        {
            const string category = "GeneralInfo";
            this.RequiredCycles = await this.shutterService.GetIntegerConfigurationParameterAsync(category, "RequiredCycles");
            this.DelayBetweenCycles = await this.shutterService.GetIntegerConfigurationParameterAsync(category, "DelayBetweenCycles");

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
            // TODO swap between 2/3 positions bay

            this.receivedActionUpdateCompletedToken = this.EventAggregator
                .GetEvent<NotificationEventUI<ShutterControlMessageData>>()
                .Subscribe(
                    message =>
                    {
                        this.UpdateCompletedCycles(new MessageNotifiedEventArgs(message));
                    },
                    ThreadOption.PublisherThread,
                    false);

            this.receivedActionUpdateErrorToken = this.EventAggregator
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
            this.EventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.receivedActionUpdateErrorToken);
            this.EventAggregator.GetEvent<NotificationEventUI<ShutterPositioningMessageData>>().Unsubscribe(this.receivedActionUpdateCompletedToken);
        }

        private void CheckInputsAccuracy()
        {
            this.IsStartButtonActive = this.requiredCycles > 0 && this.delayBetweenCycles > 0;
        }

        private async Task ExecuteStartCommandAsync()
        {
            this.IsStartButtonActive = false;
            this.IsStopButtonActive = true;

            const int bayNumber = 1; // TODO remove hardcoded bay number
            await this.shutterService.ExecuteControlTestAsync(bayNumber, this.DelayBetweenCycles, this.RequiredCycles);
        }

        private async Task ExecuteStopCommandAsync()
        {
            this.IsStartButtonActive = true;
            this.IsStopButtonActive = false;

            await this.shutterService.StopAsync();
        }

        private void UpdateCompletedCycles(MessageNotifiedEventArgs data)
        {
            if (data.NotificationMessage is ShutterControlMessageData parsedData)
            {
                if (this.RequiredCycles == parsedData.CurrentShutterPosition)
                {
                    this.IsStartButtonActive = true;
                    this.IsStopButtonActive = false;
                }

                this.CompletedCycles = parsedData.CurrentShutterPosition;
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
