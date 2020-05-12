using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Service;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Ferretto.VW.MAS.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    public class VertimagConfigurationViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly ConfigurationService configurationService;

        private bool canNext;

        private string invertersParametersFolder;

        private bool isSuccessful;

        private bool isVertimagConfigurationValid;

        private RelayCommand nextCommand;

        private RelayCommand openInvertersParametersFolderCommand;

        private RelayCommand openVertimagConfigurationFileCommand;

        private string resultInvertersFolder;

        private string resultVertimagConfiguration;

        private VertimagConfiguration vertimagConfiguration;

        private string vertimagConfigurationFilePath;

        #endregion

        #region Constructors

        public VertimagConfigurationViewModel(ConfigurationService configurationService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

            this.Inistialize();
        }

        #endregion

        #region Properties

        public virtual string DefaultExt { get; set; }

        public virtual string DefaultFileName { get; set; }

        public virtual string Filter { get; set; }

        public virtual int FilterIndex { get; set; }

        public string InvertersParametersFolder
        {
            get => this.invertersParametersFolder;
            set => this.SetProperty(ref this.invertersParametersFolder, value);
        }

        public bool IsSuccessful => this.isSuccessful;

        public ICommand NextCommand =>
                        this.nextCommand
                ??
                (this.nextCommand = new RelayCommand(this.Next, this.CanNext));

        public ICommand OpenInvertersParametersFolderCommand =>
                        this.openInvertersParametersFolderCommand
                ??
                (this.openInvertersParametersFolderCommand = new RelayCommand(this.OpenInvertersParametersFolder));

        public ICommand OpenVertimagConfigurationFileCommand =>
                                        this.openVertimagConfigurationFileCommand
                        ??
                        (this.openVertimagConfigurationFileCommand = new RelayCommand(this.OpenVertimagConfigurationFile));

        public virtual bool OverwritePrompt { get; set; }

        public string ResultInvertersFolder
        {
            get => this.resultInvertersFolder;
            set => this.SetProperty(ref this.resultInvertersFolder, value);
        }

        public string ResultVertimagConfiguration
        {
            get => this.resultVertimagConfiguration;
            set => this.SetProperty(ref this.resultVertimagConfiguration, value);
        }

        public virtual string Title { get; set; }

        public string VertimagConfigurationFilePath
        {
            get => this.vertimagConfigurationFilePath;
            set => this.SetProperty(ref this.vertimagConfigurationFilePath, value);
        }

        #endregion

        #region Methods

        public void OpenInvertersParametersFolder()
        {
            var parametersFolder = DialogService.BrowseFolder("Inverters parameters folder", ConfigurationManager.AppSettings.GetInvertersParametersRootPath());

            if (!string.IsNullOrEmpty(parametersFolder))
            {
                this.InvertersParametersFolder = parametersFolder;
            }

            this.EvaluateCanNext();
        }

        public void OpenVertimagConfigurationFile()
        {
            string[] resultFiles = DialogService.BrowseFile("Scegli file di configurazione", string.Empty, "json", "Cartella di configurazione");

            if ((resultFiles?.Length == 1) == false)
            {
                this.VertimagConfigurationFilePath = string.Empty;
                this.isVertimagConfigurationValid = false;
            }
            else
            {
                this.VertimagConfigurationFilePath = resultFiles.First();

                var sr = new StreamReader(this.VertimagConfigurationFilePath);
                var fileContents = sr.ReadToEnd();

                this.isVertimagConfigurationValid = this.LoadConfiguration(fileContents);
            }

            this.EvaluateCanNext();
        }

        public void Save()
        {
            this.isSuccessful = true;
        }

        private bool CanNext()
        {
            return this.canNext;
        }

        private void EvaluateCanNext()
        {
            this.canNext = false;

            this.ResultInvertersFolder = string.Empty;
            this.ResultVertimagConfiguration = string.Empty;
            var isInvertersFolderValid = true;

            if (!Directory.Exists(this.InvertersParametersFolder))
            {
                isInvertersFolderValid = false;
                this.ResultInvertersFolder = "Folders does not exist";
            }

            this.canNext = this.isVertimagConfigurationValid;
            if (!this.isVertimagConfigurationValid)
            {
                this.ResultVertimagConfiguration = "file not valid or o not inserted";
            }

            this.canNext = isInvertersFolderValid && this.isVertimagConfigurationValid;

            this.RaiseCanExecuteChanged();
        }

        private void Inistialize()
        {
            this.VertimagConfigurationFilePath = ConfigurationManager.AppSettings.GetVertimagConfigurationRootPath();
            this.InvertersParametersFolder = ConfigurationManager.AppSettings.GetInvertersParametersRootPath();
            this.EvaluateCanNext();
        }

        private bool LoadConfiguration(string configuration)
        {
            try
            {
                var jsonObject = JObject.Parse(configuration);

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new CommonUtils.Converters.IPAddressConverter());

                this.vertimagConfiguration = JsonConvert.DeserializeObject<MAS.DataModels.VertimagConfiguration>(jsonObject.ToString(), settings);

                return !(this.vertimagConfiguration is null);
            }
            catch
            {
            }

            return false;
        }

        private void Next()
        {
            this.configurationService.SetConfiguration(this.invertersParametersFolder, this.vertimagConfiguration);

            this.configurationService.SetWizard(WizardMode.Inverters);
        }

        private void RaiseCanExecuteChanged()
        {
            this.nextCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
