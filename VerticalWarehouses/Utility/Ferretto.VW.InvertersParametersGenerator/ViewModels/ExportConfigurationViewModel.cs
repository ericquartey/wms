using System;
using System.Configuration;
using System.IO;
using System.Windows.Input;
using Ferretto.VW.InvertersParametersGenerator.Interfaces;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Properties;
using Ferretto.VW.InvertersParametersGenerator.Service;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    internal sealed class ExportConfigurationViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly ConfigurationService configurationService;

        private readonly IRaiseExecuteChanged parentRaiseExecuteChanged;

        private readonly string vertimagExportConfigurationPath;

        private RelayCommand exportCommand;

        private string resultVertimagConfiguration;

        private string vertimagConfigurationFilePath;

        #endregion

        #region Constructors

        public ExportConfigurationViewModel(ConfigurationService installationService, IRaiseExecuteChanged parentRaiseExecuteChanged)
        {
            this.configurationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
            this.parentRaiseExecuteChanged = parentRaiseExecuteChanged;

            this.VertimagConfigurationFilePath = this.vertimagExportConfigurationPath = ConfigurationManager.AppSettings.GetVertimagExportConfigurationRootPath();
            this.ResultVertimagConfiguration = "File not saved";
            this.parentRaiseExecuteChanged.RaiseCanExecuteChanged();
        }

        #endregion

        #region Properties

        public bool CanNext => false;

        public bool CanPrevious => true;

        public ICommand ExportCommand =>
                                        this.exportCommand
                        ??
                        (this.exportCommand = new RelayCommand(this.Export));

        public string ResultVertimagConfiguration
        {
            get => this.resultVertimagConfiguration;
            set => this.SetProperty(ref this.resultVertimagConfiguration, value);
        }

        public string VertimagConfigurationFilePath
        {
            get => this.vertimagConfigurationFilePath;
            set => this.SetProperty(ref this.vertimagConfigurationFilePath, value);
        }

        #endregion

        #region Methods

        public bool Next()
        {
            return true;
        }

        public void Previous()
        {
            this.configurationService.SetWizard(WizardMode.Parameters);
        }

        private void Export()
        {
            try
            {
                var resultFile = DialogService.SaveFile(Resources.SaveFileConfiguration, this.vertimagConfigurationFilePath, "json", Resources.ConfigurationFile, this.vertimagExportConfigurationPath);

                if (!string.IsNullOrEmpty(resultFile))
                {
                    var settings = new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver = new Models.OrderedContractResolver(),
                        Converters = new JsonConverter[]
                        {
                        new CommonUtils.Converters.IPAddressConverter(),
                        new Newtonsoft.Json.Converters.StringEnumConverter(),
                        },
                    };
                    var json = JsonConvert.SerializeObject(this.configurationService.VertimagConfiguration, settings);

                    File.WriteAllText(resultFile, json);
                    this.VertimagConfigurationFilePath = resultFile;
                    this.ResultVertimagConfiguration = null;
                    this.configurationService.ShowNotification(Resources.ExportedSuccessfully);
                }
                else
                {
                    this.ResultVertimagConfiguration = "file not valid or o not inserted";
                }
            }
            catch (Exception ex)
            {
                this.configurationService.ShowNotification(ex);
            }
        }

        #endregion
    }
}
