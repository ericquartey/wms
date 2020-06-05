using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels.Extensions;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class InstructionDefinition : DataModel
    {
        #region Properties

        public Axis Axis { get; set; }

        public BayNumber BayNumber { get; set; }

        public string CounterName { get; set; }

        public string Description { get; set; }

        public InstructionType InstructionType { get; set; }

        public bool IsCarousel { get; set; }

        public bool IsShutter { get; set; }

        public bool IsSystem { get; set; }

        public int? MaxDays { get; set; }

        public int? MaxRelativeCount { get; set; }

        public int? MaxTotalCount { get; set; }

        #endregion

        #region Methods

        public void GetDescription(InstructionType instructionType)
        {
            switch (instructionType)
            {
                case InstructionType.AirFiltersCheck:
                    this.Description = Resources.Instructions.AirFiltersCheck;
                    break;

                case InstructionType.BearingsCheck:
                    this.Description = Resources.Instructions.BearingsCheck;
                    break;

                case InstructionType.BearingsGrease:
                    this.Description = Resources.Instructions.BearingsGrease;
                    break;

                case InstructionType.BeltAdjust:
                    this.Description = Resources.Instructions.BeltAdjust;
                    break;

                case InstructionType.BeltFasten:
                    this.Description = Resources.Instructions.BeltFasten;
                    break;

                case InstructionType.BeltSubstitute:
                    this.Description = Resources.Instructions.BeltSubstitute;
                    break;

                case InstructionType.CableChainCheck:
                    this.Description = Resources.Instructions.CableChainCheck;
                    break;

                case InstructionType.CablesCheck:
                    this.Description = Resources.Instructions.CablesCheck;
                    break;

                case InstructionType.ChainAdjust:
                    this.Description = Resources.Instructions.ChainAdjust;
                    break;

                case InstructionType.ChainGrease:
                    this.Description = Resources.Instructions.ChainGrease;
                    break;

                case InstructionType.ChainSubstitute:
                    this.Description = Resources.Instructions.ChainSubstitute;
                    break;

                case InstructionType.ContactorsSubstitute:
                    this.Description = Resources.Instructions.ContactorsSubstitute;
                    break;

                case InstructionType.ElectricalComponentsCheck:
                    this.Description = Resources.Instructions.ElectricalComponentsCheck;
                    break;

                case InstructionType.FirstCellCheck:
                    this.Description = Resources.Instructions.FirstCellCeck;
                    break;

                case InstructionType.GuidesCheck:
                    this.Description = Resources.Instructions.GuidesCheck;
                    break;

                case InstructionType.GuidesSubstitute:
                    this.Description = Resources.Instructions.GuidesSubstitute;
                    break;

                case InstructionType.LampsCheck:
                    this.Description = Resources.Instructions.LampsCheck;
                    break;

                case InstructionType.LinkCheck:
                    this.Description = Resources.Instructions.LinkCheck;
                    break;

                case InstructionType.LinksGrease:
                    this.Description = Resources.Instructions.LinksGrease;
                    break;

                case InstructionType.LinkSubstitute:
                    this.Description = Resources.Instructions.LinkSubstitute;
                    break;

                case InstructionType.MicroSwitchesCheck:
                    this.Description = Resources.Instructions.MicroSwitchesCheck;
                    break;

                case InstructionType.MicroSwitchesMount:
                    this.Description = Resources.Instructions.MicroSwitchesMount;
                    break;

                case InstructionType.MicroSwitchesSubstitute:
                    this.Description = Resources.Instructions.MicroSwitchesSubstitute;
                    break;

                case InstructionType.MotorChainAdjust:
                    this.Description = Resources.Instructions.MotorChainAdjust;
                    break;

                case InstructionType.MotorChainGrease:
                    this.Description = Resources.Instructions.MotorChainGrease;
                    break;

                case InstructionType.MotorChainSubstitute:
                    this.Description = Resources.Instructions.MotorChainSubstitute;
                    break;

                case InstructionType.MotorGearOil:
                    this.Description = Resources.Instructions.MotorGearOil;
                    break;

                case InstructionType.MotorGearSubstitute:
                    this.Description = Resources.Instructions.MotorGearSubstitute;
                    break;

                case InstructionType.OpticalSensorsClean:
                    this.Description = Resources.Instructions.OpticalSensorsClean;
                    break;

                case InstructionType.OpticalSensorsMount:
                    this.Description = Resources.Instructions.OpticalSensorsMount;
                    break;

                case InstructionType.PinPawlFastenersCheck:
                    this.Description = Resources.Instructions.PinPawlFastenersCheck;
                    break;

                case InstructionType.PinPawlFastenersSubstitute:
                    this.Description = Resources.Instructions.PinPawlFastenersSubstitute;
                    break;

                case InstructionType.PlasticCamsCheck:
                    this.Description = Resources.Instructions.PlasticCamsCheck;
                    break;

                case InstructionType.RandomCellCheck:
                    this.Description = Resources.Instructions.RandomCellCheck;
                    break;

                case InstructionType.SensorCheck:
                    this.Description = Resources.Instructions.SensorCheck;
                    break;

                case InstructionType.SensorsClean:
                    this.Description = Resources.Instructions.SensorsClean;
                    break;

                case InstructionType.SensorsMount:
                    this.Description = Resources.Instructions.SensorsMount;
                    break;

                case InstructionType.ShaftCheck:
                    this.Description = Resources.Instructions.ShaftCheck;
                    break;

                case InstructionType.SupportsCheck:
                    this.Description = Resources.Instructions.SupportsCheck;
                    break;

                case InstructionType.Undefined:
                    this.Description = null;
                    break;

                case InstructionType.WheelsCheck:
                    this.Description = Resources.Instructions.WheelsCheck;
                    break;

                default:
                    this.Description = null;
                    break;
            }
        }

        public void SetCounterName(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                case BayNumber.BayOne:
                    this.CounterName = nameof(MachineStatistics.TotalLoadUnitsInBay1);
                    break;

                case BayNumber.BayTwo:
                    this.CounterName = nameof(MachineStatistics.TotalLoadUnitsInBay2);
                    break;

                case BayNumber.BayThree:
                    this.CounterName = nameof(MachineStatistics.TotalLoadUnitsInBay3);
                    break;
            }
        }

        #endregion
    }
}
