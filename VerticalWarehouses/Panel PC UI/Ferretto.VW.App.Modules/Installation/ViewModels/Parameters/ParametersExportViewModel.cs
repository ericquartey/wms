using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class FirstLetterPropertyNameResolver : CamelCasePropertyNamesContractResolver
    {
        #region Methods

        public static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.GetCustomAttribute<JsonPropertyAttribute>() is JsonPropertyAttribute jsonProperty)
            {
                property.PropertyName = FirstCharToUpper(jsonProperty.PropertyName);
            }

            return property;
        }

        #endregion
    }

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

                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new FirstLetterPropertyNameResolver();
                settings.Converters.Add(new IPAddressConverter());
                settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

                File.WriteAllText(
                    this.FullPath,
                    JsonConvert.SerializeObject(configuration, settings));

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

        #endregion
    }
}
