using System;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class MachineErrorsService : IMachineErrorsService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly SubscriptionToken machineModeChangedToken;

        private readonly SubscriptionToken machinePowerChangedToken;

        private readonly IMachineService machineService;

        private readonly SubscriptionToken machineStatusChangesToken;

        private readonly INavigationService navigationService;

        private readonly IOperatorHubClient operatorHubClient;

        private readonly ISensorsService sensorsService;

        private readonly SubscriptionToken sensorsToken;

        private MachineError activeError;

        private bool autoNavigateOnError;

        #endregion

        #region Constructors

        public MachineErrorsService(
            IMachineErrorsWebService machineErrorsWebService,
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            IMachineService machineService,
            IOperatorHubClient operatorHubClient,
            ISensorsService sensorsService)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));

            this.sensorsToken = this.eventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        async (m) => this.OnSensorsChanged(m),
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);

            this.machineStatusChangesToken = this.machineStatusChangesToken
                ?? this.eventAggregator
                    .GetEvent<MachineStatusChangedPubSubEvent>()
                    .Subscribe(
                        this.OnMachineStatusChanged,
                        ThreadOption.UIThread,
                        false);

            this.machineModeChangedToken = this.machineModeChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
                    .Subscribe(
                       this.OnChangedEventArgs,
                       ThreadOption.UIThread,
                       false);

            this.machinePowerChangedToken = this.machinePowerChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MachinePowerChangedEventArgs>>()
                    .Subscribe(
                       this.OnChangedEventArgs,
                       ThreadOption.UIThread,
                       false);

            this.operatorHubClient.ErrorStatusChanged += async (sender, e) => await this.OnMachineErrorStatusChangedAsync();
            this.operatorHubClient.ConnectionStatusChanged += async (sender, e) => await this.OnHubConnectionChangedAsync(sender, e);
        }

        #endregion

        #region Events

        public event EventHandler<MachineErrorEventArgs> ErrorStatusChanged;

        #endregion

        #region Properties

        public MachineError ActiveError
        {
            get => this.activeError;
            set
            {
                if (this.activeError != value)
                {
                    this.activeError = value;

                    var newErrorPresent = this.activeError != null;

                    this.eventAggregator
                      .GetEvent<PresentationChangedPubSubEvent>()
                      .Publish(
                          new PresentationChangedMessage(
                              new Presentation
                              {
                                  Type = PresentationTypes.Error,
                                  IsVisible = newErrorPresent
                              }));
                }
            }
        }

        public bool AutoNavigateOnError
        {
            get => this.autoNavigateOnError;
            set
            {
                this.autoNavigateOnError = value;

                this.NavigateToErrorPageAsync();
            }
        }

        public string ViewErrorActive { get; set; }

        #endregion

        #region Methods

        private async Task CheckErrorsPresenceAsync()
        {
            try
            {
                var prevError = this.ActiveError;
                this.ActiveError = await this.machineErrorsWebService.GetCurrentAsync();
                if (this.ActiveError != prevError)
                {
                    await this.NavigateToErrorPageAsync();
                }
            }
            catch (Exception)
            {
                // TODO: show error
            }
        }

        private string GetViewDescription(int activeErrorCode)
        {
            string viewDesc = Utils.Modules.Errors.ERRORDETAILSVIEW;

            if ((activeErrorCode == (int)MachineErrorCode.LoadUnitMissingOnElevator) ||
                (activeErrorCode == (int)MachineErrorCode.LoadUnitMissingOnBay))
            {
                viewDesc = Utils.Modules.Errors.ERRORLOADUNITMISSING;
            }
            if (activeErrorCode == (int)MachineErrorCode.InverterFaultStateDetected)
            {
                viewDesc = Utils.Modules.Errors.ERRORINVERTERFAULT;
            }

            return viewDesc;
        }

        private async Task NavigateToErrorPageAsync()
        {
            if (!this.AutoNavigateOnError)
            {
                return;
            }

            if (this.ActiveError is null)
            {
                await Application.Current.Dispatcher.BeginInvoke(
                   System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                   new Action(() =>
                   {
                       if (this.navigationService.IsActiveView(nameof(Utils.Modules.Errors), this.ViewErrorActive))
                       {
                           this.navigationService.GoBack();
                       }
                   }));

                this.ViewErrorActive = null;
            }
            else
            {
                string viewRequest = this.GetViewDescription(this.ActiveError.Code);

                if ((this.ViewErrorActive != null) && (this.ViewErrorActive != viewRequest))
                {
                    await Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        if (this.navigationService.IsActiveView(nameof(Utils.Modules.Errors), this.ViewErrorActive))
                        {
                            this.navigationService.GoBack();
                        }
                    }));
                }

                this.ViewErrorActive = viewRequest;

                await Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        this.navigationService.Appear(
                                nameof(Utils.Modules.Errors),
                                this.ViewErrorActive,
                                data: null,
                                trackCurrentView: true);
                    }));
            }
        }

        private void OnChangedEventArgs(EventArgs e)
        {
        }

        private async Task OnHubConnectionChangedAsync(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                await this.CheckErrorsPresenceAsync();
            }
        }

        private async Task OnMachineErrorStatusChangedAsync()
        {
            await this.CheckErrorsPresenceAsync()
                .ContinueWith((m) => this.ErrorStatusChanged?.Invoke(null, new MachineErrorEventArgs(this.ActiveError)));
        }

        private void OnMachineStatusChanged(MachineStatusChangedMessage e)
        {
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
        }

        #endregion
    }
}
