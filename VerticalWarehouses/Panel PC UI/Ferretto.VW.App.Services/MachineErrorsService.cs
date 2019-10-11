using System;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal class MachineErrorsService : IMachineErrorsService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly INavigationService navigationService;

        private readonly IOperatorHubClient operatorHubClient;

        private Error activeError;

        private bool autoNavigateOnError;

        #endregion

        #region Constructors

        public MachineErrorsService(
            IMachineErrorsWebService machineErrorsWebService,
            IEventAggregator eventAggregator,
            INavigationService navigationService,
            IOperatorHubClient operatorHubClient)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.operatorHubClient = operatorHubClient ?? throw new ArgumentNullException(nameof(operatorHubClient));

            this.operatorHubClient.ErrorStatusChanged += async (sender, e) => await this.OnMachineErrorStatusChangedAsync();
            this.operatorHubClient.ConnectionStatusChanged += async (sender, e) => await this.OnHubConnectionChangedAsync(sender, e);
        }

        #endregion

        #region Properties

        public Error ActiveError
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
                                    IsEnabled = newErrorPresent
                                }));

                    if (newErrorPresent && this.AutoNavigateOnError)
                    {
                        this.NavigateToErrorPageAsync();
                    }
                }
            }
        }

        public bool AutoNavigateOnError
        {
            get => this.autoNavigateOnError;
            set
            {
                this.autoNavigateOnError = value;

                if (this.autoNavigateOnError && this.ActiveError != null)
                {
                    this.NavigateToErrorPageAsync();
                }
            }
        }

        #endregion

        #region Methods

        private async Task CheckErrorsPresenceAsync()
        {
            try
            {
                this.ActiveError = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (Exception)
            {
                // TODO: show error
            }
        }

        private async Task NavigateToErrorPageAsync()
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

        private async Task OnHubConnectionChangedAsync(object sender, ConnectionStatusChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                await this.CheckErrorsPresenceAsync();
            }
        }

        private async Task OnMachineErrorStatusChangedAsync()
        {
            await this.CheckErrorsPresenceAsync();
        }

        #endregion
    }
}
