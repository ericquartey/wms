using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.MAS.DataModels;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.Services
{
    public sealed class ConfigurationService : BindableBase
    {
        #region Fields

        private readonly InvertersNodeService invertersNodeService;

        private IEnumerable<InverterNode> invertersNode;

        private IEnumerable<InverterParametersDataInfo> invertersParameters;

        private string invertersParametersFolder;

        private VertimagConfiguration vertimagConfiguration;

        private WizardMode wizardMode;

        #endregion Fields

        #region Constructors

        public ConfigurationService()
        {
            this.invertersNodeService = new InvertersNodeService();
        }

        #endregion Constructors

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

        #endregion Properties

        #region Methods

        public void ConfigureInverterNode(byte inverterIndex, IEnumerable<InverterParameter> inverterParameters)
        {
            var nodeInverter = this.GetInverterNode(inverterIndex);
            if (nodeInverter is null)
            {
                throw new ArgumentNullException($"Inverter {inverterIndex} not found on Inverters node");
            }

            foreach (var nodeParameter in nodeInverter.Parameters)
            {
                if (inverterParameters.SingleOrDefault(p => p.Code == nodeParameter.Code) is InverterParameter inverterParameter)
                {
                    inverterParameter.StringValue = nodeParameter.Value;
                }
            }
        }

        public InverterNode GetInverterNode(byte inverterIndex)
        {
            return this.invertersNode.SingleOrDefault(i => i.InverterIndex == inverterIndex);
        }

        public void SetConfiguration(string invertersParametersFolder, VertimagConfiguration vertimagConfiguration)
        {
            this.invertersParametersFolder = invertersParametersFolder;
            this.vertimagConfiguration = vertimagConfiguration;
        }

        public void SetInvertersConfiguration(IEnumerable<InverterParametersDataInfo> invertersParameters)
        {
            this.invertersParameters = invertersParameters;
            this.invertersNode = this.invertersNodeService.BuildMachineInverterNode(this.invertersParameters);
        }

        public void SetWizard(WizardMode nMode)
        {
            this.WizardMode = nMode;
        }

        public void Start()
        {
            this.WizardMode = WizardMode.ImportConfiguration;
        }

        #endregion Methods
    }
}
