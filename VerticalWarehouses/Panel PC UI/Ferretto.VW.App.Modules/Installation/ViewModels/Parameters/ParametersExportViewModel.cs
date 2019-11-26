using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.ContractResolver;
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
        }

        private bool CanExport()
        {
            return !this.IsBusy
                   &&
                   !string.IsNullOrEmpty(this.FullPath);
        }

        private async Task ExportAsync()
        {
            try
            {
                if (File.Exists(this.FullPath))
                {
                    var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                    var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmFileOverwrite, InstallationApp.FileIsAlreadyPresent, DialogType.Question, DialogButtons.YesNo);
                    if (messageBoxResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                this.IsBusy = true;
                this.IsBackNavigationAllowed = false;

                this.RaisePropertyChanged();

                var configuration = await this.machineConfigurationWebService.GetAsync();

                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new FirstLetterPropertyNameResolver();
                settings.Converters.Add(new IPAddressConverter());
                settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

                File.WriteAllText(
                    this.FullPath,
                    JsonConvert.SerializeObject(configuration, settings));

                this.ShowNotification(Resources.InstallationApp.ExportSuccessful, Services.Models.NotificationSeverity.Success);
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
