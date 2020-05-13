using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Messages.Data;
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

        private readonly ConfigurationService configurationService;

        private readonly bool isSuccessful;

        private RelayCommand nextCommand;

        private IEnumerable<FileInfo> configurationFiles = Array.Empty<FileInfo>();

        private DelegateCommand loadFileCommand = null;

        private bool isBusy = false;

        private FileInfo selectedFile = null;

        private readonly List<InverterParametersDataInfo> invertersParameters;

        private List<InverterParametersDataInfo> inverters;

        private InverterParametersDataInfo currentInverterParameters;

        private bool isParametersSet;

        private List<InverterParameter> inverterParameters;
        private string totalParameters;


        #endregion

        #region Constructors

        public SetParametersViewModel(ConfigurationService configurationService)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.SelectedFile = null;
            this.InitializeData();
        }

        private void InitializeData()
        {
            this.inverters = this.configurationService.InvertersParameters.ToList();
            this.currentInverterParameters = this.inverters.First();
            this.RaisePropertyChanged(nameof(this.Title));
            this.GetInverterParametersFiles();
            this.RaiseCanExecuteChanged();
        }

        public InverterParametersDataInfo CurrentInverterParameters => this.currentInverterParameters;

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        private void GetInverterParametersFiles()
        {
            this.IsBusy = true;
            try
            {
                //if (Double.IsNaN(this.configurationService.VertimagConfiguration.Machine.LoadUnitMaxNetWeight))
                //{
                //    throw new InvalidDataException($" Invalid data on {nameof(his.configurationService.VertimagConfiguration.Machine.LoadUnitMaxNetWeight)}");
                //}

                var sufix = this.IsOneTonMachine ? "800" : "1000";
                var files = $"{this.currentInverterParameters.Type.ToString().ToUpper()}*{sufix}*.txt";
                var di = new DirectoryInfo(this.configurationService.InvertersParametersFolder);
                this.configurationFiles = di.EnumerateFiles(files);

                this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
                if (!this.configurationFiles.Any())
                {
                    this.configurationService.ShowNotification(Resources.NoParametersFilesFound);
                }

            }
            catch (Exception ex)
            {
                this.configurationService.ShowNotification(ex);
                this.configurationFiles = null;
            }

            this.IsBusy = false;
        }

        public static IEnumerable<InverterParameterField> GetParametersFromFile(string filename)
        {
            try
            {

                //var inverterFileName = "ACU_giostra_800kg_INV20200305.txt";
                //var parmsDir = $"{Environment.CurrentDirectory}\\Parameters\\{filename}";
                var engine = new FileHelperEngine<InverterParameterField>();
                engine.ErrorManager.ErrorMode = ErrorMode.IgnoreAndContinue;
                return engine.ReadFileAsList(filename);
            }
            catch (Exception ex)
            {
                ConfigurationService.GetInstance.ShowNotification(ex);
            }

            return null;
        }


        private bool IsOneTonMachine
        {
            get
            {
                var elevatorInvertersCount = this.configurationService.VertimagConfiguration.Machine.Elevator.Axes
                    .Where(a => a.Inverter != null)
                    .Select(a => a.Inverter.Id)
                    .Distinct()
                    .Count();

                return elevatorInvertersCount > 1;
            }
        }

        #endregion

        #region Properties

        public bool IsParametersSet
        {
            get => this.isParametersSet;
            set => this.SetProperty(ref this.isParametersSet, value);
        }

        public string TotalParameters
        {
            get => this.totalParameters;
            set => this.SetProperty(ref this.totalParameters, value);
        }

        public IEnumerable<InverterParameter> InverterParameters => this.inverterParameters;

        public IEnumerable<InverterParametersData> Inverters => this.inverters;
        

        public bool IsSuccessful => this.isSuccessful;

        public ICommand NextCommand =>
                        this.nextCommand
                        ??
                        (this.nextCommand = new RelayCommand(this.Next, this.CanNext));


        public IEnumerable<FileInfo> ConfigurationFiles => this.configurationFiles;

        public ICommand LoadFileCommand =>
            this.loadFileCommand
            ??
            (this.loadFileCommand = new DelegateCommand(
            async () => await this.LoadFileAsync(), this.CanImport));

        private async Task LoadFileAsync()
        {
            //await Task.Run(() => this.LoadFile());
            this.LoadFile();
            this.RaiseCanExecuteChanged();
        }

        private void LoadFile()
        {
            this.IsBusy = true;
            try
            {
                this.inverterParameters = new List<InverterParameter>();

                var parametersInfo = LoadParametersList(this.currentInverterParameters.Type);
                var parameters = GetParametersFromFile(this.selectedFile.FullName);
                foreach (var parameter in parameters)
                {
                    if (string.IsNullOrEmpty(parameter.Writable))
                    {
                        continue;
                    }
                    var parameterInfo = parametersInfo.FirstOrDefault(pi => pi.Code == parameter.Code);
                    if (parameterInfo.IsReadOnly)
                    {
                        continue;
                    }

                    var inverterParameter = new InverterParameter
                    {
                        Code = this.GetCodeIndex(parameter),
                        DataSet = this.GetDatasetIndex(parameter),
                        Type = parameterInfo.Type,
                        Value = ExtractNumber(parameter.Value)
                    };

                    this.inverterParameters.Add(inverterParameter);

                this.RaisePropertyChanged(nameof(this.InverterParameters));
                }
            }
            catch (Exception ex)
            {
                this.configurationService.ShowNotification(ex);
            }

            this.TotalParameters = string.Format(Resources.TotalParameters, this.inverterParameters.Count);
            this.IsParametersSet = true;
            this.IsBusy =  false;
        }

        private short GetCodeIndex(InverterParameterField parameter)
        {
            if (short.TryParse(parameter.Code, out var code))
            {
                return code;
            }

            throw new InvalidDataException($"Invalid code '{parameter.Code} for parameter '{parameter.Description}'");
        }

        private int GetDatasetIndex(InverterParameterField parameter)
        {
            if (parameter.Dataset.Trim().ToLowerInvariant().Equals("[*]"))
            {
                return 0;
            }

            if (int.TryParse(ExtractNumber(parameter.Dataset), out var dataset))
            {
                return dataset;
            }

            throw new InvalidDataException($"Invalid dataset '{parameter.Dataset}' for parameter code '{parameter.Code}'");
            
        }

        public static string ExtractNumber(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var number = Regex.Match(source, @"\d+");
            return number != null ? number.Value : string.Empty;
        }

        private static IEnumerable<ParameterInfo> LoadParametersList(InverterType inverterType)
        {
            var parameters = new List<ParameterInfo>();

            var inverterFileName = $"Para_list_{inverterType.ToString().ToUpper()}.xlsx";
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

            return parameters;
        }

        private bool CanImport()
        {
            return this.selectedFile != null;
        }

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
        public string Title => string.Format(Resources.InverterTypeParametersConfiguration, this.currentInverterParameters.InverterIndex, this.currentInverterParameters.Type);

        #endregion

        #region Methods

        private void RaiseCanExecuteChanged()
        {
            this.loadFileCommand?.RaiseCanExecuteChanged();
            this.nextCommand?.RaiseCanExecuteChanged();
        }

        private bool CanNext()
        {
            return true;
        }

        private void Next()
        {
            this.SaveInverterParameters();

            if (!this.GotoNextInverterParametersSet())
            {
                this.configurationService.SetWizard(WizardMode.ExportConfiguration);
            }
        }

        private void SaveInverterParameters()
        {            
            var inverter = this.GetVertimagConfigurationByInverterId(this.currentInverterParameters.InverterIndex);
            inverter.Parameters = this.InverterParameters;
        }

        private Inverter GetVertimagConfigurationByInverterId(byte inverterIndex)
        {
            var vertimagConfiguration = this.configurationService.VertimagConfiguration;
            if (vertimagConfiguration.Machine.Elevator.Axes.FirstOrDefault(a => (byte)a.Inverter.Index == inverterIndex)?.Inverter is Inverter inverter)
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
            if (indexOf == this.inverters.Count() - 1)
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

        #endregion
    }
}
