using System;
using System.IO;
using System.Windows.Input;
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

        private RelayCommand exportCommand;

        private string vertimagConfigurationFilePath;

        #endregion

        #region Constructors

        public ExportConfigurationViewModel(ConfigurationService installationService)
        {
            this.configurationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
        }

        #endregion

        #region Properties

        public ICommand ExportCommand =>
                        this.exportCommand
                        ??
                        (this.exportCommand = new RelayCommand(this.Export));

        public bool IsSuccessful => true;

        public string VertimagConfigurationFilePath
        {
            get => this.vertimagConfigurationFilePath;
            set => this.SetProperty(ref this.vertimagConfigurationFilePath, value);
        }

        #endregion

        #region Methods

        private void Export()
        {
            try
            {
                var resultFile = DialogService.SaveFile(Resources.SaveFileConfiguration, this.vertimagConfigurationFilePath, "json", Resources.ConfigurationFile);

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
                    this.configurationService.ShowNotification(Resources.ExportedSuccessfully);
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
