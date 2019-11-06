using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersImportStep2ViewModel : BaseParametersImportExportViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private VertimagConfiguration configuration;

        private DelegateCommand confirmSaveCommand;

        #endregion

        #region Constructors

        public ParametersImportStep2ViewModel(IMachineConfigurationWebService machineConfigurationWebService)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public VertimagConfiguration Configuration => this.configuration;

        public ICommand ConfirmSaveCommand =>
                    this.confirmSaveCommand
                   ??
                   (this.confirmSaveCommand = new DelegateCommand(
                       async () => await this.SaveAsync(), this.CanSave));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.IsBusy = true;

                if (this.Data is VertimagConfiguration vertimagConfiguration)
                {
                    this.configuration = vertimagConfiguration;
                    this.RaisePropertyChanged(nameof(this.Configuration));
                }

                this.IsBackNavigationAllowed = true;
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

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                this.ClearNotifications();

                await this.machineConfigurationWebService.SetAsync(this.configuration);

                this.ShowNotification(Resources.InstallationApp.RestoreSuccessful);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
                this.IsBackNavigationAllowed = true;
            }
        }

        #endregion
    }
}
