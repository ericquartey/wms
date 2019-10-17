using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class ShutterEnduranceTestViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly ShutterSensors sensors;

        private readonly IMachineShuttersWebService shuttersWebService;

        private int bayNumber;

        private int? completedCycles;

        private int? inputDelayBetweenCycles;

        private int? inputRequiredCycles;

        private bool isExecutingProcedure;

        private bool isWaitingForResponse;

        private SubscriptionToken receivedActionUpdateErrorToken;

        private SubscriptionToken sensorsChangedToken;

        private SubscriptionToken shutterTestStatusChangedToken;

        private DelegateCommand startCommand;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public ShutterEnduranceTestViewModel(
            IMachineShuttersWebService shuttersWebService,
            IBayManager bayManager,
            IMachineSensorsWebService machineSensorsWebService)
            : base(PresentationMode.Installer)
        {
            if (shuttersWebService is null)
            {
                throw new System.ArgumentNullException(nameof(shuttersWebService));
            }

            if (bayManager is null)
            {
                throw new System.ArgumentNullException(nameof(bayManager));
            }

            if (machineSensorsWebService is null)
            {
                throw new System.ArgumentNullException(nameof(machineSensorsWebService));
            }

            this.machineSensorsWebService = machineSensorsWebService;

            this.shuttersWebService = shuttersWebService;

            this.bayManager = bayManager;

            this.sensors = new ShutterSensors();
        }

        #endregion

        #region Delegates

        public delegate void CheckAccuracyOnPropertyChangedEventHandler();

        #endregion

        #region Properties

        public int BayNumber
        {
            get => this.bayNumber;
            private set => this.SetProperty(ref this.bayNumber, value);
        }

        public int? CompletedCycles
        {
            get => this.completedCycles;
            private set => this.SetProperty(ref this.completedCycles, value);
        }

        public string Error => string.Join(
                System.Environment.NewLine,
                this[nameof(this.InputDelayBetweenCycles)],
                this[nameof(this.InputRequiredCycles)]);

        public int? InputDelayBetweenCycles
        {
            get => this.inputDelayBetweenCycles;
            set
            {
                if (this.SetProperty(ref this.inputDelayBetweenCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? InputRequiredCycles
        {
            get => this.inputRequiredCycles;
            set
            {
                if (this.SetProperty(ref this.inputRequiredCycles, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsExecutingProcedure
        {
            get => this.isExecutingProcedure;
            private set
            {
                if (this.SetProperty(ref this.isExecutingProcedure, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ShutterSensors Sensors => this.sensors;

        public ICommand StartCommand =>
            this.startCommand
            ??
            (this.startCommand = new DelegateCommand(
                async () => await this.StartAsync(),
                this.CanExecuteStartCommand));

        public ICommand StopCommand =>
            this.stopCommand
            ??
            (this.stopCommand = new DelegateCommand(
                async () => await this.StopAsync(),
                this.CanExecuteStopCommand));

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputRequiredCycles):
                        if (!this.InputRequiredCycles.HasValue)
                        {
                            return $"InputRequiredCycles is required.";
                        }

                        if (this.InputRequiredCycles.Value <= 0)
                        {
                            return "InputRequiredCycles must be strictly positive.";
                        }

                        break;

                    case nameof(this.InputDelayBetweenCycles):
                        if (!this.InputDelayBetweenCycles.HasValue)
                        {
                            return $"InputDelayBetweenCycles is required.";
                        }

                        if (this.InputDelayBetweenCycles.Value <= 0)
                        {
                            return "InputDelayBetweenCycles must be strictly positive.";
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.shutterTestStatusChangedToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                    .Unsubscribe(this.shutterTestStatusChangedToken);

                this.shutterTestStatusChangedToken = null;
            }

            if (this.sensorsChangedToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.sensorsChangedToken);

                this.sensorsChangedToken = null;
            }

            if (this.receivedActionUpdateErrorToken != null)
            {
                this.EventAggregator
                 .GetEvent<MachineAutomationErrorPubSubEvent>()
                 .Unsubscribe(this.receivedActionUpdateErrorToken);

                this.receivedActionUpdateErrorToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.shutterTestStatusChangedToken = this.EventAggregator
                .GetEvent<NotificationEventUI<ShutterPositioningMessageData>>()
                .Subscribe(
                    this.OnShutterTestStatusChanged,
                    ThreadOption.UIThread,
                    false);

            this.sensorsChangedToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    this.OnSensorsChanged,
                    ThreadOption.UIThread,
                    false);

            try
            {
                this.IsWaitingForResponse = true;

                var bay = await this.bayManager.GetBay();
                this.BayNumber = (int)bay.Number;

                var procedureParameters = await this.shuttersWebService.GetTestParametersAsync();
                this.InputRequiredCycles = procedureParameters.RequiredCycles;
                this.InputDelayBetweenCycles = procedureParameters.DelayBetweenCycles;

                var sensorsStates = await this.machineSensorsWebService.GetAsync();
                this.sensors.Update(sensorsStates.ToArray(), this.BayNumber);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private bool CanExecuteStartCommand()
        {
            return
                !this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse
                &&
                string.IsNullOrWhiteSpace(this.Error);
        }

        private bool CanExecuteStopCommand()
        {
            return
                this.IsExecutingProcedure
                &&
                !this.IsWaitingForResponse;
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            if (message is null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }

            this.sensors.Update(message.Data?.SensorsStates, this.BayNumber);
        }

        private void OnShutterTestStatusChanged(NotificationMessageUI<ShutterPositioningMessageData> message)
        {
            if (message is null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }

            if (message.Data is null)
            {
                throw new System.ArgumentException();
            }

            if (message.IsErrored())
            {
                this.IsExecutingProcedure = false;
            }
            else if (message.IsNotRunning())
            {
                this.IsExecutingProcedure = false;
            }
            else
            {
                this.CompletedCycles = message.Data.PerformedCycles;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.startCommand.RaiseCanExecuteChanged();
            this.stopCommand.RaiseCanExecuteChanged();
        }

        private async Task StartAsync()
        {
            this.IsExecutingProcedure = true;
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.RunTestAsync(
                    this.InputDelayBetweenCycles.Value,
                    this.InputRequiredCycles.Value);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task StopAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.shuttersWebService.StopAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsExecutingProcedure = false;
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
