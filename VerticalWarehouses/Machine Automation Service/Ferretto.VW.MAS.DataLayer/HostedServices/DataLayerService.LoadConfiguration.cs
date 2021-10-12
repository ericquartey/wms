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
                    InstructionDefinition instruction;
                    switch (instructionType)
                    {
                        case InstructionType.AirFiltersCheck:
                        case InstructionType.ElectricalComponentsCheck:
                        case InstructionType.LampsCheck:
                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.None,
                                BayNumber.None);
                            instruction.IsSystem = true;
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
                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.BeltSubstitute:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear * 5,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear * 5,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.ChainAdjust:
                        case InstructionType.ChainGrease:
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.ChainSubstitute:
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    null,
                                    NormalMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                null,
                                NormalMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.ContactorsSubstitute:
                            instruction = new InstructionDefinition(instructionType,
                                null,
                                OneYear * 5,
                                null,
                                Axis.None,
                                BayNumber.None);
                            instruction.IsSystem = true;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.GuidesCheck:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                NormalMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.GuidesSubstitute:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    null,
                                    VeryHighMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionType.LinkCheck:
                        case InstructionType.LinksGrease:
                        case InstructionType.MotorGearOil:
                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.OpticalSensorsClean:
                        case InstructionType.OpticalSensorsMount:
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            break;

                        case InstructionType.MicroSwitchesCheck:
                        case InstructionType.MicroSwitchesMount:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.SensorCheck:
                        case InstructionType.SensorsClean:
                        case InstructionType.SensorsMount:
                            if (instructionType == InstructionType.SensorCheck)
                            {
                                foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                                {
                                    instruction = new InstructionDefinition(instructionType,
                                        nameof(MachineStatistics.TotalMissions),
                                        OneYear,
                                        LowMissionCount,
                                        Axis.None,
                                        bay.Number);
                                    instruction.IsShutter = true;
                                    dataContext.InstructionDefinitions.Add(instruction);
                                }
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.LinkSubstitute:
                        case InstructionType.MotorChainSubstitute:
                        case InstructionType.PinPawlFastenersSubstitute:
                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                null,
                                NormalMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.MicroSwitchesSubstitute:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear * 5,
                                    null,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear * 5,
                                    null,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear * 5,
                                null,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.MotorChainAdjust:
                        case InstructionType.MotorChainGrease:
                        case InstructionType.PinPawlFastenersCheck:
                        case InstructionType.WheelsCheck:
                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionType.MotorGearSubstitute:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    null,
                                    VeryHighMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    null,
                                    VeryHighMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                null,
                                HighMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(instructionType,
                                nameof(MachineStatistics.TotalMissions),
                                null,
                                HighMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            break;

                        case InstructionType.PlasticCamsCheck:
                        case InstructionType.SupportsCheck:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionType.ShaftCheck:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified))
                            {
                                instruction = new InstructionDefinition(instructionType,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
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

        private void GenerateSetupProcedures(DataLayerContext dataContext)
        {
            var sets = dataContext.SetupProceduresSets.SingleOrDefault();
            if (sets != null)
            {
                if (dataContext.SetupProceduresSets.Any(s => s.HorizontalResolutionCalibration == null))
                {
                    var procedure = new RepeatedTestProcedure()
                    {
                        FeedRate = 1,
                        IsBypassed = false,
                        IsCompleted = false,
                        InProgress = false,
                        PerformedCycles = 0,
                        RequiredCycles = 20
                    };
                    sets.HorizontalResolutionCalibration = procedure;
                    dataContext.SetupProcedures.Add(procedure);
                    dataContext.SaveChanges();
                }
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
