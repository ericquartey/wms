using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static IEnumerable<InverterParameterField> ReadTextFile(string filename)
        {
            try
            {
                //var inverterFileName = "ACU_giostra_800kg_INV20200305.txt";
                var parmsDir = $"{Environment.CurrentDirectory}\\Parameters\\{filename}";

                var engine = new FileHelperEngine<InverterParameterField>();
                engine.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;
                return engine.ReadFileAsList(parmsDir);
            }
            catch (Exception ex)
            {
                ConfigurationService.GetInstance.ShowNotification(ex);
            }

            return null;
        }

        public bool IsOneTonMachine()
        {
            var elevatorInvertersCount = this.vertimagConfiguration.Machine.Elevator.Axes
                .Where(a => a.Inverter != null)
                .Select(a => a.Inverter.Id)
                .Distinct()
                .Count();

            return elevatorInvertersCount > 1;
        }

        public IEnumerable<ParameterInfo> LoadParametersList()
        {
            var parameters = new List<ParameterInfo>();
            try
            {
                var inverterFileName = "Para_list_ACU.xlsx";
                var parmsDir = $"{Environment.CurrentDirectory}\\Parameters\\{inverterFileName}";
                var fileInfo = new FileInfo(parmsDir);

                using (var package = new ExcelPackage(fileInfo))
                {
                    var workbook = package.Workbook;
                    var worksheet = workbook.Worksheets.First();

                    var start = worksheet.Dimension.Start;
                    var end = worksheet.Dimension.End;

                    var list = new List<ParameterInfo>();

                    for (int row = start.Row + 3; row <= end.Row; row++)
                    {
                        var code = worksheet.Cells[row, 1].Text;
                        var description = worksheet.Cells[row, 3].Text;
                        var type = worksheet.Cells[row, 4].Text;
                        var access = worksheet.Cells[row, 8].Text;
                        list.Add(new ParameterInfo(code, description, type, access == "r_only"));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }

            return parameters;
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
