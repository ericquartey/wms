using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Maintenance;
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

                foreach (InstructionDevice instructionDevice in Enum.GetValues(typeof(InstructionDevice)))
                {
                    InstructionDefinition instruction;
                    switch (instructionDevice)
                    {
                        case InstructionDevice.MotorGearOil: //motoriduttore
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.MotorGear: //motoriduttore
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Substitute,
                                nameof(MachineStatistics.TotalMissions),
                                null,
                                HighMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Substitute,
                                    nameof(MachineStatistics.TotalMissions),
                                    null,
                                    VeryHighMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Substitute,
                                    nameof(MachineStatistics.TotalMissions),
                                    null,
                                    VeryHighMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.Belt:
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Adjust,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Substitute,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear * 5,
                                null,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null))
                            {
                                instruction = new InstructionDefinition(
                                   instructionDevice,
                                   InstructionOperation.Adjust,
                                   nameof(MachineStatistics.TotalMissions),
                                   OneYear,
                                   LowMissionCount,
                                   Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Substitute,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear * 5,
                                    null,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.Chain:
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Adjust,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.FirstCell: //quote e mensole
                        case InstructionDevice.BayQuote:
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.Link: // giunti del sistema di sollevamento
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Adjust,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.Bearings: // supporti cuscinetti
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.OpticalSensors: // fotocellule
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Adjust,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.MicroSwitches: // microinterrutori
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Adjust,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Substitute,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear * 5,
                                null,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            foreach (var bay in machine.Bays)
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                     instructionDevice,
                                     InstructionOperation.Adjust,
                                     nameof(MachineStatistics.TotalMissions),
                                     OneYear,
                                     LowMissionCount,
                                     Axis.None,
                                     bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                     instructionDevice,
                                     InstructionOperation.Substitute,
                                     nameof(MachineStatistics.TotalMissions),
                                     OneYear * 5,
                                     null,
                                     Axis.None,
                                     bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);

                                if (bay.Shutter != null)
                                {
                                    instruction = new InstructionDefinition(
                                           instructionDevice,
                                           InstructionOperation.Check,
                                           nameof(MachineStatistics.TotalMissions),
                                           OneYear,
                                           LowMissionCount,
                                           Axis.None,
                                           bay.Number);
                                    instruction.IsShutter = true;
                                    dataContext.InstructionDefinitions.Add(instruction);

                                    instruction = new InstructionDefinition(
                                         instructionDevice,
                                         InstructionOperation.Adjust,
                                         nameof(MachineStatistics.TotalMissions),
                                         OneYear,
                                         LowMissionCount,
                                         Axis.None,
                                         bay.Number);
                                    instruction.IsShutter = true;
                                    dataContext.InstructionDefinitions.Add(instruction);

                                    instruction = new InstructionDefinition(
                                         instructionDevice,
                                         InstructionOperation.Substitute,
                                         nameof(MachineStatistics.TotalMissions),
                                         OneYear * 5,
                                         null,
                                         Axis.None,
                                         bay.Number);
                                    instruction.IsShutter = true;
                                    dataContext.InstructionDefinitions.Add(instruction);
                                }
                            }
                            break;

                        case InstructionDevice.Sensors: // sensori
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Adjust,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Adjust,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            foreach (var bay in machine.Bays)
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                     instructionDevice,
                                     InstructionOperation.Adjust,
                                     nameof(MachineStatistics.TotalMissions),
                                     OneYear,
                                     LowMissionCount,
                                     Axis.None,
                                     bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.Cables: // scatole di distribuzione
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.None,
                                BayNumber.None);
                            instruction.IsSystem = true;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.CableChain: // catena portacavi
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Vertical,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.MotorChain: // catena motore
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Adjust,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Substitute,
                                nameof(MachineStatistics.TotalMissions),
                                null,
                                NormalMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Adjust,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Substitute,
                                    nameof(MachineStatistics.TotalMissions),
                                    null,
                                    NormalMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.MotorBelt:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null))
                            {
                                instruction = new InstructionDefinition(
                                   instructionDevice,
                                   InstructionOperation.Adjust,
                                   nameof(MachineStatistics.TotalMissions),
                                   OneYear,
                                   LowMissionCount,
                                   Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Substitute,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear * 5,
                                    null,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.PinPawlFasteners: // perni aggancio nottolino
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Adjust,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Substitute,
                                nameof(MachineStatistics.TotalMissions),
                                null,
                                NormalMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.Guides: // guide elevatore / giude lineari
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);

                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Adjust,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Substitute,
                                    nameof(MachineStatistics.TotalMissions),
                                    null,
                                    VeryHighMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.Wheels: // ruote di contrasto
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.Horizontal,
                                BayNumber.None);
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.Lamps:
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.None,
                                BayNumber.None);
                            instruction.IsSystem = true;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.AirFilters:
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Check,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear,
                                LowMissionCount,
                                Axis.None,
                                BayNumber.None);
                            instruction.IsSystem = true;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.VaristorsAndRelays:
                            instruction = new InstructionDefinition(
                                instructionDevice,
                                InstructionOperation.Substitute,
                                nameof(MachineStatistics.TotalMissions),
                                OneYear * 5,
                                null,
                                Axis.None,
                                BayNumber.None);
                            instruction.IsSystem = true;
                            dataContext.InstructionDefinitions.Add(instruction);
                            break;

                        case InstructionDevice.Supports:
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Adjust,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);

                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Adjust,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.PlasticCams:
                            foreach (var bay in machine.Bays.Where(b => b.Carousel != null || b.IsExternal))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.BayChain,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }

                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.Shaft:
                            foreach (var bay in machine.Bays.Where(b => b.Shutter != null))
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Check,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                instruction.IsShutter = true;
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
                            break;

                        case InstructionDevice.Clean:
                            instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Adjust,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    BayNumber.None);
                            instruction.IsSystem = true;
                            dataContext.InstructionDefinitions.Add(instruction);

                            foreach (var bay in machine.Bays)
                            {
                                instruction = new InstructionDefinition(
                                    instructionDevice,
                                    InstructionOperation.Adjust,
                                    nameof(MachineStatistics.TotalMissions),
                                    OneYear,
                                    LowMissionCount,
                                    Axis.None,
                                    bay.Number);
                                dataContext.InstructionDefinitions.Add(instruction);
                            }
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
                        RequiredCycles = 25
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
