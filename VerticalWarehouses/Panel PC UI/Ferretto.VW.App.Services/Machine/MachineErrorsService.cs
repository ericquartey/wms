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

        private async Task NavigateToErrorPageAsync()
        {
            if (!this.AutoNavigateOnError)
            {
                return;
            }

            if (this.ActiveError is null)
            {
                //FRANCESCO 20200311
                // errore è in questo punto... in pratica le view degli errori sono di tre tipi e si accumulano nello stack
                // è abbastanza chiaro nel momento in cui si scatena un errore inverter (perchè si porta dietro anche un errore di macchina)
                // un solo go back ti riporta ad una view vuota (l'analisi l'ho fatta controllando lo stack delle pagine)

                //Soluzione 1: quando gli allarmi sono finiti lancio dei goback fino a che non trovo una pagina non di allarme (graficamente lenta)
                // Soluzione 2: modifica al navigation service "go back to first without error"
                // Soluzione 3: ad ogni mark as resolved faccio un go back (vedi errordetailsviewmodel ecc...), interfacciandomi con la navigazione

                // soluzione temp per evitare di accumulare pagine, vedi riga 215

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
            else
            {
                this.ViewErrorActive = Utils.Modules.Errors.ERRORDETAILSVIEW;

                if ((this.ActiveError.Code == (int)MachineErrorCode.LoadUnitMissingOnElevator) ||
                    (this.ActiveError.Code == (int)MachineErrorCode.LoadUnitMissingOnBay))
                {
                    this.ViewErrorActive = Utils.Modules.Errors.ERRORLOADUNITMISSING;
                }
                if (this.ActiveError.Code == (int)MachineErrorCode.InverterFaultStateDetected)
                {
                    this.ViewErrorActive = Utils.Modules.Errors.ERRORINVERTERFAULT;
                }

                await Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        //soluzione temp
                        if (this.navigationService.IsActiveView(nameof(Utils.Modules.Errors), Utils.Modules.Errors.ERRORDETAILSVIEW)
                            ||
                            this.navigationService.IsActiveView(nameof(Utils.Modules.Errors), Utils.Modules.Errors.ERRORINVERTERFAULT)
                            ||
                            this.navigationService.IsActiveView(nameof(Utils.Modules.Errors), Utils.Modules.Errors.ERRORLOADUNITMISSING)
                            )
                        {
                            this.navigationService.GoBack();
                        }
                        //
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
