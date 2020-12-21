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
using Prism.Commands;
using Prism.Mvvm;
using Ferretto.VW.MAS.DataModels;
using System.Collections.Generic;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    internal sealed class ExportConfigurationViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private readonly ConfigurationService configurationService;

        private readonly IParentActionChanged parentActionChanged;

        private readonly string vertimagExportConfigurationPath;

        private DelegateCommand exportCommand;

        private DelegateCommand exportInverterCommand;

        private string resultVertimagConfiguration;

        private string resultVertimagInverterConfiguration;

        private string vertimagConfigurationFilePath;

        private string vertimagInverterConfigurationFilePath;

        #endregion Fields

        #region Constructors

        public ExportConfigurationViewModel(ConfigurationService installationService, IParentActionChanged parentActionChanged)
        {
            this.configurationService = installationService ?? throw new ArgumentNullException(nameof(installationService));
            this.parentActionChanged = parentActionChanged;

            this.VertimagInverterConfigurationFilePath = this.vertimagExportConfigurationPath = ConfigurationManager.AppSettings.GetVertimagExportConfigurationRootPath();
            this.VertimagConfigurationFilePath = this.vertimagExportConfigurationPath = ConfigurationManager.AppSettings.GetVertimagExportConfigurationRootPath();
            this.ResultVertimagConfiguration = Resources.FileNotSaved;
            this.ResultVertimagInverterConfiguration = Resources.FileNotSaved;
            this.parentActionChanged.RaiseCanExecuteChanged();
        }

        #endregion Constructors

        #region Properties

        public bool CanNext => false;

        public bool CanPrevious => true;

        public ICommand ExportCommand =>
                        this.exportCommand
                        ??
                        (this.exportCommand = new DelegateCommand(this.Export));

        public ICommand ExportInverterCommand =>
                        this.exportInverterCommand
                        ??
                        (this.exportInverterCommand = new DelegateCommand(this.ExportInverter));

        public string ResultVertimagConfiguration
        {
            get => this.resultVertimagConfiguration;
            set => this.SetProperty(ref this.resultVertimagConfiguration, value);
        }

        public string ResultVertimagInverterConfiguration
        {
            get => this.resultVertimagInverterConfiguration;
            set => this.SetProperty(ref this.resultVertimagInverterConfiguration, value);
        }

        public string VertimagConfigurationFilePath
        {
            get => this.vertimagConfigurationFilePath;
            set => this.SetProperty(ref this.vertimagConfigurationFilePath, value);
        }

        public string VertimagInverterConfigurationFilePath
        {
            get => this.vertimagInverterConfigurationFilePath;
            set => this.SetProperty(ref this.vertimagInverterConfigurationFilePath, value);
        }

        #endregion Properties

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
                    this.parentActionChanged.Notify(Resources.ExportedSuccessfully, NotificationSeverity.Success);
                }
                else
                {
                    this.ResultVertimagConfiguration = Resources.FileNotValidOrNotInserted;
                }
            }
            catch (Exception ex)
            {
                this.parentActionChanged.Notify(ex, NotificationSeverity.Error);
            }
        }

        private void ExportInverter()
        {
            try
            {
                var resultFile = DialogService.SaveFile(Resources.SaveFileConfiguration, this.vertimagInverterConfigurationFilePath, "json", Resources.ConfigurationFile, this.vertimagExportConfigurationPath);

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

                    var vertimagInverterConfiguration = this.ConvertConfiguration(this.configurationService.VertimagConfiguration);

                    var json = JsonConvert.SerializeObject(vertimagInverterConfiguration, settings);

                    File.WriteAllText(resultFile, json);
                    this.VertimagInverterConfigurationFilePath = resultFile;
                    this.ResultVertimagInverterConfiguration = null;
                    this.parentActionChanged.Notify(Resources.ExportedSuccessfully, NotificationSeverity.Success);
                }
                else
                {
                    this.ResultVertimagInverterConfiguration = Resources.FileNotValidOrNotInserted;
                }
            }
            catch (Exception ex)
            {
                this.parentActionChanged.Notify(ex, NotificationSeverity.Error);
            }
        }

        private List<Inverter> ConvertConfiguration(VertimagConfiguration vertimagConfiguration)
        {
            List<Inverter> inverters = new List<Inverter>();

            foreach (var axe in vertimagConfiguration.Machine.Elevator.Axes)
            {
                if (!(axe.Inverter is null))
                {
                    var inverter = axe.Inverter;
                    inverters.Add(inverter);
                }
            }

            foreach (var bay in vertimagConfiguration.Machine.Bays)
            {
                if (!(bay.Inverter?.Parameters is null))
                {
                    var inverter = bay.Inverter;

                    if (!(bay.Inverter is null))
                    {
                        inverters.Add(inverter);
                    }
                    if (!(bay.Shutter?.Inverter?.Parameters is null))
                    {
                        var inverterShutter = bay.Shutter.Inverter;

                        if (!(bay.Shutter.Inverter is null))
                        {
                            inverters.Add(inverterShutter);
                        }
                    }
                }
            }

            return inverters;
        }

        #endregion Methods
    }
}
