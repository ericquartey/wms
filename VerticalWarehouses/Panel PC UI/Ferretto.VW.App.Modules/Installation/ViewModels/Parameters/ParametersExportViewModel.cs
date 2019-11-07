using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Converters;
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

        public ParametersExportViewModel(IMachineConfigurationWebService machineConfigurationWebService, IBayManager bayManager)
                : base(bayManager)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public string DeviceInfo
        {
            get
            {
                if (this.IsDeviceReady)
                {
                    return InstallationApp.DeviceFound;
                }

                return InstallationApp.DeviceNotFound;
            }
        }

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

        public override void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.DeviceInfo));
            ((DelegateCommand)this.exportCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)this.stopCommand).RaiseCanExecuteChanged();
        }

        private bool CanExport()
        {
            return !this.IsBusy
                   &&
                   !string.IsNullOrEmpty(this.FullPath);
        }

        private bool CanStop()
        {
            return this.IsBusy
                   &&
                   !string.IsNullOrEmpty(this.FullPath);
        }

        private async Task ExportAsync()
        {
            try
            {
                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmFileOverwrite, InstallationApp.FileIsAlreadyPresent, DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult != DialogResult.Yes)
                {
                    return;
                }

                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                this.RaisePropertyChanged();

                var configuration = await this.machineConfigurationWebService.GetAsync();

                File.WriteAllText(
                    this.FullPath,
                    JsonConvert.SerializeObject(configuration, new JsonConverter[] { new IPAddressConverter() }));

                this.ShowNotification(Resources.InstallationApp.ExportSuccessful);
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
