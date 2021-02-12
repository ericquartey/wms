using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ferretto.VW.InvertersParametersGenerator.Interfaces;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Properties;
using Ferretto.VW.InvertersParametersGenerator.Service;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    public class VertimagConfigurationViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly ConfigurationService configurationService;

        private readonly IParentActionChanged parentActionChanged;

        private bool canNext;

        private string invertersParametersFolder;

        private bool isSuccessful;

        private bool isVertimagConfigurationValid;

        private DelegateCommand openInvertersParametersFolderCommand;

        private DelegateCommand openVertimagConfigurationFileCommand;

        private string resultInvertersFolder;

        private string resultVertimagConfiguration;

        private VertimagConfiguration vertimagConfiguration;

        private string vertimagConfigurationFilePath;

        #endregion Fields

        #region Constructors

        public VertimagConfigurationViewModel(ConfigurationService configurationService, IParentActionChanged parentActionChanged)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.parentActionChanged = parentActionChanged;
            this.Inistialize();
        }

        #endregion Constructors

        #region Properties

        public bool CanNext => this.canNext;

        public bool CanPrevious => false;

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

        public ICommand OpenInvertersParametersFolderCommand =>
                        this.openInvertersParametersFolderCommand
                ??
                (this.openInvertersParametersFolderCommand = new DelegateCommand(this.OpenInvertersParametersFolder));

        public ICommand OpenVertimagConfigurationFileCommand =>
                       this.openVertimagConfigurationFileCommand
                ??
                 (this.openVertimagConfigurationFileCommand = new DelegateCommand(this.OpenVertimagConfigurationFile));

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

        #endregion Properties

        #region Methods

        public bool Next()
        {
            this.configurationService.SetConfiguration(this.invertersParametersFolder, this.vertimagConfiguration);
            this.configurationService.SetWizard(WizardMode.Inverters);
            return true;
        }

        public void OpenInvertersParametersFolder()
        {
            var parametersFolder = DialogService.BrowseFolder(Resources.InvertersFolder, ConfigurationManager.AppSettings.GetInvertersParametersRootPath());

            if (!string.IsNullOrEmpty(parametersFolder))
            {
                this.InvertersParametersFolder = parametersFolder;
            }

            this.EvaluateCanNext();
        }

        public void OpenVertimagConfigurationFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = Resources.ConfigurationFolder,

                CheckFileExists = true,

                DefaultExt = "json",
                Filter = "Json files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                this.VertimagConfigurationFilePath = openFileDialog.FileName;

                using (var sr = new StreamReader(this.VertimagConfigurationFilePath))
                {
                    var fileContents = sr.ReadToEnd();
                    this.isVertimagConfigurationValid = this.LoadConfiguration(fileContents);
                }
            }
            else
            {
                this.isVertimagConfigurationValid = false;
            }

            this.EvaluateCanNext();
        }

        public void Previous()
        {
            this.configurationService.SetWizard(WizardMode.None);
        }

        public void Save()
        {
            this.isSuccessful = true;
        }

        private bool CanPreviuos()
        {
            return false;
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
                this.ResultInvertersFolder = Resources.FoldersDoesNotExist;
            }

            this.canNext = this.isVertimagConfigurationValid;
            if (!this.isVertimagConfigurationValid)
            {
                this.ResultVertimagConfiguration = Resources.FileNotValidOrNotInserted;
            }

            this.canNext = isInvertersFolderValid && this.isVertimagConfigurationValid;

            this.RaiseCanExecuteChanged();
        }

        private void Inistialize()
        {
            this.InvertersParametersFolder = Environment.CurrentDirectory + "\\Parameters";
            this.EvaluateCanNext();

            this.ResultVertimagConfiguration = string.Empty;
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
            catch (Exception ex)
            {
                this.parentActionChanged.Notify(ex, NotificationSeverity.Error);
            }

            return false;
        }

        private void RaiseCanExecuteChanged()
        {
            this.parentActionChanged.RaiseCanExecuteChanged();
        }

        #endregion Methods
    }
}
