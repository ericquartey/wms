using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels.Extensions;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class InstructionDefinition : DataModel
    {
        #region Constructors

        public InstructionDefinition(InstructionType instructionType, string counterName, int? maxDays, int? maxRelativeCount, Axis axis, BayNumber bayNumber)
        {
            this.InstructionType = instructionType;
            this.GetDescription(instructionType);
            if (bayNumber == BayNumber.None)
            {
                this.CounterName = counterName;
            }
            else
            {
                this.SetCounterName(bayNumber);
            }
            this.MaxDays = maxDays;
            this.MaxRelativeCount = maxRelativeCount;
            this.Axis = axis;
            this.BayNumber = bayNumber;
        }

        #endregion

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
                    this.Description = Resources.Instructions.ResourceManager.GetString("AirFiltersCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.BearingsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("BearingsCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.BearingsGrease:
                    this.Description = Resources.Instructions.ResourceManager.GetString("BearingsGrease", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.BeltAdjust:
                    this.Description = Resources.Instructions.ResourceManager.GetString("BeltAdjust", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.BeltFasten:
                    this.Description = Resources.Instructions.ResourceManager.GetString("BeltFasten", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.BeltSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("BeltSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.CableChainCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("CableChainCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.CablesCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("CablesCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.ChainAdjust:
                    this.Description = Resources.Instructions.ResourceManager.GetString("ChainAdjust", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.ChainGrease:
                    this.Description = Resources.Instructions.ResourceManager.GetString("ChainGrease", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.ChainSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("ChainSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.ContactorsSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("ContactorsSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.ElectricalComponentsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("ElectricalComponentsCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.FirstCellCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("FirstCellCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.GuidesCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("GuidesCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.GuidesSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("GuidesSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.LampsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("LampsCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.LinkCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("LinkCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.LinksGrease:
                    this.Description = Resources.Instructions.ResourceManager.GetString("LinksGrease", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.LinkSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("LinkSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.MicroSwitchesCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MicroSwitchesCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.MicroSwitchesMount:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MicroSwitchesMount", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.MicroSwitchesSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MicroSwitchesSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.MotorChainAdjust:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorChainAdjust", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.MotorChainGrease:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorChainGrease", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.MotorChainSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorChainSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.MotorGearOil:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorGearOil", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.MotorGearSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorGearSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.OpticalSensorsClean:
                    this.Description = Resources.Instructions.ResourceManager.GetString("OpticalSensorsClean", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.OpticalSensorsMount:
                    this.Description = Resources.Instructions.ResourceManager.GetString("OpticalSensorsMount", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.PinPawlFastenersCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("PinPawlFastenersCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.PinPawlFastenersSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("PinPawlFastenersSubstitute", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.PlasticCamsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("PlasticCamsCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.RandomCellCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("RandomCellCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.SensorCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("SensorCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.SensorsClean:
                    this.Description = Resources.Instructions.ResourceManager.GetString("SensorsClean", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.SensorsMount:
                    this.Description = Resources.Instructions.ResourceManager.GetString("SensorsMount", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.ShaftCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("ShaftCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.SupportsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("SupportsCheck", CommonUtils.Culture.Actual);
                    break;

                case InstructionType.Undefined:
                    this.Description = null;
                    break;

                case InstructionType.WheelsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("WheelsCheck", CommonUtils.Culture.Actual);
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
