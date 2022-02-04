using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels.Maintenance;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class InstructionDefinition : DataModel
    {
        #region Constructors

        [JsonConstructor]
        public InstructionDefinition(InstructionType instructionType, string counterName, int? maxDays, int? maxRelativeCount, Axis axis, BayNumber bayNumber)
        {
            this.InstructionType = instructionType;
            //this.GetDescription(instructionType);
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

        public InstructionDefinition(InstructionDevice device, InstructionOperation operation, string counterName, int? maxDays, int? maxRelativeCount, Axis axis, BayNumber bayNumber, int setPoint = 0)
        {
            this.Device = device;
            this.Operation = operation;

            this.GetDescription(device);

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
            this.SetPoint = setPoint;
        }

        #endregion

        #region Properties

        public Axis Axis { get; set; }

        public BayNumber BayNumber { get; set; }

        public string CounterName { get; set; }

        public string Description { get; set; }

        public InstructionDevice Device { get; set; }

        public InstructionType InstructionType { get; set; }

        public bool IsCarousel { get; set; }

        public bool IsShutter { get; set; }

        public bool IsSystem { get; set; }

        public int? MaxDays { get; set; }

        public int? MaxRelativeCount { get; set; }

        public int? MaxTotalCount { get; set; }

        public InstructionOperation Operation { get; set; }

        public int SetPoint { get; set; }

        #endregion

        #region Methods

        public void GetDescription(InstructionType device)
        {
            this.Operation = InstructionOperation.Check;
            switch (device)
            {
                case InstructionType.AirFiltersCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("AirFilters", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.AirFilters;
                    break;

                case InstructionType.BearingsCheck:
                case InstructionType.BearingsGrease:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Bearings", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Bearings;
                    break;

                case InstructionType.BeltAdjust:
                case InstructionType.BeltFasten:
                case InstructionType.BeltSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Belt", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Belt;
                    break;

                case InstructionType.CableChainCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("CableChain", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.CableChain;
                    break;

                case InstructionType.CablesCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Cables", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Cables;
                    break;

                case InstructionType.ElectricalComponentsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("VaristorsAndRelays", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.VaristorsAndRelays;
                    break;

                case InstructionType.ChainAdjust:
                case InstructionType.ChainGrease:
                case InstructionType.ChainSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Chain", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Chain;
                    break;

                case InstructionType.ContactorsSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Contactors", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Contactors;
                    break;

                case InstructionType.FirstCellCheck:
                case InstructionType.RandomCellCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("FirstCell", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.FirstCell;
                    break;

                case InstructionType.GuidesCheck:
                case InstructionType.GuidesSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Guides", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Guides;
                    break;

                case InstructionType.LampsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Lamps", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Lamps;
                    break;

                case InstructionType.LinkCheck:
                case InstructionType.LinksGrease:
                case InstructionType.LinkSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Link", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Link;
                    break;

                case InstructionType.MicroSwitchesCheck:
                case InstructionType.MicroSwitchesMount:
                case InstructionType.MicroSwitchesSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MicroSwitches", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.MicroSwitches;
                    break;

                case InstructionType.MotorChainAdjust:
                case InstructionType.MotorChainGrease:
                case InstructionType.MotorChainSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorChain", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.MotorChain;
                    break;

                case InstructionType.MotorGearOil:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorGearOil", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.MotorGear;
                    break;

                case InstructionType.MotorGearSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorGear", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.MotorGear;
                    break;

                case InstructionType.OpticalSensorsClean:
                case InstructionType.OpticalSensorsMount:
                    this.Description = Resources.Instructions.ResourceManager.GetString("OpticalSensors", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.OpticalSensors;
                    break;

                case InstructionType.PinPawlFastenersCheck:
                case InstructionType.PinPawlFastenersSubstitute:
                    this.Description = Resources.Instructions.ResourceManager.GetString("PinPawlFasteners", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.PinPawlFasteners;
                    break;

                case InstructionType.PlasticCamsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("PlasticCams", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.PlasticCams;
                    break;

                case InstructionType.SensorsClean:
                case InstructionType.SensorsMount:
                case InstructionType.SensorCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Sensors", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Sensors;
                    break;

                case InstructionType.ShaftCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Shaft", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Shaft;
                    break;

                case InstructionType.SupportsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Supports", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Supports;
                    break;

                case InstructionType.WheelsCheck:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Wheels", CommonUtils.Culture.Actual);
                    this.Device = InstructionDevice.Wheels;
                    break;

                case InstructionType.Undefined:
                default:
                    this.Description = null;
                    break;
            }
        }

        public void GetDescription(InstructionDevice device)
        {
            switch (device)
            {
                case InstructionDevice.AirFilters:
                    this.Description = Resources.Instructions.ResourceManager.GetString("AirFilters", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Bearings:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Bearings", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Belt:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Belt", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.CableChain:
                    this.Description = Resources.Instructions.ResourceManager.GetString("CableChain", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Cables:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Cables", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Chain:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Chain", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Contactors:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Contactors", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.FirstCell:
                    this.Description = Resources.Instructions.ResourceManager.GetString("FirstCell", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Guides:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Guides", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Lamps:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Lamps", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Link:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Link", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.MicroSwitches:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MicroSwitches", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.MotorChain:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorChain", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.MotorBelt:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorBelt", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.MotorGearOil:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorGearOil", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.MotorGear:
                    this.Description = Resources.Instructions.ResourceManager.GetString("MotorGear", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.OpticalSensors:
                    this.Description = Resources.Instructions.ResourceManager.GetString("OpticalSensors", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.PinPawlFasteners:
                    this.Description = Resources.Instructions.ResourceManager.GetString("PinPawlFasteners", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.PlasticCams:
                    this.Description = Resources.Instructions.ResourceManager.GetString("PlasticCams", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.BayQuote:
                    this.Description = Resources.Instructions.ResourceManager.GetString("BayQuote", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Sensors:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Sensors", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Shaft:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Shaft", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Supports:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Supports", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Wheels:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Wheels", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.VaristorsAndRelays:
                    this.Description = Resources.Instructions.ResourceManager.GetString("VaristorsAndRelays", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Clean:
                    this.Description = Resources.Instructions.ResourceManager.GetString("Clean", CommonUtils.Culture.Actual);
                    break;

                case InstructionDevice.Undefined:
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
