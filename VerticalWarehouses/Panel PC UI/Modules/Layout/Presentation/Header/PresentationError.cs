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

            this.operatorHubClient.ErrorStatusChanged += async (sender, e) => await this.OnMachineErrorStatusChanged(sender, e);

            this.CheckErrorsPresenceAsync();
        }

        #endregion

        #region Properties

        public new bool IsVisible
        {
            get => base.IsVisible && this.areErrorsPresent;
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

                this.IsVisible = this.IsVisible && this.areErrorsPresent;
            }
            catch (System.Exception ex)
            {
                // TODO: show error
            }
        }

        private async Task OnMachineErrorStatusChanged(
            object sender,
            MAS.AutomationService.Contracts.Hubs.EventArgs.ErrorStatusChangedEventArgs e)
        {
            await this.CheckErrorsPresenceAsync();
        }

        #endregion
    }
}
