﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Ferretto.VW.MAS.InverterDriver.Contracts;
using FileHelpers;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    internal sealed class SetParametersViewModel : BindableBase, IOperationResult
    {
        #region Fields

        private const string STRINGTYPE = "string";

        private const string INTTYPE = "int";

        private const string SHORTTYPE = "short";

        private const string USHORTTYPE = "ushort";

        private const short NODEID = 900;

        private const short BAUNDRATE = 903;

        private const short VABUS_BAUNDRATE = 10;

        public static readonly IList<short> parameterToIgnore = new ReadOnlyCollection<short>(new List<short> { 1202, 1203, 1204, 1206, 1352, 1399 });

        public static readonly IList<short> parameterToIgnoreAGL = new ReadOnlyCollection<short>(new List<short> { 371, 376, 443, 531, 613, 616, 623, 631, 654, 781, 1389, 1503, 1510, 1511, 1520, 1534, 1542, 1543, 1550, 1551, 1552 });

        private readonly ConfigurationService configurationService;

        private readonly IParentActionChanged parentActionChanged;

        private InverterParametersDataInfo currentInverterParameters;

        private List<InverterParameter> inverterParameters;

        private List<InverterParametersDataInfo> inverters;

        private bool isBusy = false;

        private bool isParametersSet;

        private string pattern;

        private Regex regexDigit;

        private string totalParameters;

        private IEnumerable<FileInfo> configurationFiles = new List<FileInfo>();

        private DelegateCommand loadFileCommand;

        private FileInfo selectedFile;

        #endregion Fields

        #region Constructors

        public SetParametersViewModel(ConfigurationService configurationService, IParentActionChanged parentActionChanged)
        {
            this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            this.parentActionChanged = parentActionChanged;
            this.InitializeData();
        }

        #endregion Constructors

        #region Properties

        public bool CanNext => this.isParametersSet;

        public bool CanPrevious => true;

        public IEnumerable<FileInfo> ConfigurationFiles => this.configurationFiles;

        public InverterParametersDataInfo CurrentInverterParameters => this.currentInverterParameters;

        public IEnumerable<InverterParameter> InverterParameters => this.inverterParameters;

        public IEnumerable<InverterParametersData> Inverters => this.inverters;

        public ICommand LoadFileCommand =>
            this.loadFileCommand
            ??
            (this.loadFileCommand = new DelegateCommand(
            this.LoadParameters, this.CanImport));

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

        public string Pattern
        {
            get => this.pattern;
            set => this.SetProperty(ref this.pattern, value);
        }

        public string Title => string.Format(Resources.InverterTypeParametersConfiguration, this.currentInverterParameters.InverterIndex, this.currentInverterParameters.Type.ToString());

        public string TotalParameters
        {
            get => this.totalParameters;
            set => this.SetProperty(ref this.totalParameters, value);
        }

        #endregion Properties

        #region Methods

        private bool CanImport()
        {
            return this.selectedFile != null;
        }

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

        public bool Next()
        {
            this.SaveInverterParameters();

            if (!this.GotoNextInverterParametersSet())
            {
                this.configurationService.SetWizard(WizardMode.ExportConfiguration);
                return true;
            }

            return false;
        }

        private void GetInverterParametersFiles()
        {
            this.IsBusy = true;
            try
            {
                //var sufix = this.configurationService.VertimagConfiguration.Machine.LoadUnitMaxNetWeight;
                //this.Pattern = $"{this.currentInverterParameters.Type.ToString().ToUpper(CultureInfo.InvariantCulture)}*{sufix}*.vcb";
                this.Pattern = $"*{this.currentInverterParameters.Type.ToString().ToUpper(CultureInfo.InvariantCulture)}*.vcb";
                var di = new DirectoryInfo(this.configurationService.InvertersParametersFolder);
                this.configurationFiles = di.EnumerateFiles(this.Pattern);

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

        public void Previous()
        {
            this.configurationService.SetWizard(WizardMode.Inverters);
        }

        private Inverter GetVertimagConfigurationByInverterId(byte inverterIndex)
        {
            var vertimagConfiguration = this.configurationService.VertimagConfiguration;
            if (vertimagConfiguration.Machine.Elevator.Axes.SingleOrDefault(a => (a.Inverter != null && (byte)a.Inverter.Index == inverterIndex))?.Inverter is Inverter inverter)
            {
                return inverter;
            }

            if (vertimagConfiguration.Machine.Bays.SingleOrDefault(a => (a.Inverter != null && (byte)a.Inverter.Index == inverterIndex))?.Inverter is Inverter inverterBay)
            {
                return inverterBay;
            }

            if (vertimagConfiguration.Machine.Bays.SingleOrDefault(a => (a.Shutter.Inverter != null && (byte)a.Shutter.Inverter.Index == inverterIndex))?.Shutter?.Inverter is Inverter inverterShutter)
            {
                return inverterShutter;
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
            this.regexDigit = new Regex(@"^-?\d+(?:\,\d+)?", RegexOptions.Compiled);
            this.inverters = this.configurationService.InvertersParameters.ToList();
            this.currentInverterParameters = this.inverters.First();
            this.RaisePropertyChanged(nameof(this.Title));
            this.GetInverterParametersFiles();
            this.RaiseCanExecuteChanged();
        }

        private void LoadParameters()
        {
            this.IsBusy = true;
            try
            {
                var inverterParameters = this.GetParameter(this.currentInverterParameters.Type, this.selectedFile.FullName).OrderBy(s => s.Code);

                this.inverterParameters = inverterParameters.OrderBy(i => i.Code).ToList();

                this.RaisePropertyChanged(nameof(this.InverterParameters));
                this.IsParametersSet = true;
            }
            catch (Exception ex)
            {
                this.IsParametersSet = false;
                this.parentActionChanged.Notify(ex, NotificationSeverity.Error);
            }

            this.TotalParameters = string.Format(Resources.TotalParameters, this.inverterParameters.Count);
            this.IsBusy = false;
            this.RaiseCanExecuteChanged();
        }

        private List<InverterParameter> GetParameter(InverterType inverterType, string path)
        {
            var file = new StreamReader(path);

            string line;

            var parameters = new List<InverterParameter>();
            var parametersInfo = new List<ParameterInfo>();

            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains("Parameter = "))
                {
                    var clean = line.Remove(0, 13);
                    clean = clean.Remove(clean.Length - 1, 1);
                    var split = clean.Split(';');

                    var code = default(short);

                    if (char.IsLetter(split[0].FirstOrDefault()))
                    {
                        var hex = short.Parse(split[0].Substring(0, 1), System.Globalization.NumberStyles.HexNumber).ToString();
                        code = short.Parse(hex + split[0].Substring(1, 2));
                    }
                    else
                    {
                        code = short.Parse(split[0].Substring(0, 3));
                    }

                    var desc = split[1];

                    var um = split[2];

                    //var min = split[0].Substring(3, 8); //min parameter value
                    //var max = split[0].Substring(11, 8); //max parameter value
                    //var default = split[0].Substring(19, 8); //default parameter value

                    var type0 = default(int);//0 dword, 1 word, 2 read, 3 write
                    if (char.IsLetter(Convert.ToChar(split[0].Substring(27, 1))))
                    {
                        type0 = int.Parse(split[0].Substring(27, 1), System.Globalization.NumberStyles.HexNumber);
                    }
                    else
                    {
                        type0 = int.Parse(split[0].Substring(27, 1));
                    }

                    var data = ConvertBit0(type0);

                    var type1 = default(int);//0 special, 1 dataset, 2 unsigned, 3 string
                    if (char.IsLetter(Convert.ToChar(split[0].Substring(28, 1))))
                    {
                        type1 = int.Parse(split[0].Substring(28, 1), System.Globalization.NumberStyles.HexNumber);
                    }
                    else
                    {
                        type1 = int.Parse(split[0].Substring(28, 1));
                    }

                    var type = ConvertBit1(type1);

                    var type2 = default(int);//3 index, 2 internal use
                    if (char.IsLetter(Convert.ToChar(split[0].Substring(27, 1))))
                    {
                        type2 = int.Parse(split[0].Substring(29, 1), System.Globalization.NumberStyles.HexNumber);
                    }
                    else
                    {
                        type2 = int.Parse(split[0].Substring(29, 1));
                    }

                    if (type2 == 4)
                    {
                        //internal use
                        continue;
                    }

                    //var type3 = int.Parse(split[0].Substring(30, 1)); //unused

                    if (!string.IsNullOrEmpty(data.type) &&
                        string.IsNullOrEmpty(type))
                    {
                        type = data.type;
                    }

                    //force readonly
                    if (code == NODEID ||
                        code == BAUNDRATE ||
                        code == VABUS_BAUNDRATE)
                    {
                        data.isReadonly = true;
                    }

                    //skip parameter
                    if (parameterToIgnore.Any(s => s == code))
                    {
                        continue;
                    }

                    //skip current parameters
                    if (inverterType == InverterType.Agl && parameterToIgnoreAGL.Any(s => s == code))
                    {
                        continue;
                    }

                    var inverterVersionParameter = new ParameterInfo(code, desc, type, um, data.isReadonly);

                    parametersInfo.Add(inverterVersionParameter);
                }
                else if (line.Contains("Value = "))
                {
                    var clean = line.Remove(0, 8);
                    var split = clean.Split(',', 3);

                    var code = default(short);

                    if (char.IsLetter(split[0].FirstOrDefault()))
                    {
                        var hex = short.Parse(split[0].Substring(0, 1), NumberStyles.HexNumber).ToString();
                        code = short.Parse(hex + split[0].Substring(1, 2));
                    }
                    else
                    {
                        code = short.Parse(split[0]);
                    }

                    if (!parametersInfo.Any(s => s.Code == code))
                    {
                        continue;
                    }

                    var dataset = int.Parse(split[1]);

                    var value = default(string);
                    if (split[2].Contains('"'))
                    {
                        value = split[2].Remove(0, 2);
                        value = value.Remove(value.Length - 1, 1);
                    }
                    else
                    {
                        value = split[2].Remove(0, 1);
                    }

                    var parameter = parametersInfo.Single(s => s.Code == code);
                    var decimalCount = default(int);

                    if (parameter.Type != STRINGTYPE
                        && value.Contains(','))
                    {
                        var decimalSplit = value.Split(",");
                        var result = this.ExtractValue(parameter.Type, decimalSplit[1]);
                        if (!string.IsNullOrEmpty(result))
                        {
                            decimalCount = result.Length;
                            value = FixDecimalValue(this.ExtractValue(parameter.Type, value), decimalCount);
                        }
                    }

                    short writeCode = default(short);
                    short readCode = default(short);

                    if (inverterType == InverterType.Ang)
                    {
                        writeCode = GetANGWriteReadCode(parameter.Code).writeCode;
                        readCode = GetANGWriteReadCode(parameter.Code).readCode;
                    }
                    else if (inverterType == InverterType.Agl)
                    {
                        writeCode = GetAGLWriteReadCode(parameter.Code).writeCode;
                        readCode = GetAGLWriteReadCode(parameter.Code).readCode;
                    }
                    else if (inverterType == InverterType.Acu)
                    {
                        writeCode = GetACUWriteReadCode(parameter.Code).writeCode;
                        readCode = GetACUWriteReadCode(parameter.Code).readCode;
                    }

                    var newPara = new InverterParameter
                    {
                        Code = parameter.Code,
                        IsReadOnly = parameter.IsReadOnly,
                        Description = parameter.Description,
                        Type = parameter.Type,
                        StringValue = this.ExtractValue(parameter.Type, value),
                        DataSet = dataset,
                        DecimalCount = decimalCount,
                        ReadCode = readCode,
                        WriteCode = writeCode,
                        Um = parameter.Um
                    };

                    parameters.Add(newPara);
                }
                else if (line.Contains("IndexParam_"))
                {
                    var clean = line.Remove(0, 11);
                    var split = clean.Split(new char[] { '_', '=' }, 3);

                    var code = default(short);

                    if (char.IsLetter(split[0].FirstOrDefault()))
                    {
                        var hex = short.Parse(split[0].Substring(0, 1), NumberStyles.HexNumber).ToString();
                        code = short.Parse(hex + split[0].Substring(1, 2));
                    }
                    else
                    {
                        code = short.Parse(split[0]);
                    }

                    var dataset = int.Parse(split[1]);

                    if (!parametersInfo.Any(s => s.Code == code))
                    {
                        continue;
                    }
                    else if (parameters.Any(s => s.Code == code && s.DataSet == dataset))
                    {
                        continue;
                    }

                    var parameter = parameters.FirstOrDefault(s => s.Code == code);

                    var value = default(string);

                    if (parameter.Type == STRINGTYPE)
                    {
                        value = split[2];
                    }
                    else if (parameter.Type == INTTYPE)
                    {
                        var hexValue = int.Parse(split[2], System.Globalization.NumberStyles.HexNumber).ToString();
                        value = FixDecimalValue(hexValue, parameter.DecimalCount);
                    }
                    else if (parameter.Type == USHORTTYPE)
                    {
                        var hexValue = ushort.Parse(split[2], System.Globalization.NumberStyles.HexNumber).ToString();
                        value = FixDecimalValue(hexValue, parameter.DecimalCount);
                    }
                    else if (parameter.Type == SHORTTYPE)
                    {
                        var hexValue = short.Parse(split[2], System.Globalization.NumberStyles.HexNumber).ToString();
                        value = FixDecimalValue(hexValue, parameter.DecimalCount);
                    }

                    var newPara = new InverterParameter
                    {
                        Code = parameter.Code,
                        IsReadOnly = parameter.IsReadOnly,
                        Description = parameter.Description,
                        Type = parameter.Type,
                        StringValue = this.ExtractValue(parameter.Type, value),
                        DataSet = dataset,
                        DecimalCount = parameter.DecimalCount,
                        ReadCode = parameter.ReadCode,
                        WriteCode = parameter.WriteCode,
                        Um = parameter.Um
                    };

                    parameters.Add(newPara);

                    if (parameters.Any(s => s.Code == parameter.Code && s.DataSet == 0) &&
                        dataset != 0)
                    {
                        var parameterToRemove = parameters.SingleOrDefault(s => s.Code == code && s.DataSet == 0);
                        parameters.Remove(parameterToRemove);
                    }
                }
            }

            return parameters;
        }

        private static (short readCode, short writeCode) GetANGWriteReadCode(short code)
        {
            switch (code)
            {
                case 1202:
                case 1203:
                case 1204:
                case 1205:
                case 1206:
                case 1207:
                case 1208:
                case 1209:
                case 1210:
                case 1211:
                case 1212:
                case 1213:
                case 1214:
                case 1215:
                case 1216:
                case 1217:
                case 1218:
                case 1219:
                    return (1201, 1200);

                case 1247:
                case 1248:
                    return (1201, 1200);

                case 1252:
                case 1253:
                    return (1251, 1250);

                case 1260:
                case 1261:
                case 1262:
                case 1263:
                case 1264:
                case 1265:
                    return (1201, 1200);

                case 1343:
                case 1344:
                case 1345:
                case 1346:
                case 1347:
                case 1348:
                case 1349:
                case 1350:
                case 1351:
                case 1352:
                    return (1342, 1341);

                case 1362:
                    return (1361, 1360);

                case 1379:
                case 1380:
                case 1381:
                case 1382:
                case 1383:
                case 1384:
                case 1385:
                case 1386:
                case 1387:
                case 1388:
                case 1389:
                case 1390:
                case 1391:
                case 1392:
                case 1393:
                case 1394:
                case 1395:
                case 1396:
                case 1397:
                    return (1378, 1377);

                case 1422:
                    return (1421, 1420);

                case 1429:
                case 1430:
                    return (1428, 1427);

                default:
                    return (0, 0);
            }
        }

        private static (short readCode, short writeCode) GetACUWriteReadCode(short code)
        {
            switch (code)
            {
                case 1202:
                case 1203:
                case 1204:
                case 1205:
                case 1206:
                case 1207:
                case 1208:
                case 1209:
                case 1210:
                case 1211:
                case 1212:
                case 1213:
                case 1214:
                case 1215:
                case 1216:
                case 1217:
                case 1218:
                case 1219:
                    return (1201, 1200);

                case 1247:
                case 1248:
                    return (1201, 1200);

                case 1252:
                case 1253:
                    return (1251, 1250);

                case 1260:
                case 1261:
                case 1262:
                case 1263:
                case 1264:
                case 1265:
                    return (1201, 1200);

                case 1343:
                case 1344:
                case 1345:
                case 1346:
                case 1347:
                case 1348:
                case 1349:
                case 1350:
                case 1351:
                case 1352:
                    return (1342, 1341);

                case 1362:
                    return (1361, 1360);

                case 1379:
                case 1380:
                case 1381:
                case 1382:
                case 1383:
                case 1384:
                case 1385:
                case 1386:
                case 1387:
                case 1388:
                case 1389:
                case 1390:
                case 1391:
                case 1392:
                case 1393:
                case 1394:
                case 1395:
                case 1396:
                case 1397:
                    return (1378, 1377);

                case 1422:
                    return (1421, 1420);

                case 1429:
                case 1430:
                    return (1428, 1427);

                default:
                    return (0, 0);
            }
        }

        private static (short readCode, short writeCode) GetAGLWriteReadCode(short code)
        {
            switch (code)
            {
                case 1252:
                case 1253:
                    return (1251, 1250);

                case 1343:
                case 1344:
                case 1345:
                case 1346:
                case 1347:
                case 1348:
                case 1349:
                case 1350:
                case 1351:
                case 1352:
                    return (1342, 1341);

                case 1362:
                    return (1361, 1360);

                case 1379:
                case 1380:
                case 1381:
                case 1382:
                case 1388:
                case 1389:
                case 1390:
                case 1391:
                    return (1378, 1377);

                case 1422:
                    return (1421, 1420);

                case 1429:
                case 1430:
                    return (1428, 1427);

                default:
                    return (0, 0);
            }
        }

        private static (bool isReadonly, string type) ConvertBit0(int bit0)
        {
            switch (bit0)
            {
                case 4:
                    //read
                    return (true, string.Empty);

                case 5:
                    //read, dword
                    return (true, INTTYPE);

                case 6:
                    //read, word
                    return (true, SHORTTYPE);

                case 12:
                    //write, read
                    return (false, string.Empty);

                case 13:
                    //read, write, dword
                    return (false, INTTYPE);

                case 14:
                    //read, write, word
                    return (false, SHORTTYPE);

                default:
                    //error
                    return (false, string.Empty);
            }
        }

        private static string ConvertBit1(int bit1)
        {
            switch (bit1)
            {
                case 0:
                    //none
                    break;

                case 1:
                    //special
                    break;

                case 2:
                    //dataset
                    break;

                case 3:
                    //special, dataset
                    break;

                case 4:
                    //unsigned
                    return USHORTTYPE;

                case 5:
                    //unsigned, special
                    return USHORTTYPE;

                case 6:
                    //unsigned, dataset
                    return USHORTTYPE;

                case 7:
                    //unsigned, dataset, special
                    return USHORTTYPE;

                case 8:
                    //string
                    return STRINGTYPE;

                case 10:
                    //string, dataset
                    return STRINGTYPE;

                default:
                    //error
                    break;
            }

            return string.Empty;
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

        private static string FixDecimalValue(string value, int decimalCount)
        {
            if (decimalCount > value.Length)
            {
                if (decimalCount == 1)
                {
                    value = "0" + value;
                }
                else if (decimalCount == 2)
                {
                    value = "00" + value;
                }
            }

            return value;
        }

        #endregion Methods
    }
}
