using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineIdentityService machineIdentityService;

        private Machine parameters;

        #endregion

        #region Constructors

        public ParametersViewModel(IMachineIdentityService machineIdentityService)
            : base(Services.PresentationMode.Installer)
        {
            this.machineIdentityService = machineIdentityService ?? throw new ArgumentNullException(nameof(machineIdentityService));
        }

        #endregion

        #region Properties

        public Machine Parameters => this.parameters;

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            this.parameters = await this.machineIdentityService.GetMachineAsync();
            this.RaisePropertyChanged(nameof(this.Parameters));
        }

        #endregion
    }
}
