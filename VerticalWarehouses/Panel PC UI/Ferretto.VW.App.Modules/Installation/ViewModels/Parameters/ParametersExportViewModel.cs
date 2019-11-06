using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersExportViewModel : BaseParametersImportExportViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private DelegateCommand exportCommand;

        private DelegateCommand stopCommand;

        #endregion

        #region Constructors

        public ParametersExportViewModel(IMachineConfigurationWebService machineConfigurationWebService)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public ICommand ExportCommand =>
            this.exportCommand
                    ??
                    (this.exportCommand = new DelegateCommand(
                    async () => await this.ExportAsync(), this.CanExport));

        public ICommand StopCommand =>
                    this.stopCommand
                   ??
                   (this.stopCommand = new DelegateCommand(
                       async () => await this.StopAsync(), this.CanStop));

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

                this.IsBackNavigationAllowed = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                //this.IsBusy = false;
            }
        }

        public override void RaisePropertyChanged()
        {
            ((DelegateCommand)this.exportCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.stopCommand).RaiseCanExecuteChanged();
        }

        private bool CanExport()
        {
            return !this.IsBusy
                   &&
                   this.Data != null
                   &&
                   !string.IsNullOrEmpty(this.WorkingFolder);
        }

        private bool CanStop()
        {
            return this.IsBusy
                   &&
                   this.Data != null
                   &&
                   !string.IsNullOrEmpty(this.WorkingFolder);
        }

        private async Task ExportAsync()
        {
            try
            {
                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                this.RaisePropertyChanged();

                var configuration = await this.machineConfigurationWebService.GetAsync();

                using (StreamWriter file = File.CreateText(
                    this.WorkingFolder))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, configuration, typeof(VertimagConfiguration));
                }

                this.ShowNotification(Resources.InstallationApp.RestoreSuccessful);

                // TO DO save configuration
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

        private Task StopAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
