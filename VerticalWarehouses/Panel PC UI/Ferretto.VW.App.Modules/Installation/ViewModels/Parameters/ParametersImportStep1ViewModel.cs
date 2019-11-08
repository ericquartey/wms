using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersImportStep1ViewModel : BaseParametersImportExportViewModel
    {
        #region Fields

        private DelegateCommand restoreConfigurationCommand;

        #endregion

        #region Constructors

        public ParametersImportStep1ViewModel(IBayManager bayManager)
                : base(bayManager)
        {
        }

        #endregion

        #region Properties

        public string DeviceWithFileConfiguration
        {
            get
            {
                if (string.IsNullOrEmpty(this.ExistingPath))
                {
                    return string.Format(InstallationApp.InsertDeviceWithFileConfiguration, this.FileName);
                }

                return InstallationApp.DeviceFileFound;
            }
        }

        public ICommand RestoreConfigurationCommand =>
                    this.restoreConfigurationCommand
                    ??
                    (this.restoreConfigurationCommand = new DelegateCommand(
                    async () => await this.RestoreAsync(), this.CanRestore));

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

        public override void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.DeviceWithFileConfiguration));
            this.restoreConfigurationCommand.RaiseCanExecuteChanged();
        }

        private bool CanRestore()
        {
            return !this.IsBusy
                    &&
                   !string.IsNullOrEmpty(this.ExistingPath);
        }

        private async Task RestoreAsync()
        {
            try
            {
                this.IsBusy = true;

                var configuration = JsonConvert.DeserializeObject<VertimagConfiguration>(File.ReadAllText(this.ExistingPath), new JsonConverter[] { new IPAddressConverter() });

                if (configuration is null)
                {
                    this.ShowNotification(InstallationApp.FileReadError, Services.Models.NotificationSeverity.Error);
                    this.IsBusy = false;
                    return;
                }

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.Parameters.PARAMETERSIMPORTSTEP2,
                    configuration,
                    trackCurrentView: false);
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

        private async Task StopAsync()
        {
        }

        #endregion
    }
}
