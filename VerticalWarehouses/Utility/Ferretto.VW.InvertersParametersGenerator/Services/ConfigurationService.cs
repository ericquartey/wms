using System;
using System.IO;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.ViewModels;
using Ferretto.VW.MAS.DataModels;
using Newtonsoft.Json;
using NLog;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.Services
{
    public sealed class ConfigurationService : BindableBase
    {
        #region Fields

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Newtonsoft.Json.Formatting.Indented
        };

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private string invertersParametersFolder;

        private VertimagConfiguration vertimagConfiguration;

        private WizardMode wizardMode;

        #endregion

        #region Constructors

        public ConfigurationService()
        {
        }

        #endregion

        #region Properties

        public static ConfigurationService GetInstance => new ConfigurationService();

        public VertimagConfiguration VertimagConfiguration => this.vertimagConfiguration;

        public WizardMode WizardMode
        {
            get => this.wizardMode;
            set => this.SetProperty(ref this.wizardMode, value);
        }

        #endregion

        #region Methods

        public void SaveVertimagConfiguration(string configurationFilePath, string fileContents)
        {
            try
            {
                File.WriteAllText(configurationFilePath, fileContents);
            }
            catch (Exception ex)
            {
                var msg = $" Error wrting configuration file \"{configurationFilePath}\"";
                this.logger.Error(ex, msg);
                throw new InvalidOperationException(msg);
            }
        }

        public void SetConfiguration(string invertersParametersFolder, VertimagConfiguration vertimagConfiguration)
        {
            this.invertersParametersFolder = invertersParametersFolder;
            this.vertimagConfiguration = vertimagConfiguration;
        }

        public void SetWizard(WizardMode nMode)
        {
            this.WizardMode = nMode;
        }

        public void ShowNotification(Exception ex)
        {
        }

        public void Start()
        {
            this.WizardMode = WizardMode.ImportConfiguration;
        }

        internal void Export()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
