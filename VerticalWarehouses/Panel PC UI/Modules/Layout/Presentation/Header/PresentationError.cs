using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationError : BasePresentation
    {
        #region Fields

        private readonly IErrorsMachineService errorsMachineService;

        private readonly INavigationService navigationService;

        private readonly IOperatorHubClient operatorHubClient;

        private bool areErrorsPresent;

        #endregion

        #region Constructors

        public PresentationError(
            IErrorsMachineService errorsMachineService,
            IOperatorHubClient operatorHubClient,
            INavigationService navigationService)
        {
            if (errorsMachineService == null)
            {
                throw new System.ArgumentNullException(nameof(errorsMachineService));
            }

            if (operatorHubClient == null)
            {
                throw new System.ArgumentNullException(nameof(operatorHubClient));
            }

            if (navigationService == null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.errorsMachineService = errorsMachineService;
            this.operatorHubClient = operatorHubClient;
            this.navigationService = navigationService;

            this.Type = PresentationTypes.Error;

            this.operatorHubClient.ErrorStatusChanged += async (sender, e) => await this.OnMachineErrorStatusChangedAsync(sender, e);
            this.operatorHubClient.ConnectionStatusChanged += async (sender, e) => await this.OnHubConnectionChangedAsync(sender, e);

            this.CheckErrorsPresenceAsync();
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

        public override void Execute()
        {
            this.navigationService.Appear(nameof(Utils.Modules.Errors), Utils.Modules.Errors.ERRORDETAILSVIEW);
        }

        private async Task CheckErrorsPresenceAsync()
        {
            try
            {
                var error = await this.errorsMachineService.GetCurrentAsync();

                this.areErrorsPresent = error != null;

                this.IsVisible = this.IsVisible.HasValue
                    ? this.IsVisible.Value && this.areErrorsPresent
                    : this.IsVisible;
            }
            catch (System.Exception ex)
            {
                // TODO: show error
            }
        }

        private async Task OnHubConnectionChangedAsync(object sender, ConnectionStatusChangedEventArgs e)
        {
            await this.CheckErrorsPresenceAsync();
        }

        private async Task OnMachineErrorStatusChangedAsync(object sender, ErrorStatusChangedEventArgs e)
        {
            await this.CheckErrorsPresenceAsync();
        }

        #endregion
    }
}
