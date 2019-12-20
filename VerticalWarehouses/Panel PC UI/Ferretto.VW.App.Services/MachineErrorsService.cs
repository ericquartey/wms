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
            IOperatorHubClient operatorHubClient,
            ISensorsService sensorsService)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));
            this.sensorsService = sensorsService ?? throw new ArgumentNullException(nameof(sensorsService));

            this.sensorsToken = this.eventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data != null);

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

        private void CheckMissingLoadunits()
        {
            // presenza elevatore
            if (this.sensorsService.IsLoadingUnitOnElevator)
            {
            }

            // presenza baia 1
            if (this.sensorsService.IsLoadingUnitInBayByNumber(BayNumber.BayOne))
            {
            }
            if (this.sensorsService.IsLoadingUnitInMiddleBottomBayByNumber(BayNumber.BayOne))
            {
            }

            // presenza baia 2
            if (this.sensorsService.IsLoadingUnitInBayByNumber(BayNumber.BayTwo))
            {
            }
            if (this.sensorsService.IsLoadingUnitInMiddleBottomBayByNumber(BayNumber.BayTwo))
            {
            }

            // presenza baia 3
            if (this.sensorsService.IsLoadingUnitInBayByNumber(BayNumber.BayThree))
            {
            }
            if (this.sensorsService.IsLoadingUnitInMiddleBottomBayByNumber(BayNumber.BayThree))
            {
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
                await Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        if (this.navigationService.IsActiveView(nameof(Utils.Modules.Errors), Utils.Modules.Errors.ERRORDETAILSVIEW))
                        {
                            this.navigationService.GoBack();
                        }
                    }));
            }
            else
            {
                await Application.Current.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        this.navigationService.Appear(
                            nameof(Utils.Modules.Errors),
                            Utils.Modules.Errors.ERRORDETAILSVIEW,
                            data: null,
                            trackCurrentView: true);
                    }));
            }
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

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.CheckMissingLoadunits();
        }

        #endregion
    }
}
