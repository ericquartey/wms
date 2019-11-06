using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Newtonsoft.Json;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersImportStep1ViewModel : BaseParametersImportExportViewModel
    {
        #region Fields

        private DelegateCommand restoreConfigurationCommand;

        private DelegateCommand stopCommand;

        private string workingFolder;

        #endregion

        #region Constructors

        public ParametersImportStep1ViewModel()
        {
        }

        #endregion

        #region Properties

        public ICommand RestoreConfigurationCommand =>
            this.restoreConfigurationCommand
                    ??
                    (this.restoreConfigurationCommand = new DelegateCommand(
                    async () => await this.RestoreAsync(), this.CanRestore));

        public ICommand StopCommand =>
                    this.stopCommand
                   ??
                   (this.stopCommand = new DelegateCommand(
                       async () => await this.StopAsync(), this.CanStop));

        public string WorkingFolder
        {
            get => this.workingFolder;
            set => this.SetProperty(ref this.workingFolder, value);
        }

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

        private bool CanRestore()
        {
            return !this.IsBusy
                    &&
                    !string.IsNullOrEmpty(this.WorkingFolder);
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private bool CanStop()
        {
            return this.IsBusy;
        }

        private async Task RestoreAsync()
        {
            try
            {
                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                var configuration = JsonConvert.DeserializeObject<VertimagConfiguration>(File.ReadAllText(this.WorkingFolder));

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
                this.IsBackNavigationAllowed = true;
            }
        }

        private async Task StopAsync()
        {
        }

        #endregion
    }
}
