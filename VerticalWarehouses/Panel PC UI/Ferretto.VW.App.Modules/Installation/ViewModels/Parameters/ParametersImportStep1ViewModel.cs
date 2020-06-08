using System;
using System.IO;
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
                    return string.Format(Localized.Get("InstallationApp.InsertDeviceWithFileConfiguration"), this.FileName);
                }

                return Localized.Get("InstallationApp.DeviceFileFound");
            }
        }

        public ICommand RestoreConfigurationCommand =>
                    this.restoreConfigurationCommand
                    ??
                    (this.restoreConfigurationCommand = new DelegateCommand(
                    this.Restore, this.CanRestore));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
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

        private void Restore()
        {
            try
            {
                this.IsBusy = true;

                var configuration = JsonConvert.DeserializeObject<VertimagConfiguration>(File.ReadAllText(this.ExistingPath), new JsonConverter[] { new IPAddressConverter() });

                if (configuration is null)
                {
                    this.ShowNotification(Localized.Get("InstallationApp.FileReadError"), Services.Models.NotificationSeverity.Error);
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

        #endregion
    }
}
