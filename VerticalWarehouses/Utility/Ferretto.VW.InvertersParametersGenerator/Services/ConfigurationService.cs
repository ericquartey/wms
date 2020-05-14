using System;
using System.Collections.Generic;
using System.IO;
using Ferretto.VW.InvertersParametersGenerator.Models;
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

        private IEnumerable<InverterParametersDataInfo> invertersParameters;

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

        public IEnumerable<InverterParametersDataInfo> InvertersParameters => this.invertersParameters;

        public string InvertersParametersFolder => this.invertersParametersFolder;

        public VertimagConfiguration VertimagConfiguration => this.vertimagConfiguration;

        public WizardMode WizardMode
        {
            get => this.wizardMode;
            set => this.SetProperty(ref this.wizardMode, value);
        }

        #endregion

        #region Methods

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

        internal void ShowNotification(string info)
        {
        }

        #endregion
    }
}
