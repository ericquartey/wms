using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.InvertersParametersGenerator.Interfaces;
using Ferretto.VW.InvertersParametersGenerator.Models;
using Ferretto.VW.InvertersParametersGenerator.Properties;
using Ferretto.VW.InvertersParametersGenerator.Services;
using Ferretto.VW.MAS.DataModels;
using FileHelpers;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    internal sealed class SetParametersViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private const string ACCESSREADONLY = "r_only";

        private const string DATASETZERO = "[*]";

        private const short MASTERINVERTERCODE = 924;

        private const short SLAVEINVERTERCODE = 925;

        private const string STRINGTYPE = "String";

        private readonly ConfigurationService configurationService;

        private readonly IParentActionChanged parentActionChanged;

        private IEnumerable<FileInfo> configurationFiles = Array.Empty<FileInfo>();

        private InverterParametersDataInfo currentInverterParameters;

        private List<InverterParameter> inverterParameters;

        private List<InverterParametersDataInfo> inverters;

        private bool isBusy = false;

        private bool isParametersSet;

        private DelegateCommand loadFileCommand = null;

        private Regex regexDataSet;

        private Regex regexDigit;

        private FileInfo selectedFile = null;

        private string totalParameters;

        #endregion

        #region Constructors

        public SetParametersViewModel(ConfigurationService configurationService, IParentActionChanged parentActionChanged)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.parentActionChanged = parentActionChanged;
            this.SelectedFile = null;
            this.InitializeData();
        }

        #endregion

        #region Properties

        public bool CanNext => this.isParametersSet;

        public bool CanPrevious => true;

        public IEnumerable<FileInfo> ConfigurationFiles => this.configurationFiles;

        public InverterParametersDataInfo CurrentInverterParameters => this.currentInverterParameters;

        public IEnumerable<InverterParameter> InverterParameters => this.inverterParameters;

        public IEnumerable<InverterParametersData> Inverters => this.inverters;

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsParametersSet
        {
            get => this.isParametersSet;
            set => this.SetProperty(ref this.isParametersSet, value);
        }

        public ICommand LoadFileCommand =>
            this.loadFileCommand
            ??
            (this.loadFileCommand = new DelegateCommand(
            this.LoadFile, this.CanImport));

        public FileInfo SelectedFile
        {
            get => this.selectedFile;
            set
            {
                if (this.SetProperty(ref this.selectedFile, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string Title => string.Format(Resources.InverterTypeParametersConfiguration, this.currentInverterParameters.InverterIndex, this.currentInverterParameters.Type.ToString());

        public string TotalParameters
        {
            get => this.totalParameters;
            set => this.SetProperty(ref this.totalParameters, value);
        }

        #endregion

        #region Methods

        public string ExtractDigit(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var match = this.regexDigit.Match(value);
            if (match.Success)
            {
                return match.Value.Replace(",", null);
            }

            return null;
        }

        public string ExtractValue(string type, string value)
        {
            if (type == STRINGTYPE)
            {
                return value;
            }

            return this.ExtractDigit(value);
        }

        public IEnumerable<InverterParameterField> GetParametersFromFile(string filename)
        {
            try
            {
                var engine = new FileHelperEngine<InverterParameterField>();
                engine.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;
                return engine.ReadFileAsList(filename);
            }
            catch (Exception ex)
            {
                this.parentActionChanged.Notify(ex, NotificationSeverity.Error);
            }

            return null;
        }

        public bool Next()
        {
            this.SaveInverterParameters();

            if (!this.GotoNextInverterParametersSet())
            {
                if (this.AdjustInvertConfigurationNodes())
                {
                    this.configurationService.SetWizard(WizardMode.ExportConfiguration);
                    return true;
                }
            }

            return false;
        }

        public void Previous()
        {
            this.configurationService.SetWizard(WizardMode.Inverters);
        }

        private static IEnumerable<ParameterInfo> LoadParametersList(InverterType inverterType)
        {
            var parameters = new List<ParameterInfo>();

            var inverterFileName = $"Para_list_{inverterType.ToString().ToUpper(CultureInfo.InvariantCulture)}.xlsx";
            var parmsDir = $"{Environment.CurrentDirectory}\\Parameters\\{inverterFileName}";
            var fileInfo = new FileInfo(parmsDir);

            using (var package = new ExcelPackage(fileInfo))
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.First();

                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;

                for (int row = start.Row + 3; row <= end.Row; row++)
                {
                    var code = worksheet.Cells[row, 1].Text;
                    var description = worksheet.Cells[row, 3].Text;
                    var type = worksheet.Cells[row, 4].Text;
                    var access = worksheet.Cells[row, 8].Text;
                    parameters.Add(new ParameterInfo(code, description, type, access == ACCESSREADONLY));
                }
            }

            return parameters;
        }

        private bool AdjustInvertConfigurationNodes()
        {
            if (this.configurationService.VertimagConfiguration.Machine.Bays.FirstOrDefault(b => b.Inverter != null) is Bay bay)
            {
                if (bay.Inverter.Parameters.FirstOrDefault(p => p.Code == SLAVEINVERTERCODE) is InverterParameter firstBayInverterParameter)
                {
                    if (this.configurationService.VertimagConfiguration.Machine.Elevator.Axes.FirstOrDefault(axe => axe.Inverter != null) is ElevatorAxis elevatorAxis)
                    {
                        if (elevatorAxis.Inverter.Parameters.FirstOrDefault(p => p.Code == MASTERINVERTERCODE) is InverterParameter mainInverterParameter)
                        {
                            mainInverterParameter.StringValue = firstBayInverterParameter.StringValue;
                            return true;
                        }
                    }
                }
            }

            throw new ArgumentException($"Set paramter on mainInverter for confifgure first node failed");
        }

        private bool CanImport()
        {
            return this.selectedFile != null;
        }

        private int GetDatasetIndex(InverterParameterField parameter)
        {
            if (string.IsNullOrEmpty(parameter.Dataset)
                ||
                parameter.Dataset.Trim().ToLowerInvariant().Equals(DATASETZERO))
            {
                return 0;
            }

            var match = this.regexDataSet.Match(parameter.Dataset);
            if (match.Success)
            {
                if (int.TryParse(match.Value, out var dataset))
                {
                    return dataset;
                }
            }

            throw new ArgumentException($"Invalid dataset '{parameter.Dataset}' for parameter code '{parameter.Code}'");
        }

        private void GetInverterParametersFiles()
        {
            this.IsBusy = true;
            try
            {
                var sufix = this.configurationService.VertimagConfiguration.Machine.LoadUnitMaxNetWeight;
                var files = $"{this.currentInverterParameters.Type.ToString().ToUpper(CultureInfo.InvariantCulture)}*{sufix}*.txt";
                var di = new DirectoryInfo(this.configurationService.InvertersParametersFolder);
                this.configurationFiles = di.EnumerateFiles(files);

                this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
                if (!this.configurationFiles.Any())
                {
                    this.parentActionChanged.Notify(Resources.NoParametersFilesFound, NotificationSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                this.parentActionChanged.Notify(ex, NotificationSeverity.Error);
                this.configurationFiles = null;
            }

            this.IsBusy = false;
        }

        private Inverter GetVertimagConfigurationByInverterId(byte inverterIndex)
        {
            var vertimagConfiguration = this.configurationService.VertimagConfiguration;
            if (vertimagConfiguration.Machine.Elevator.Axes.FirstOrDefault(a => (a.Inverter != null && (byte)a.Inverter.Index == inverterIndex))?.Inverter is Inverter inverter)
            {
                return inverter;
            }

            if (vertimagConfiguration.Machine.Bays.FirstOrDefault(a => (byte)a.Inverter.Index == inverterIndex)?.Inverter is Inverter inverterBay)
            {
                return inverterBay;
            }

            return null;
        }

        private bool GotoNextInverterParametersSet()
        {
            var indexOf = this.inverters.IndexOf(this.currentInverterParameters);
            if (indexOf == this.inverters.Count - 1)
            {
                return false;
            }

            this.currentInverterParameters = this.inverters[indexOf + 1];
            this.IsParametersSet = false;
            this.RaisePropertyChanged(nameof(this.Title));
            this.GetInverterParametersFiles();
            this.RaiseCanExecuteChanged();

            return true;
        }

        private void InitializeData()
        {
            this.regexDataSet = new Regex(@"(?<=\[).+?(?=\])", RegexOptions.Compiled);
            this.regexDigit = new Regex(@"^-?\d+(?:\,\d+)?", RegexOptions.Compiled);
            this.inverters = this.configurationService.InvertersParameters.ToList();
            this.currentInverterParameters = this.inverters.First();
            this.RaisePropertyChanged(nameof(this.Title));
            this.GetInverterParametersFiles();
            this.RaiseCanExecuteChanged();
        }

        private void LoadFile()
        {
            this.IsBusy = true;
            try
            {
                this.inverterParameters = new List<InverterParameter>();
                this.RaisePropertyChanged(nameof(this.InverterParameters));
                var lastParameterCode = string.Empty;
                var parametersInfo = LoadParametersList(this.currentInverterParameters.Type);
                var parameters = this.GetParametersFromFile(this.selectedFile.FullName);
                foreach (var parameter in parameters)
                {
                    var code = Regex.Replace(parameter.Code, @"^0+(?!$)", string.Empty);
                    if (string.IsNullOrEmpty(code))
                    {
                        code = lastParameterCode;
                    }
                    else
                    {
                        lastParameterCode = code;
                    }

                    var parameterInfo = parametersInfo.FirstOrDefault(pi => pi.Code == code);
                    if (parameterInfo is null)
                    {
                        throw new ArgumentException($"Parameter code '{code}' not found on parameters list for inverter type {this.currentInverterParameters.Type}");
                    }

                    if (string.IsNullOrEmpty(parameter.Writable))
                    {
                        continue;
                    }
                    else
                    {
                        if (parameterInfo.IsReadOnly)
                        {
                            throw new ArgumentException($"Parameter code '{code}' is writable but on parameters list is Not writable, case inverter type {this.currentInverterParameters.Type}");
                        }
                    }

                    if (parameterInfo.IsReadOnly)
                    {
                        continue;
                    }

                    var inverterParameter = new InverterParameter
                    {
                        Code = short.Parse(code),
                        DataSet = this.GetDatasetIndex(parameter),
                        Type = parameterInfo.Type,
                        StringValue = this.ExtractValue(parameterInfo.Type, parameter.Value)
                    };

                    this.inverterParameters.Add(inverterParameter);

                    this.RaisePropertyChanged(nameof(this.InverterParameters));
                    this.IsParametersSet = true;
                }
            }
            catch (Exception ex)
            {
                this.IsParametersSet = false;
                this.SelectedFile = null;
                this.parentActionChanged.Notify(ex, NotificationSeverity.Error);
            }

            this.TotalParameters = string.Format(Resources.TotalParameters, this.inverterParameters.Count);
            this.IsBusy = false;
            this.RaiseCanExecuteChanged();
        }

        private void RaiseCanExecuteChanged()
        {
            this.loadFileCommand?.RaiseCanExecuteChanged();
            this.parentActionChanged.RaiseCanExecuteChanged();
        }

        private void SaveInverterParameters()
        {
            var inverter = this.GetVertimagConfigurationByInverterId(this.currentInverterParameters.InverterIndex);
            inverter.Parameters = this.InverterParameters;
        }

        #endregion
    }
}
