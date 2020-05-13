using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.MAS.DataModels;
using FileHelpers;
using Newtonsoft.Json;
using NLog;
using OfficeOpenXml;
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

        private IEnumerable<InverterParametersDataInfo> invertersParameters;

        #endregion

        #region Constructors

        public ConfigurationService()
        {
        }

        #endregion

        #region Properties

        public static ConfigurationService GetInstance => new ConfigurationService();

        public VertimagConfiguration VertimagConfiguration => this.vertimagConfiguration;
        public string InvertersParametersFolder => this.invertersParametersFolder;
        

        public IEnumerable<InverterParametersDataInfo> InvertersParameters => this.invertersParameters;

        public WizardMode WizardMode
        {
            get => this.wizardMode;
            set => this.SetProperty(ref this.wizardMode, value);
        }

        #endregion

        #region Methods


   
        internal void ShowNotification(string info)
        {
            
        }

 



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

        public void SetInvertersConfiguration(IEnumerable<InverterParametersDataInfo> invertersParameters)
        {
            this.invertersParameters = invertersParameters;
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
