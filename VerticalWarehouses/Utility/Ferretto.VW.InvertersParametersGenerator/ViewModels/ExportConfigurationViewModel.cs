using System;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
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

        private readonly bool isSuccessful;

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

        public bool IsSuccessful => this.isSuccessful;

        public string VertimagConfigurationFilePath
        {
            get => this.vertimagConfigurationFilePath;
            set => this.SetProperty(ref this.vertimagConfigurationFilePath, value);
        }

        #endregion

        #region Methods

        private void Export()
        {
            string[] resultFiles = DialogService.BrowseFile("Scegli file di configurazione", string.Empty, "json", "Cartella di configurazione");

            if ((resultFiles?.Length == 1) == false)
            {
                this.VertimagConfigurationFilePath = string.Empty;
            }
            else
            {
                this.VertimagConfigurationFilePath = resultFiles.First();

                string json = JsonConvert.SerializeObject(this.configurationService.VertimagConfiguration, Formatting.Indented);
                File.WriteAllText(this.VertimagConfigurationFilePath, json);
            }
        }

        #endregion
    }
}
