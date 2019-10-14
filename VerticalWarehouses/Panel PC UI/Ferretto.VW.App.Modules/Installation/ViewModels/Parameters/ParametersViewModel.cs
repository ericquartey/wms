using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private VertimagConfiguration configuration;

        private bool isBusy;

        private DelegateCommand saveCommand;

        #endregion

        #region Constructors

        public ParametersViewModel(IMachineConfigurationWebService machineConfigurationWebService)
            : base(Services.PresentationMode.Installer)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public VertimagConfiguration Configuration => this.configuration;

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public ICommand SaveCommand =>
                            this.saveCommand
           ??
           (this.saveCommand = new DelegateCommand(
               async () => await this.SaveAsync()));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.configuration = await this.machineConfigurationWebService.GetAsync();
            this.RaisePropertyChanged(nameof(this.Configuration));
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;

                await this.machineConfigurationWebService.SetAsync(this.configuration);

                this.ShowNotification(Resources.InstallationApp.SaveSuccessful);

                this.IsBusy = false;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        #endregion
    }
}
