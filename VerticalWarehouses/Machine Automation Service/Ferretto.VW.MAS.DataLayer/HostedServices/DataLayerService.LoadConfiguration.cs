using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Converters;
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

        private void GenerateInstructions(DataLayerContext dataContext)
        {
            if (dataContext.InstructionDefinitions.Any())
            {
                return;
            }

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                const int OneYear = 365;
                var machine = scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
                foreach (InstructionType instructionType in Enum.GetValues(typeof(InstructionType)))
                {
                    var instruction = new InstructionDefinition();
                    switch (instructionType)
                    {
                        case InstructionType.AirFiltersCheck:
                            instruction.InstructionType = instructionType;
                            instruction.IsSystem = true;
                            instruction.MaxDays = OneYear;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.MaxRelativeCount = 20000;
                            break;

                        case InstructionType.BearingsCheck:
                        case InstructionType.BearingsGrease:
                        case InstructionType.BeltAdjust:
                        case InstructionType.BeltFasten:
                        case InstructionType.CableChainCheck:
                        case InstructionType.CablesCheck:
                            instruction.InstructionType = instructionType;
                            instruction.Axis = CommonUtils.Messages.Enumerations.Axis.Vertical;
                            instruction.MaxDays = OneYear;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.MaxRelativeCount = 20000;
                            break;

                        case InstructionType.ChainAdjust:
                        case InstructionType.ChainGrease:

                            instruction.InstructionType = instructionType;
                            instruction.Axis = CommonUtils.Messages.Enumerations.Axis.Vertical;
                            instruction.MaxDays = OneYear;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            instruction.MaxRelativeCount = 20000;
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction.Axis = CommonUtils.Messages.Enumerations.Axis.Horizontal;

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null))
                            {
                                dataContext.InstructionDefinitions.Add(instruction);
                                instruction.Axis = CommonUtils.Messages.Enumerations.Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                            }
                            break;

                        case InstructionType.BeltSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.Axis = CommonUtils.Messages.Enumerations.Axis.Vertical;
                            instruction.MaxDays = OneYear * 5;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            break;

                        case InstructionType.ChainSubstitute:
                            instruction.InstructionType = instructionType;
                            instruction.Axis = CommonUtils.Messages.Enumerations.Axis.Vertical;
                            instruction.MaxRelativeCount = 40000;
                            instruction.CounterName = nameof(MachineStatistics.TotalMissions);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction.Axis = CommonUtils.Messages.Enumerations.Axis.Horizontal;
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null))
                            {
                                dataContext.InstructionDefinitions.Add(instruction);
                                instruction.Axis = CommonUtils.Messages.Enumerations.Axis.BayChain;
                                instruction.BayNumber = bay.Number;
                            }
                            break;

                        default:
                            continue;
                    }
                    dataContext.InstructionDefinitions.Add(instruction);
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
