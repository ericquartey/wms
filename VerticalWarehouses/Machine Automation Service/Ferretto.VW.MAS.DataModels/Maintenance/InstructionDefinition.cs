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

        public InstructionDefinition(InstructionDevice device, InstructionOperation operation, string counterName, int? maxDays, int? maxRelativeCount, Axis axis, BayNumber bayNumber)
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

        #endregion

        #region Methods

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
