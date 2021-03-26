using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
//using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.Simulator.Resources;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services.Models
{
    public enum InverterCalibrationMode : ushort
    {
        Elevator = 5,

        FindSensorCarousel = 20,

        FindSensor = 22,

        ResetEncoder = 35,
    }

    public enum InverterOperationMode : ushort
    {
        Position = 1,

        Homing = 6,

        Velocity = 2,

        ProfileVelocity = 3,

        SlaveGear = 253,

        LeaveLimitSwitch = 254,

        TableTravel = 255,
    }

    public enum InverterRole
    {
        Main = 0,

        ElevatorChain = 1,

        Shutter1 = 2,

        Bay1 = 3,

        Shutter2 = 4,

        Bay2 = 5,

        Shutter3 = 6,

        Bay3 = 7,
    }

    public enum InverterSensors
    {
        #region INFO ANG Inverter Inputs

        /// <summary>
        /// S1IND-STO (hardware)
        /// </summary>
        ANG_HardwareSensorSTO = 0,

        /// <summary>
        /// S2IND-SS1 (hardware)
        /// </summary>
        ANG_HardwareSensorSS1 = 1,

        /// <summary>
        /// S3IND-Sensore zero elevatore
        /// </summary>
        ANG_ZeroElevatorSensor = 2,

        OneKMachineZeroCradle = 2,

        /// <summary>
        /// S4IND-Encoder canale B
        /// </summary>
        ANG_EncoderChannelBCradle = 3,

        /// <summary>
        /// S5IND-Encoder canale A
        /// </summary>
        ANG_EncoderChannelACradle = 4,

        /// <summary>
        /// S6IND-Encoder canale Z
        /// </summary>
        ANG_ElevatorOverrunSensor = 5,

        /// <summary>
        /// MF3IND-Extracorsa elevatore
        /// </summary>
        //ANG_HardwareSensorSTO = 6,

        /// <summary>
        /// S5IND-Taratura barriera
        /// </summary>
        //ANG_BarrierCalibration = 4,

        /// <summary>
        /// MF2IND-Sensore zero culla
        /// </summary>
        ANG_ZeroCradleSensor = 7,

        /// <summary>
        /// S7IND-STO (hardware)
        /// </summary>
        //ANG_HardwareSensorSTOB = 6,

        /// <summary>
        /// MFI1-Barriera ottica di misura
        /// </summary>
        //ANG_OpticalMeasuringBarrier = 7,

        /// <summary>
        /// MF2-Presenza cassetto su culla lato macchina
        /// </summary>
        //ANG_PresenceDrawerOnCradleMachineSide = 8,

        /// <summary>
        /// MF3-Presenza cassetto su culla lato operatore
        /// </summary>
        //ANG_PresenceDraweronCradleOperatoreSide = 9,

        /// <summary>
        /// MF4-Temperatura motore elevatore
        /// </summary>
        //ANG_ElevatorMotorTemprature = 10,

        #endregion

        #region INFO AGL Inverter Inputs

        /// <summary>
        /// STO (hardware)
        /// </summary>
        AGL_HardwareSensorSTO = 0,

        /// <summary>
        /// IN1D-SS1 (hardware)
        /// </summary>
        AGL_HardwareSensorSS1 = 1, //12,

        /// <summary>
        /// IN2D-Sensore serranda (A)
        /// </summary>
        AGL_ShutterSensorA = 2, //13,

        /// <summary>
        /// IN3D-Sensore serranda (B)
        /// </summary>
        AGL_ShutterSensorB = 3, //14,

        /// <summary>
        /// IN4D-Libero
        /// </summary>
        AGL_FreeSensor1 = 4, //15,

        /// <summary>
        /// IN5D-Libero
        /// </summary>
        AGL_FreeSensor2 = 5, //16,

        /// <summary>
        /// MFI1-Libero
        /// </summary>
        AGL_FreeSensor3 = 6, //17,

        /// <summary>
        /// MFI2-Libero
        /// </summary>
        AGL_FreeSensor4 = 7, //18,

        /// <summary>
        /// STO (hardware)
        /// </summary>
        AGL_HardwareSensorSTOB = 8, //19,

        #endregion

        #region INFO ACU Inverter Inputs

        /// <summary>
        /// S1IND-STO (hardware)
        /// </summary>
        ACU_HardwareSensorSTO = 0,

        /// <summary>
        /// S2IND-SS1 (hardware)
        /// </summary>
        ACU_HardwareSensorSS1 = 1,

        /// <summary>
        /// S3IND-Sensore zero
        /// </summary>
        ACU_ZeroSensor = 2,

        /// <summary>
        /// S4IND-Encoder canale B
        /// </summary>
        ACU_EncoderChannelB = 3,

        /// <summary>
        /// S5IND-Encoder canale A
        /// </summary>
        ACU_EncoderChannelA = 4,

        /// <summary>
        /// S6IND-Encoder canale A
        /// </summary>
        ACU_ZeroSensorTop = 5,

        /// <summary>
        /// MF1IND-Libero
        /// </summary>
        ACU_FreeSensor1 = 6,

        /// <summary>
        /// EM-S1IND-Libero
        /// </summary>
        ACU_FreeSensor2 = 7,

        /// <summary>
        /// S7IND-STO (hardware)
        /// </summary>
        //ACU_HardwareSensorSTOB = 26,

        #endregion
    }

    public enum InverterType
    {
        Undefined,

        Ang,

        Agl,

        Acu
    }

    public class InverterModel : BindableBase
    {
        #region Fields

        public BitModel[] ioDeviceBay;

        public BitModel[] ioDeviceMain;

        private const int LOWER_SPEED_Y_AXIS = 17928;

        private readonly Dictionary<Axis, double> axisPosition;

        private readonly Timer homingTimer;

        private readonly Timer shutterTimer;

        private readonly double target0_extBay = 0.0d;

        private readonly Timer targetTimer;

        private InverterCalibrationMode calibrationMode;

        private int controlWord;

        private BitModel[] controlWordArray;

        private Axis currentAxis;

        private ObservableCollection<BitModel> digitalIO = new ObservableCollection<BitModel>();

        private bool enabled;

        private bool homingTimerActive;

        private ICommand inverterInFaultCommand;

        private InverterType inverterType;

        private bool isDouble = false;

        private bool isExternal = false;

        private MAS.DataModels.Machine machine;

        private InverterOperationMode operationMode;

        private bool shutterTimerActive;

        private int statusWord;

        private double targetRace_extBay = 1100.0d;

        private bool targetTimerActive;

        #endregion

        #region Constructors

        public InverterModel(InverterType inverterType)
        {
            this.InverterType = inverterType;
            this.Enabled = true;

            this.homingTimer = new Timer(this.HomingTick, null, -1, Timeout.Infinite);
            this.targetTimer = new Timer(this.TargetTick, null, -1, Timeout.Infinite);
            this.shutterTimer = new Timer(this.ShutterTick, null, -1, Timeout.Infinite);

            this.digitalIO.Add(new BitModel("00", false, GetInverterSignalDescription(inverterType, 0)));
            this.digitalIO.Add(new BitModel("01", false, GetInverterSignalDescription(inverterType, 1)));
            this.digitalIO.Add(new BitModel("02", false, GetInverterSignalDescription(inverterType, 2)));
            this.digitalIO.Add(new BitModel("03", false, GetInverterSignalDescription(inverterType, 3)));
            this.digitalIO.Add(new BitModel("04", false, GetInverterSignalDescription(inverterType, 4)));
            this.digitalIO.Add(new BitModel("05", false, GetInverterSignalDescription(inverterType, 5)));
            this.digitalIO.Add(new BitModel("06", false, GetInverterSignalDescription(inverterType, 6)));
            this.digitalIO.Add(new BitModel("07", false, GetInverterSignalDescription(inverterType, 7)));

            this.OperationMode = InverterOperationMode.Velocity;
            switch (inverterType)
            {
                case InverterType.Ang:
                    // Remove overrun signal
                    this.digitalIO[(int)InverterSensors.ANG_ElevatorOverrunSensor].Value = true;
                    break;

                case InverterType.Agl:
                    break;

                case InverterType.Acu:
                    this.OperationMode = InverterOperationMode.Position;
                    break;

                default:
                    break;
            }

            this.currentAxis = Axis.Horizontal;

            this.axisPosition = new Dictionary<Axis, double>();
            this.axisPosition.Add(Axis.Horizontal, 0);
            this.axisPosition.Add(Axis.Vertical, 0);

            if (inverterType != InverterType.Agl)
            {
                this.HorizontalZeroSensor(true);
            }

            this.TargetPosition = new Dictionary<Axis, double>();
            this.TargetPosition.Add(Axis.Horizontal, 0);
            this.TargetPosition.Add(Axis.Vertical, 0);

            this.StartPosition = new Dictionary<Axis, double>();
            this.StartPosition.Add(Axis.Horizontal, 0);
            this.StartPosition.Add(Axis.Vertical, 0);

            this.TargetAcceleration = new Dictionary<Axis, int>();
            this.TargetAcceleration.Add(Axis.Horizontal, 0);
            this.TargetAcceleration.Add(Axis.Vertical, 0);

            this.TargetDeceleration = new Dictionary<Axis, int>();
            this.TargetDeceleration.Add(Axis.Horizontal, 0);
            this.TargetDeceleration.Add(Axis.Vertical, 0);

            this.TargetSpeed = new Dictionary<Axis, int>();
            this.TargetSpeed.Add(Axis.Horizontal, 0);
            this.TargetSpeed.Add(Axis.Vertical, 0);

            this.SwitchPositions = new Dictionary<Axis, double[]>();
            this.SwitchPositions.Add(Axis.Horizontal, new double[5]);
            this.SwitchPositions.Add(Axis.Vertical, new double[5]);
        }

        #endregion

        #region Events

        public event EventHandler<HorizontalMovementEventArgs> OnHorizontalMovementComplete;

        #endregion

        #region Properties

        public int AxisChanged { get; set; }

        public double AxisPosition
        {
            get => this.axisPosition[this.IsHorizontalAxis ? Axis.Horizontal : Axis.Vertical];
            set
            {
                this.axisPosition[this.IsHorizontalAxis ? Axis.Horizontal : Axis.Vertical] = value;
                this.RaisePropertyChanged(nameof(this.AxisPosition));

                if (this.IsHorizontalAxis)
                {
                    this.RaisePropertyChanged(nameof(this.AxisPositionX));
                }
                else
                {
                    this.RaisePropertyChanged(nameof(this.AxisPositionY));
                }
            }
        }

        public double AxisPositionX { get => this.axisPosition[Axis.Horizontal]; set { var item = this.axisPosition[Axis.Horizontal]; this.SetProperty(ref item, value); } }

        public double AxisPositionY { get => this.axisPosition[Axis.Vertical]; set { var item = this.axisPosition[Axis.Vertical]; this.SetProperty(ref item, value); } }

        public List<InverterBlockDefinition> BlockDefinitions { get; set; }

        public InverterCalibrationMode CalibrationMode
        {
            get => this.calibrationMode;
            set => this.SetProperty(ref this.calibrationMode, value);
        }

        public int ControlWord
        {
            get => this.controlWord;
            set => this.SetProperty(ref this.controlWord, value);
        }

        public BitModel[] ControlWordArray => this.controlWordArray ?? (this.controlWordArray = this.RefreshControlWordArray());

        public Axis CurrentAxis { get => this.currentAxis; set => this.SetProperty(ref this.currentAxis, value); }

        public ObservableCollection<BitModel> DigitalIO
        {
            get => this.digitalIO;
            set => this.SetProperty(ref this.digitalIO, value);
        }

        public bool Enabled { get => this.enabled; set => this.SetProperty(ref this.enabled, value); }

        public int Id { get; set; }

        public double ImpulsesEncoderPerRound { get; set; }

        public ICommand InverterInFaultCommand => this.inverterInFaultCommand ?? (this.inverterInFaultCommand = new DelegateCommand(() => this.InverterInFault()));

        public InverterRole InverterRole => (InverterRole)this.Id;

        public InverterType InverterType
        {
            get => this.inverterType;
            set => this.inverterType = value;
        }

        public bool IsDouble
        {
            get => this.isDouble;
            set => this.isDouble = value;
        }

        public bool IsElevatorOverrun
        {
            get => !this.digitalIO[(int)InverterSensors.ANG_ElevatorOverrunSensor].Value;
        }

        public bool IsExternal
        {
            get => this.isExternal;
            set => this.isExternal = value;
        }

        public bool IsFault
        {
            get => (this.statusWord & 0x0008) > 0;
            set
            {
                if (value)
                {
                    this.statusWord |= 0x0008;
                }
                else
                {
                    this.statusWord &= ~0x0008;
                }
                this.RaisePropertyChanged(nameof(this.IsFault));
                this.RaisePropertyChanged(nameof(this.StatusWord));
                this.RaisePropertyChanged(nameof(this.StatusWordArray));
            }
        }

        public bool IsHorizontalAxis => (this.ControlWord & 0x8000) > 0 || this.Id > 0;

        public bool IsOperationEnabled
        {
            get => (this.StatusWord & 0x0004) > 0;
            set
            {
                if (value)
                {
                    this.StatusWord |= 0x0004;
                }
                else
                {
                    this.StatusWord &= ~0x0004;
                }
            }
        }

        public bool IsQuickStopTrue
        {
            get => (this.StatusWord & 0x0020) > 0;
            set
            {
                if (value)
                {
                    this.StatusWord |= 0x0020;
                }
                else
                {
                    this.StatusWord &= ~0x0020;
                }
            }
        }

        public bool IsReadyToSwitchOn
        {
            get => (this.StatusWord & 0x0001) > 0;
            set
            {
                if (value)
                {
                    this.StatusWord |= 0x0001;
                }
                else
                {
                    this.StatusWord &= ~0x0001;
                }
            }
        }

        public bool IsRelativeMovement => (this.ControlWord & 0x0040) > 0;

        public bool IsRemote => (this.statusWord & 0x0200) > 0;

        public bool IsShutterClosed => !this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value && !this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value;

        public bool IsShutterHalf => this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value && !this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value;

        public bool IsShutterOpened => !this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value && this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value;

        public bool IsStartedOnBoard { get; set; }

        public bool IsSwitchedOn
        {
            get => (this.statusWord & 0x0002) > 0;
            set
            {
                if (value)
                {
                    this.StatusWord |= 0x0002;
                }
                else
                {
                    this.StatusWord &= ~0x0002;
                }
            }
        }

        public bool IsSwitchOnDisabled => (this.statusWord & 0x0040) > 0;

        public bool IsTargetReached
        {
            get => (this.StatusWord & 0x0400) > 0;
            set
            {
                if (value)
                {
                    this.StatusWord |= 0x0400;
                }
                else
                {
                    this.StatusWord &= ~0x0400;
                }
            }
        }

        public bool IsVoltageEnabled
        {
            get => (this.StatusWord & 0x0010) > 0;
            set
            {
                if (value)
                {
                    this.StatusWord |= 0x0010;
                }
                else
                {
                    this.StatusWord &= ~0x0010;
                }
            }
        }

        public bool IsWarning => (this.statusWord & 0x0080) > 0;

        public bool IsWarning2 => (this.statusWord & 0x8000) > 0;

        public MAS.DataModels.Machine Machine
        {
            get { return this.machine; }
            set
            {
                this.machine = value;

                if (this.machine != null)
                {
                    switch (this.InverterRole)
                    {
                        case InverterRole.Main:
                            {
                                this.ImpulsesEncoderPerRound = this.machine.Elevator.Axes.First().Resolution;
                            }
                            break;

                        case InverterRole.ElevatorChain:
                            this.ImpulsesEncoderPerRound = this.machine.Elevator.Axes.Last().Resolution;
                            this.Enabled = this.machine.Elevator.Axes.Count(x => x.Inverter != null) > 1;
                            break;

                        case InverterRole.Shutter1:
                            this.Enabled = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayOne)?.Shutter != null;
                            break;

                        case InverterRole.Shutter2:
                            {
                                this.Enabled = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayTwo)?.Shutter != null;
                            }
                            break;

                        case InverterRole.Shutter3:
                            {
                                this.Enabled = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayThree)?.Shutter != null;
                            }
                            break;

                        case InverterRole.Bay1:
                            this.ImpulsesEncoderPerRound = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayOne)?.Resolution ?? this.ImpulsesEncoderPerRound;
                            this.Enabled = this.machine.Bays.Any(x => x.Number == BayNumber.BayOne && x.Inverter != null);
                            this.IsExternal = this.machine.Bays.Any(x => x.Number == BayNumber.BayOne && x.Inverter != null && x.IsExternal);
                            this.IsDouble = this.machine.Bays.Any(x => x.Number == BayNumber.BayOne && x.Inverter != null && x.Positions.Count() == 2);
                            this.targetRace_extBay = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayOne)?.External?.Race ?? this.targetRace_extBay;
                            break;

                        case InverterRole.Bay2:
                            this.ImpulsesEncoderPerRound = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayTwo)?.Resolution ?? this.ImpulsesEncoderPerRound;
                            this.Enabled = this.machine.Bays.Any(x => x.Number == BayNumber.BayTwo && x.Inverter != null);
                            this.IsExternal = this.machine.Bays.Any(x => x.Number == BayNumber.BayTwo && x.Inverter != null && x.IsExternal);
                            this.IsDouble = this.machine.Bays.Any(x => x.Number == BayNumber.BayTwo && x.Inverter != null && x.Positions.Count() == 2);
                            this.targetRace_extBay = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayTwo)?.External?.Race ?? this.targetRace_extBay;

                            break;

                        case InverterRole.Bay3:
                            this.ImpulsesEncoderPerRound = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayThree)?.Resolution ?? this.ImpulsesEncoderPerRound;
                            this.Enabled = this.machine.Bays.Any(x => x.Number == BayNumber.BayThree && x.Inverter != null);
                            this.IsExternal = this.machine.Bays.Any(x => x.Number == BayNumber.BayThree && x.Inverter != null && x.IsExternal);
                            this.IsDouble = this.machine.Bays.Any(x => x.Number == BayNumber.BayThree && x.Inverter != null && x.Positions.Count() == 2);
                            this.targetRace_extBay = this.machine.Bays.FirstOrDefault(x => x.Number == BayNumber.BayThree)?.External?.Race ?? this.targetRace_extBay;
                            break;
                    }
                }
            }
        }

        public InverterOperationMode OperationMode
        {
            get => this.operationMode;
            set => this.SetProperty(ref this.operationMode, value);
        }

        public int SpeedRate { get; set; }

        public Dictionary<Axis, double> StartPosition { get; set; }

        public int StatusWord
        {
            get => this.statusWord;
            set => this.SetProperty(ref this.statusWord, value, () =>
            {
                this.RaisePropertyChanged(nameof(this.IsFault));
                this.RaisePropertyChanged(nameof(this.StatusWordArray));
            });
        }

        public BitModel[] StatusWordArray => (from x in Enumerable.Range(0, 16)
                                              let binary = Convert.ToString(this.StatusWord, 2).PadLeft(16, '0')
                                              select new { Value = binary[x] == '1' ? true : false, Description = (15 - x).ToString(), Index = (15 - x) })
                                               .Select(x => new BitModel(x.Index.ToString("00"), x.Value, GetStatusWordSignalDescription(this.OperationMode, x.Index))).Reverse().ToArray();

        public Dictionary<Axis, double[]> SwitchPositions { get; set; }

        public int TableIndex { get; set; }

        public Dictionary<Axis, int> TargetAcceleration { get; set; }

        public Dictionary<Axis, int> TargetDeceleration { get; set; }

        public Dictionary<Axis, double> TargetPosition { get; set; }

        public int TargetShutterPosition { get; set; }

        public Dictionary<Axis, int> TargetSpeed { get; set; }

        public int TorqueCurrent { get; set; }

        #endregion

        #region Methods

        public void BuildHomingStatusWord()
        {
            //StartHoming && EnableOperation
            if ((this.ControlWord & 0x0010) > 0 && (this.ControlWord & 0x0008) > 0)
            {
                if (!this.homingTimerActive)
                {
                    this.TargetPosition[Axis.Vertical] = 0;// + new Random().Next(-5, 15);
                    this.TargetPosition[Axis.Horizontal] = this.AxisPosition;
                    if (this.calibrationMode == InverterCalibrationMode.FindSensor ||
                        this.calibrationMode == InverterCalibrationMode.FindSensorCarousel
                        )
                    {
                        //this.TargetPosition[Axis.Horizontal] += new Random().Next(-5, 15);
                    }
                    this.homingTimerActive = true;
                    this.homingTimer.Change(0, 500);
                }
            }
            else
            {
                if (this.homingTimerActive)
                {
                    this.homingTimer.Change(-1, Timeout.Infinite);
                    this.homingTimerActive = false;
                }
                // Reset HomingAttained
                this.StatusWord &= 0xEFFF;
            }
        }

        public void BuildPositionStatusWord()
        {
            //New SetPoint
            if ((this.ControlWord & 0x0010) > 0 && (this.ControlWord & 0x0008) > 0)
            {
                if (!this.targetTimerActive && (this.StatusWord & 0x1000) == 0)
                {
                    this.targetTimer.Change(0, 50);
                    this.targetTimerActive = true;
                }
            }
            else
            {
                if (this.targetTimerActive)
                {
                    this.targetTimer.Change(-1, Timeout.Infinite);
                    this.targetTimerActive = false;
                }
            }
        }

        public void BuildTableTravelStatusWord()
        {
            //StartMotionBlock
            if ((this.ControlWord & 0x0200) > 0 && (this.ControlWord & 0x0008) > 0)
            {
                this.statusWord |= 0x0100;  // motion block in progress on
                if (!this.targetTimerActive)
                {
                    this.targetTimer.Change(0, 50);
                    this.targetTimerActive = true;
                }
            }
            else
            {
                if (this.targetTimerActive)
                {
                    this.targetTimer.Change(-1, Timeout.Infinite);
                    this.targetTimerActive = false;
                }
                this.statusWord &= ~0x0100;  // motion block in progress off
            }
        }

        public void BuildVelocityStatusWord()
        {
            //EnableOperation
            if ((this.ControlWord & 0x0008) > 0)
            {
                this.StatusWord |= 0x0004;
                if (!this.shutterTimerActive && !this.IsTargetReached)
                {
                    this.shutterTimer.Change(0, 1000);
                    this.shutterTimerActive = true;
                }
            }
            else
            {
                if (this.shutterTimerActive)
                {
                    this.shutterTimer.Change(-1, Timeout.Infinite);
                    this.shutterTimerActive = false;
                }
            }
        }

        public void EmergencyStop()
        {
            this.homingTimer.Change(-1, Timeout.Infinite);
            this.homingTimerActive = false;

            this.shutterTimer.Change(-1, Timeout.Infinite);
            this.shutterTimerActive = false;

            this.targetTimer.Change(-1, Timeout.Infinite);
            this.targetTimerActive = false;

            this.DigitalIO[(int)InverterSensors.ACU_HardwareSensorSTO].Value = false;
            this.StatusWord = 0;
        }

        public int GetDigitalIO()
        {
            var result = 0;
            for (var i = 0; i < this.DigitalIO.Count; i++)
            {
                if (this.DigitalIO[i].Value)
                {
                    result += (int)Math.Pow(2, i);
                }
            }
            return result;
        }

        public double Impulses2millimeters(int value)
        {
            var resolution = this.ImpulsesEncoderPerRound;
            return value / resolution;
        }

        public int Millimeters2Impulses(double value)
        {
            var resolution = this.ImpulsesEncoderPerRound;
            return (int)Math.Round(value * resolution);
        }

        public BitModel[] RefreshControlWordArray()
        {
            var cw = (from x in Enumerable.Range(0, 16)
                      let binary = Convert.ToString(this.ControlWord, 2).PadLeft(16, '0')
                      select new { Value = binary[x] == '1' ? true : false, Description = (15 - x).ToString(), Index = (15 - x) })
                     .Select(x => new BitModel(x.Index.ToString("00"), x.Value, GetControlWordSignalDescription(this.OperationMode, x.Index))).Reverse().ToArray();

            if (this.controlWordArray != null)
            {
                for (var i = 0; i < cw.Length; i++)
                {
                    this.controlWordArray[i].Value = cw[i].Value;
                }
            }

            return cw;
        }

        internal static string GetControlWordSignalDescription(InverterOperationMode operationMode, int signalIndex)
        {
            switch (signalIndex)
            {
                case 0:
                    return Inverter.SwitchedOn;

                case 1:
                    return Inverter.EnableVoltage;

                case 2:
                    return Inverter.QuickStop;

                case 3:
                    return Inverter.EnableOperation;

                case 4:
                    return operationMode == InverterOperationMode.Velocity ? Inverter.RfgEnable : operationMode == InverterOperationMode.Position ? Inverter.NewSetPoint : operationMode == InverterOperationMode.Homing ? Inverter.HomingStarted : operationMode == InverterOperationMode.TableTravel ? Inverter.SequenceMode : Inverter.OperationModeSpecific;

                case 5:
                    return operationMode == InverterOperationMode.Velocity ? Inverter.RfgUnlock : operationMode == InverterOperationMode.Position ? Inverter.ChangeSetImmediately : Inverter.OperationModeSpecific;

                case 6:
                    return operationMode == InverterOperationMode.Velocity ? Inverter.RfgUseRef : operationMode == InverterOperationMode.Position ? Inverter.AbsoluteRelative : operationMode == InverterOperationMode.TableTravel ? Inverter.Resume : Inverter.OperationModeSpecific;

                case 7:
                    return Inverter.ResetFault;

                case 8:
                    return Inverter.Halt;

                case 9:
                    return operationMode == InverterOperationMode.Position ? Inverter.ChangeOnSetPoint : operationMode == InverterOperationMode.TableTravel ? Inverter.StartMotionBlock : Inverter.OperationModeSpecific;

                case 10:
                    return Inverter.HeartBeat;

                case 11:
                    return operationMode == InverterOperationMode.TableTravel ? Inverter.MotionBlockSelect0 : Inverter.Free;

                case 12:
                    return operationMode == InverterOperationMode.TableTravel ? Inverter.MotionBlockSelect1 : Inverter.Free;

                case 13:
                    return operationMode == InverterOperationMode.TableTravel ? Inverter.MotionBlockSelect2 : Inverter.Free;

                case 14:
                    return operationMode == InverterOperationMode.TableTravel ? Inverter.MotionBlockSelect3 : Inverter.Free;

                case 15:
                    return Inverter.HorizontalAxis;
            }

            return Inverter.Free;
        }

        internal static string GetInverterSignalDescription(InverterType inverterType, int signalIndex)
        {
            switch (signalIndex)
            {
                case 0:
                    return Inverter.PowerOn;

                case 1:
                    return Inverter.NormalFunction;

                case 2:
                    return inverterType == InverterType.Ang ? Inverter.ElevatorZeroPosition : inverterType == InverterType.Agl ? Inverter.ShutterSensorA : Inverter.ZeroPosition;

                case 3:
                    return inverterType == InverterType.Ang ? Inverter.EncoderChannelBElevatorChain : inverterType == InverterType.Agl ? Inverter.ShutterSensorB : Inverter.EncoderChannelB;

                case 4:
                    return inverterType == InverterType.Ang ? Inverter.EncoderChannelAElevatorChain : inverterType == InverterType.Agl ? Inverter.Free : Inverter.EncoderChannelA;

                case 5:
                    return inverterType == InverterType.Ang ? Inverter.ElevatorExtraStroke : inverterType == InverterType.Agl ? Inverter.Free : inverterType == InverterType.Acu ? Inverter.ZeroPositionUp : Inverter.Free;

                case 6:
                    return Inverter.Free;

                case 7:
                    return inverterType == InverterType.Ang ? Inverter.SensorElevatorChainZero : inverterType == InverterType.Agl ? Inverter.Free : Inverter.Free;

                default:
                    return string.Empty;
            }
        }

        internal static string GetStatusWordSignalDescription(InverterOperationMode operationMode, int signalIndex)
        {
            switch (signalIndex)
            {
                case 0:
                    return Inverter.ReadyToSwitchOn;

                case 1:
                    return Inverter.SwitchedOn;

                case 2:
                    return Inverter.OperationEnabled;

                case 3:
                    return Inverter.Fault;

                case 4:
                    return Inverter.VoltageEnabled;

                case 5:
                    return Inverter.QuickStop;

                case 6:
                    return Inverter.SwitchOnDisabled;

                case 7:
                    return Inverter.Warning;

                case 8:
                    return operationMode == InverterOperationMode.TableTravel ? Inverter.MotionBlockInProgress : Inverter.ManufacturerSpecific;

                case 9:
                    return Inverter.Remote;

                case 10:
                    return Inverter.TargetReached;

                case 11:
                    return operationMode == InverterOperationMode.TableTravel ? Inverter.InternalLimitActive_bis : Inverter.InternalLimitActive;

                case 12:
                    return operationMode == InverterOperationMode.ProfileVelocity ? Inverter.Velocity : operationMode == InverterOperationMode.Position ? Inverter.SetPointAck : operationMode == InverterOperationMode.Homing ? Inverter.HomingAttained : operationMode == InverterOperationMode.TableTravel ? Inverter.InGear : Inverter.OperationModeSpecific;

                case 13:
                    return operationMode == InverterOperationMode.ProfileVelocity ? Inverter.MaxSlippage : (operationMode == InverterOperationMode.Position || operationMode == InverterOperationMode.TableTravel) ? Inverter.FollowingError : operationMode == InverterOperationMode.Homing ? Inverter.HomingError : Inverter.OperationModeSpecific;

                case 14:
                    return Inverter.ManufacturerSpecific;

                case 15:
                    return Inverter.ManufacturerSpecificWarning2;
            }

            return "Free";
        }

        private void HomingTick(object state)
        {
            if (!this.homingTimerActive)
            {
                return;
            }

            var increment = 150d;
            if (Math.Abs(this.TargetPosition[this.currentAxis] - this.AxisPosition) < 1)
            {
                increment = 0.1;
            }
            else if (Math.Abs(this.TargetPosition[this.currentAxis] - this.AxisPosition) <= 10)
            {
                increment = 1;
            }
            else if (Math.Abs(this.TargetPosition[this.currentAxis] - this.AxisPosition) <= 150)
            {
                increment = 10;
            }

            if (this.AxisPosition < this.TargetPosition[this.currentAxis])
            {
                this.AxisPosition += increment;
            }
            else if (this.AxisPosition > this.TargetPosition[this.currentAxis])
            {
                this.AxisPosition -= increment;
            }

            if (Math.Abs(this.TargetPosition[this.currentAxis] - this.AxisPosition) <= 0.1)
            {
                this.StatusWord |= 0x1000;          // Set TargetReached

                if (this.currentAxis == Axis.Horizontal &&
                    !this.ioDeviceMain[(int)IoPorts.DrawerInMachineSide].Value &&
                    !this.ioDeviceMain[(int)IoPorts.DrawerInOperatorSide].Value)
                {
                    if (this.Id == 0)
                    {
                        this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = true;
                    }
                    else
                    {
                        this.DigitalIO[(int)InverterSensors.OneKMachineZeroCradle].Value = true;
                    }
                }

                if (this.InverterRole > InverterRole.ElevatorChain)
                {
                    if (this.IsExternal)
                    {
                        this.AxisPosition = this.Machine.Bays.FirstOrDefault(b => b.External != null).ChainOffset;
                    }
                    else
                    {
                        this.AxisPosition = this.Machine.Bays.FirstOrDefault(b => b.Carousel != null).ChainOffset;
                    }
                    this.HorizontalZeroSensor(true);
                }
                else
                {
                    this.AxisPosition = 0;
                }

                this.homingTimerActive = false;
                this.homingTimer.Change(-1, Timeout.Infinite);
            }
            else
            {
                this.StatusWord &= 0xEFFF;          // Reset TargetReached
            }
        }

        private void HorizontalZeroSensor(bool force)
        {
            if ((this.OperationMode != InverterOperationMode.TableTravel &&
                this.CurrentAxis == Axis.Horizontal) ||
                force)
            {
                if (force
                    //|| (this.ioDeviceMain != null
                    //    && this.InverterRole <= InverterRole.ElevatorChain
                    //    && !this.ioDeviceMain[(int)IoPorts.DrawerInOperatorSide].Value
                    //    && !this.ioDeviceMain[(int)IoPorts.DrawerInMachineSide].Value
                    //    )
                    )
                {
                    if (this.InverterType == InverterType.Ang)
                    {
                        this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = true;
                    }
                    else
                    {
                        this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? true : false;
                    }
                }
                //else
                //{
                //    if (this.InverterType == InverterType.Ang)
                //    {
                //        this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = false;
                //    }
                //    else
                //    {
                //        this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? false : true;
                //    }
                //}
            }
        }

        private void InverterInFault()
        {
            this.IsFault = !this.IsFault;
            if (this.IsFault)
            {
                this.ioDeviceMain[(int)IoPorts.NormalState].Value = false;
                this.IsReadyToSwitchOn = false;
                this.IsSwitchedOn = false;
                this.IsVoltageEnabled = false;
                this.IsOperationEnabled = false;
            }
        }

        private void ShutterTick(object state)
        {
            if (!this.shutterTimerActive)
            {
                return;
            }

            if (this.TargetShutterPosition == (int)ShutterPosition.Opened ||
               (this.TargetShutterPosition == (int)ShutterPosition.Half && this.AxisPosition <= 4))
            {
                this.AxisPosition++;
            }
            else if (this.TargetShutterPosition == (int)ShutterPosition.Closed ||
                    (this.TargetShutterPosition == (int)ShutterPosition.Half && this.AxisPosition >= 6))
            {
                this.AxisPosition--;
            }

            // Shutter position
            if (this.AxisPosition <= 0)
            {
                this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value = false;
                this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value = false;
            }
            else if (this.AxisPosition >= 4 && this.AxisPosition <= 6)
            {
                this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value = true;
                this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value = false;
            }
            else if (this.AxisPosition >= 10)
            {
                this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value = false;
                this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value = true;
            }
            else
            {
                this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value = true;
                this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value = true;
            }

            if ((this.TargetShutterPosition == (int)ShutterPosition.Closed && this.IsShutterClosed) ||
                (this.TargetShutterPosition == (int)ShutterPosition.Opened && this.IsShutterOpened) ||
                (this.TargetShutterPosition == (int)ShutterPosition.Half && this.IsShutterHalf)
                )
            {
                this.ControlWord &= 0xFFEF; // Reset Rfg Enable Signal
                this.IsTargetReached = true;

                this.shutterTimer.Change(-1, Timeout.Infinite);

                this.shutterTimerActive = false;
            }
            else
            {
                this.IsTargetReached = false;
            }
        }

        private void TargetTick(object state)
        {
            if (!this.targetTimerActive)
            {
                return;
            }

            var target = this.TargetPosition[this.currentAxis];
            if (this.IsRelativeMovement)
            {
                target += this.StartPosition[this.currentAxis];
            }
            double increment = 1;
            //if (this.TargetSpeed[this.currentAxis] >= LOWER_SPEED_Y_AXIS &&
            //    Math.Abs(target - this.AxisPosition) > (this.TargetSpeed[this.currentAxis] / LOWER_SPEED_Y_AXIS) * 10)
            //{
            //    increment = (this.TargetSpeed[this.currentAxis] / LOWER_SPEED_Y_AXIS) * 10;
            //}
            //else if (Math.Abs(target - this.AxisPosition) < 1)
            //{
            //    increment = 0.1;
            //}

            increment = (50.0d / 1000.0d) * (this.TargetSpeed[this.currentAxis] / this.ImpulsesEncoderPerRound); // space [mm]
            //if (Math.Abs(target - this.AxisPosition) < 20)
            //{
            //    increment = 0.5;
            //}
            //if (Math.Abs(target - this.AxisPosition) < 1)
            //{
            //    increment = 0.1;
            //}

            if (target > this.AxisPosition)
            {
                this.AxisPosition += increment;

                if (this.AxisPosition >= target)
                {
                    this.AxisPosition = target;
                }

                if (this.OperationMode == InverterOperationMode.TableTravel)
                {
                    if (!this.IsStartedOnBoard)
                    {
                        // simulate the loading process
                        if (this.AxisPosition > this.SwitchPositions[this.currentAxis][0] && this.AxisPosition < this.SwitchPositions[this.currentAxis][1])
                        {
                            this.ioDeviceMain[(int)IoPorts.DrawerInMachineSide].Value = true;
                            OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = true, IsLoadingExternal = false });
                            if (this.InverterType == InverterType.Ang)
                            {
                                this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = false;
                            }
                            else
                            {
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? false : true;
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            }
                        }
                        if (this.AxisPosition > this.SwitchPositions[this.currentAxis][3])
                        {
                            this.ioDeviceMain[(int)IoPorts.DrawerInOperatorSide].Value = true;
                            OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = false, IsLoadingExternal = false });
                            if (this.InverterType == InverterType.Ang)
                            {
                                this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = false;
                            }
                            else
                            {
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? false : true;
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            }
                        }
                    }
                    else
                    {
                        // simulate the unloading process
                        if (this.AxisPosition > this.SwitchPositions[this.currentAxis][0] && this.AxisPosition < this.SwitchPositions[this.currentAxis][1])
                        {
                            this.ioDeviceMain[(int)IoPorts.DrawerInMachineSide].Value = false;
                            OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = true, IsLoadingExternal = false });
                            if (this.InverterType == InverterType.Ang)
                            {
                                this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = false;
                            }
                            else
                            {
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.isExternal) ? false : true;
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            }
                        }
                        if (this.AxisPosition > this.SwitchPositions[this.currentAxis][3])
                        {
                            this.ioDeviceMain[(int)IoPorts.DrawerInOperatorSide].Value = false;
                            OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = true, IsLoadingExternal = false });
                            if (this.InverterType == InverterType.Ang)
                            {
                                this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = true;
                            }
                            else
                            {
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? true : false;
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            }
                        }
                    }
                }
                else if (this.InverterType == InverterType.Acu
                    && this.OperationMode == InverterOperationMode.Position
                    && this.Id > 1)
                {
                    if (!this.isExternal)
                    {
                        // bay chain. simulate the lift process
                        if (target - this.AxisPosition < 20)
                        {
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInBay].Value = false;
                        }
                        else if (target - this.AxisPosition > 20
                            && this.ioDeviceBay[(int)IoPorts.LoadingUnitInLowerBay].Value
                            )
                        {
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInLowerBay].Value = false;
                        }
                    }
                    else if (this.isExternal && this.isDouble)
                    {
                        if (Math.Abs(this.target0_extBay - Math.Abs(this.AxisPosition)) < 20)
                        {
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = false;
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = true;
                        }
                        if (Math.Abs(this.targetRace_extBay - Math.Abs(this.AxisPosition)) < 20)
                        {
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = true;
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = false;
                        }

                        OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = this.IsStartedOnBoard, IsLoadingExternal = !this.IsStartedOnBoard });
                    }
                    else
                    {
                        // external bay, simulate the Forward (TowardOperator) direction
                        if (Math.Abs(this.target0_extBay - Math.Abs(this.AxisPosition)) < 20)
                        {
                            // turn on the internal presence of drawer
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInLowerBay].Value = true;
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = false;
                        }
                        else
                        {
                            // turn off the internal presence of drawer
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInLowerBay].Value = false;
                        }
                        if (Math.Abs(this.targetRace_extBay - Math.Abs(this.AxisPosition)) < 20)
                        {
                            // turn on the external presence of drawer
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInBay].Value = false;
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = true;
                        }
                        else
                        {
                            // turn off the external presence of drawer
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInBay].Value = true;
                        }
                    }
                }
            }
            else
            {
                this.AxisPosition -= increment;

                if (this.AxisPosition <= target)
                {
                    this.AxisPosition = target;
                }

                if (this.OperationMode == InverterOperationMode.TableTravel)
                {
                    if (!this.IsStartedOnBoard)
                    {
                        // simulate the loading process
                        if (this.AxisPosition < this.SwitchPositions[this.currentAxis][0] && this.AxisPosition < this.SwitchPositions[this.currentAxis][1])
                        {
                            this.ioDeviceMain[(int)IoPorts.DrawerInMachineSide].Value = true;
                            OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = false, IsLoadingExternal = false });
                            if (this.InverterType == InverterType.Ang)
                            {
                                this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = false;
                            }
                            else
                            {
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? false : true;
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            }
                        }
                        if (this.AxisPosition < this.SwitchPositions[this.currentAxis][3])
                        {
                            this.ioDeviceMain[(int)IoPorts.DrawerInOperatorSide].Value = true;
                            OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = false, IsLoadingExternal = false });
                            if (this.InverterType == InverterType.Ang)
                            {
                                this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = false;
                            }
                            else
                            {
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? false : true;
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            }
                        }
                    }
                    else
                    {
                        // simulate the unloading process
                        if (this.AxisPosition < this.SwitchPositions[this.currentAxis][0] && this.AxisPosition < this.SwitchPositions[this.currentAxis][1])
                        {
                            this.ioDeviceMain[(int)IoPorts.DrawerInMachineSide].Value = false;
                            OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = true, IsLoadingExternal = false });
                            if (this.InverterType == InverterType.Ang)
                            {
                                this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = false;
                            }
                            else
                            {
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? false : true;
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            }
                        }
                        if (this.AxisPosition < this.SwitchPositions[this.currentAxis][3])
                        {
                            this.ioDeviceMain[(int)IoPorts.DrawerInOperatorSide].Value = false;
                            OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = true, IsLoadingExternal = false });
                            if (this.InverterType == InverterType.Ang)
                            {
                                this.DigitalIO[(int)InverterSensors.ANG_ZeroCradleSensor].Value = true;
                            }
                            else
                            {
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? true : false;
                                this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                            }
                        }
                    }
                }
                else if (this.InverterType == InverterType.Acu
                    && this.OperationMode == InverterOperationMode.Position
                    && this.Id > 1)
                {
                    if (this.isExternal && this.isDouble)
                    {
                        if (Math.Abs(this.target0_extBay - Math.Abs(this.AxisPosition)) < 20)
                        {
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? true : false;
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                        }
                        else
                        {
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? false : true;
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensorTop].Value = !this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value;
                        }

                        OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = !this.IsStartedOnBoard, IsLoadingExternal = this.IsStartedOnBoard });
                    }
                    else if (this.isExternal && !this.isDouble)
                    {
                        // external bay, simulate the Backward (TowardMachine) direction
                        if (Math.Abs(this.target0_extBay - Math.Abs(this.AxisPosition)) < 20)
                        {
                            // turn on the internal presence of drawer
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInLowerBay].Value = true;
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? true : false;
                        }
                        else
                        {
                            // turn off the internal presence of drawer
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInLowerBay].Value = false;
                            this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? false : true;
                        }

                        if (Math.Abs(this.targetRace_extBay - Math.Abs(this.AxisPosition)) < 20)
                        {
                            // turn on the external presence of drawer
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInBay].Value = false;
                        }
                        else
                        {
                            // turn off the external presence of drawer
                            this.ioDeviceBay[(int)IoPorts.LoadingUnitInBay].Value = true;
                        }
                    }
                }
            }

            //if (this.AxisPosition == 0 && this.InverterRole == InverterRole.Main)
            //{
            //    // TEST!!!
            //    this.HorizontalZeroSensor(true);
            //}
            //else
            {
                this.HorizontalZeroSensor(false);
            }
            if (Math.Abs(target - this.AxisPosition) <= 0.1)
            {
                this.AxisPosition = target;
                if (this.OperationMode == InverterOperationMode.TableTravel)
                {
                    // simulate positioning error
                    //this.AxisPosition += (short)(new Random().Next(-3, 3));
                    //OnHorizontalMovementComplete?.Invoke(this, new HorizontalMovementEventArgs() { IsLoading = !this.IsStartedOnBoard });
                }
                else if (this.InverterRole > InverterRole.ElevatorChain)
                {
                    // simulate bay chain error: comment next line
                    //this.DigitalIO[(int)InverterSensors.ACU_ZeroSensor].Value = (!this.IsExternal) ? true : false;
                }
                this.ControlWord &= 0xFFEF;     // Reset Rfg Enable Signal
                this.StatusWord |= 0x1000;      // Set Point Ack
                this.IsTargetReached = true;
                this.targetTimer.Change(-1, Timeout.Infinite);
                this.targetTimerActive = false;
            }
            else
            {
                this.IsTargetReached = false;
            }
        }

        #endregion
    }
}
