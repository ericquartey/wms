using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationError : BasePresentation
    {
        #region Fields

        private readonly IMachineErrorsService machineErrorsService;

        private readonly INavigationService navigationService;

        private readonly IOperatorHubClient operatorHubClient;

        private bool areErrorsPresent;

        #endregion

        #region Constructors

        public PresentationError(
            IMachineErrorsService machineErrorsService,
            IOperatorHubClient operatorHubClient,
            INavigationService navigationService)
            : base(PresentationTypes.Error)
        {
            if (machineErrorsService is null)
            {
                throw new ArgumentNullException(nameof(machineErrorsService));
            }

            if (operatorHubClient is null)
            {
                throw new ArgumentNullException(nameof(operatorHubClient));
            }

            if (navigationService is null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.machineErrorsService = machineErrorsService;
            this.operatorHubClient = operatorHubClient;
            this.navigationService = navigationService;

            this.operatorHubClient.ErrorStatusChanged += async (sender, e) => await this.OnMachineErrorStatusChangedAsync();
            this.operatorHubClient.ConnectionStatusChanged += async (sender, e) => await this.OnHubConnectionChangedAsync(sender, e);
        }

        #endregion

        #region Properties

        public new bool? IsVisible
        {
            get => base.IsVisible.HasValue
                ? base.IsVisible.Value && this.areErrorsPresent
                : base.IsVisible;
            set => base.IsVisible = value;
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.navigationService.Appear(
                nameof(Utils.Modules.Errors),
                Utils.Modules.Errors.ERRORDETAILSVIEW,
                data: null,
                trackCurrentView: true);

            return Task.CompletedTask;
        }

        public async override Task OnLoadedAsync()
        {
            await this.CheckErrorsPresenceAsync();

            await base.OnLoadedAsync();
        }

        private async Task CheckErrorsPresenceAsync()
        {
            try
            {
                var error = await this.machineErrorsService.GetCurrentAsync();

                this.areErrorsPresent = error != null;

                this.IsVisible = this.areErrorsPresent;
            }
            catch (Exception)
            {
                // TODO: show error
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
            await this.CheckErrorsPresenceAsync();
        }

        #endregion
    }
}
