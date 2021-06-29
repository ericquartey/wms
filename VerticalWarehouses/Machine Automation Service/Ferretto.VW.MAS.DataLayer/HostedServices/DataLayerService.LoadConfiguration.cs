using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

        private static void ValidateJson(JObject jsonObject)
        {
            try
            {
                using (var streamReader = new StreamReader("configuration/schemas/vertimag-configuration-schema.json"))
                {
                    using (var textReader = new JsonTextReader(streamReader))
                    {
                        var schema = JSchema.Load(textReader);
                        jsonObject.Validate(schema);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        private void GenerateAccessories(DataLayerContext dataContext)
        {
            if (dataContext.Accessories.Any())
            {
                return;
            }

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
                foreach (var bayNumber in machine.Bays.Select(b => b.Number))
                {
                    var bay = machine.Bays.Single(b => b.Number == bayNumber);
                    bay.Accessories = new BayAccessories();

                    bay.Accessories.AlphaNumericBar = new AlphaNumericBar();
                    dataContext.Accessories.Add(bay.Accessories.AlphaNumericBar);

                    bay.Accessories.BarcodeReader = new BarcodeReader();
                    dataContext.Accessories.Add(bay.Accessories.BarcodeReader);

                    bay.Accessories.CardReader = new CardReader();
                    dataContext.Accessories.Add(bay.Accessories.CardReader);

                    bay.Accessories.LabelPrinter = new LabelPrinter();
                    dataContext.Accessories.Add(bay.Accessories.LabelPrinter);

                    bay.Accessories.LaserPointer = new LaserPointer();
                    dataContext.Accessories.Add(bay.Accessories.LaserPointer);

                    bay.Accessories.TokenReader = new TokenReader();
                    dataContext.Accessories.Add(bay.Accessories.TokenReader);

                    bay.Accessories.WeightingScale = new WeightingScale();
                    dataContext.Accessories.Add(bay.Accessories.WeightingScale);

                    dataContext.Bays.Update(bay);
                }
                dataContext.SaveChanges();
            }
        }

        private void GenerateInstructionDefinitions(DataLayerContext dataContext)
        {
            if (dataContext.InstructionDefinitions.Any())
            {
                return;
            }

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                const int HighMissionCount = 300000;
                const int NormalMissionCount = 40000;
                const int LowMissionCount = 20000;
                const int VeryHighMissionCount = 600000;
                const int OneYear = 365;

                // this instruction is reeeeally slow
                var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();

                foreach (InstructionType instructionType in Enum.GetValues(typeof(InstructionType)))
                {
                    var instruction = new InstructionDefinition();
                    switch (instructionType)
                    {
                        case InstructionType.AirFiltersCheck:
                        case InstructionType.ElectricalComponentsCheck:
                        case InstructionType.LampsCheck:
                            instruction.InstructionType = instructionType;
                            instruction.IsSystem = true;
                            instruction.MaxDays = OneYear;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.BearingsCheck:
                        case InstructionType.BearingsGrease:
                        case InstructionType.BeltAdjust:
                        case InstructionType.BeltFasten:
                        case InstructionType.CableChainCheck:
                        case InstructionType.CablesCheck:
                        case InstructionType.FirstCellCheck:
                        case InstructionType.RandomCellCheck:
                            instruction.InstructionType = instructionType;
                            instruction.Axis = Axis.Vertical;
                            instruction.MaxDays = OneYear;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.BeltSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear * 5;
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction.IsShutter = true;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.IsShutter = false;
                            instruction.Axis = Axis.Vertical;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.ChainAdjust:
                        case InstructionType.ChainGrease:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction.Axis = Axis.Vertical;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction.Axis = Axis.Horizontal;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.ChainSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.MaxRelativeCount = NormalMissionCount;
                            instruction.GetDescription(instruction.InstructionType);

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.Axis = Axis.Vertical;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction.Axis = Axis.Horizontal;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.ContactorsSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.IsSystem = true;
                            instruction.MaxDays = OneYear * 5;
                            instruction.GetDescription(instruction.InstructionType);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.GuidesCheck:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.IsShutter = true;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.IsShutter = false;
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction.Axis = Axis.Vertical;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.GuidesSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.MaxRelativeCount = VeryHighMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.IsShutter = true;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionType.LinkCheck:
                        case InstructionType.LinksGrease:
                        case InstructionType.MotorGearOil:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.GetDescription(instruction.InstructionType);
                            instruction.Axis = Axis.Vertical;
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction.Axis = Axis.Horizontal;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.OpticalSensorsClean:
                        case InstructionType.OpticalSensorsMount:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.Axis = Axis.Vertical;
                            dataContext.InstructionDefinitions.Add(instruction);

                            break;

                        case InstructionType.MicroSwitchesCheck:
                        case InstructionType.MicroSwitchesMount:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.IsShutter = true;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.IsShutter = false;

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.Axis = Axis.Vertical;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.SensorCheck:
                        case InstructionType.SensorsClean:
                        case InstructionType.SensorsMount:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            if (instructionType == InstructionType.SensorCheck)
                            {
                                foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                                {
                                    instruction.SetCounterName(bay.Number);
                                    instruction.IsShutter = true;
                                    instruction.BayNumber = bay.Number;
                                    dataContext.InstructionDefinitions.Add(instruction);
                                }
                                instruction.IsShutter = false;
                            }
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.Axis = Axis.Vertical;
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction.Axis = Axis.Horizontal;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.LinkSubstitute:
                        case InstructionType.MotorChainSubstitute:
                        case InstructionType.PinPawlFastenersSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.MaxRelativeCount = NormalMissionCount;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.Axis = Axis.Horizontal;
                            instruction.GetDescription(instruction.InstructionType);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.MicroSwitchesSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear * 5;
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction.IsShutter = true;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.IsShutter = false;
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.Axis = Axis.Vertical;
                            instruction.GetDescription(instruction.InstructionType);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.MotorChainAdjust:
                        case InstructionType.MotorChainGrease:
                        case InstructionType.PinPawlFastenersCheck:
                        case InstructionType.WheelsCheck:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.Axis = Axis.Horizontal;
                            instruction.GetDescription(instruction.InstructionType);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.MotorGearSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.MaxRelativeCount = VeryHighMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.IsShutter = true;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.IsShutter = false;
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction.MaxRelativeCount = HighMissionCount;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.Axis = Axis.Vertical;
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction.Axis = Axis.Horizontal;
                            dataContext.InstructionDefinitions.Add(instruction);

                            break;

                        case InstructionType.PlasticCamsCheck:
                        case InstructionType.SupportsCheck:
                            instruction.InstructionType = instructionType;
                            instruction.MaxDays = OneYear;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.IsShutter = true;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction.IsShutter = false;
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.Axis = Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionType.ShaftCheck:
                            instruction.InstructionType = instructionType;
                            instruction.MaxRelativeCount = LowMissionCount;
                            instruction.GetDescription(instruction.InstructionType);
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction.SetCounterName(bay.Number);
                                instruction.IsShutter = true;
                                instruction.BayNumber = bay.Number;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionType.Undefined:
                            break;

                        default:
                            break;
                    }
                }

                dataContext.SaveChanges();
            }
        }

        private async Task LoadConfigurationAsync(string configurationFilePath, DataLayerContext dataContext)
        {
            if (dataContext.Machines.Any())
            {
                return;
            }

            this.Logger.LogInformation($"First run: loading configuration from JSON file ...");

            string fileContents;
            using (var streamReader = new StreamReader(configurationFilePath))
            {
                fileContents = await streamReader.ReadToEndAsync();
            }

            var jsonObject = JObject.Parse(fileContents);

            ValidateJson(jsonObject);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());

            var vertimagConfiguration = JsonConvert.DeserializeObject<VertimagConfiguration>(jsonObject.ToString(), settings);
            vertimagConfiguration.Validate();

            dataContext.Machines.Add(vertimagConfiguration.Machine);
            dataContext.LoadingUnits.AddRange(vertimagConfiguration.LoadingUnits);
            dataContext.SetupProceduresSets.Add(vertimagConfiguration.SetupProcedures);
            if (vertimagConfiguration.Wms != null)
            {
                var wmsSettings = dataContext.WmsSettings.Single();
                vertimagConfiguration.Wms.Id = wmsSettings.Id;
                dataContext.AddOrUpdate(vertimagConfiguration.Wms, s => s.Id);
            }

            if (vertimagConfiguration.MachineStatistics != null)
            {
                dataContext.AddRange(vertimagConfiguration.MachineStatistics);
            }
            if (vertimagConfiguration.ServicingInfo != null)
            {
                dataContext.AddRange(vertimagConfiguration.ServicingInfo);
            }

            dataContext.SaveChanges();

            this.Logger.LogInformation($"First run: configuration loaded.");

            await this.LoadSeedsAsync();

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IMachineProvider>().UpdateWeightStatistics(dataContext);
            }
        }

        private async Task LoadSeedsAsync()
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var environment = scope.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                var seedFileName = GetSeedFileName(environment.EnvironmentName);

                if (File.Exists(seedFileName))
                {
                    this.Logger.LogInformation($"First run: applying seed file '{seedFileName}' ...");

                    var seedScript = await File.ReadAllTextAsync(seedFileName);

                    var dataContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();
                    await dataContext.Database.ExecuteSqlCommandAsync(seedScript);
                }
            }
        }

        #endregion
    }
}
