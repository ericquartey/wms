using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services.Models
{
    public enum InverterOperationMode : ushort
    {
        Position = 0x0001,

        Homing = 0x0006,

        Velocity = 0x0002,

        ProfileVelocity = 0x0003
    }

    public enum InverterParameterId : short
    {
        ControlWordParam = 410, //INFO:Writeonly

        HomingCreepSpeedParam = 1133,

        HomingFastSpeedParam = 1132,

        HomingAcceleration = 1134,

        PositionAccelerationParam = 1457,

        PositionDecelerationParam = 1458,

        PositionTargetPositionParam = 1455,

        PositionTargetSpeedParam = 1456,

        SetOperatingModeParam = 1454,

        ShutterTargetVelocityParam = 480,

        StatusWordParam = 411, //19B INFO:Readonly

        ActualPositionShaft = 1108,

        StatusDigitalSignals = 250,

        DigitalInputsOutputs = 1411,

        ShutterTargetPosition = 414 // 19E
    }

    public enum InverterRole
    {
        Main = 0,

        Chain = 1,

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
        ANG_EncoderChannelZCradle = 5,

        /// <summary>
        /// MF3IND-Extracorsa elevatore
        /// </summary>
        ANG_OverrunElevatorSensor = 6,

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
        ACU_EncoderChannelZ = 5,

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

        public BitModel[] controlWordArray;

        private readonly Dictionary<Axis, int> axisPosition;

        private readonly Timer homingTimer;

        private readonly Timer shutterTimer;

        private readonly Timer targetTimer;

        private int controlWord;

        private Axis currentAxis;

        private ObservableCollection<BitModel> digitalIO = new ObservableCollection<BitModel>();

        private bool enabled;

        private ICommand inverterInFaultCommand;

        private InverterType inverterType;

        private InverterOperationMode operationMode;

        private bool positionReached;

        private int statusWord;

        #endregion

        #region Constructors

        public InverterModel(InverterType inverterType)
        {
            this.homingTimer = new Timer(this.HomingTick, null, -1, Timeout.Infinite);
            this.homingTimerActive = false;

            this.targetTimer = new Timer(this.TargetTick, null, -1, Timeout.Infinite);
            this.targetTimerActive = false;

            this.shutterTimer = new Timer(this.ShutterTick, null, -1, Timeout.Infinite);
            this.shutterTimerActive = false;

            this.OperationMode = InverterOperationMode.Velocity;
            this.InverterType = inverterType;

            this.digitalIO.Add(new BitModel("00", false, GetInverterSignalDescription(inverterType, 0)));
            this.digitalIO.Add(new BitModel("01", false, GetInverterSignalDescription(inverterType, 1)));
            this.digitalIO.Add(new BitModel("02", false, GetInverterSignalDescription(inverterType, 2)));
            this.digitalIO.Add(new BitModel("03", false, GetInverterSignalDescription(inverterType, 3)));
            this.digitalIO.Add(new BitModel("04", false, GetInverterSignalDescription(inverterType, 4)));
            this.digitalIO.Add(new BitModel("05", false, GetInverterSignalDescription(inverterType, 5)));
            this.digitalIO.Add(new BitModel("06", false, GetInverterSignalDescription(inverterType, 6)));
            this.digitalIO.Add(new BitModel("07", false, GetInverterSignalDescription(inverterType, 7)));

            // Remove overrun signal
            if (inverterType == InverterType.Ang)
            {
                this.digitalIO[(int)InverterSensors.ANG_OverrunElevatorSensor].Value = true;
            }

            this.currentAxis = Axis.Horizontal;

            this.axisPosition = new Dictionary<Axis, int>();
            this.axisPosition.Add(Axis.Horizontal, 0);
            this.axisPosition.Add(Axis.Vertical, 300);

            this.TargetPosition = new Dictionary<Axis, int>();
            this.TargetPosition.Add(Axis.Horizontal, 0);
            this.TargetPosition.Add(Axis.Vertical, 0);

            this.TargetAcceleration = new Dictionary<Axis, int>();
            this.TargetAcceleration.Add(Axis.Horizontal, 0);
            this.TargetAcceleration.Add(Axis.Vertical, 0);

            this.TargetDeceleration = new Dictionary<Axis, int>();
            this.TargetDeceleration.Add(Axis.Horizontal, 0);
            this.TargetDeceleration.Add(Axis.Vertical, 0);

            this.TargetSpeed = new Dictionary<Axis, int>();
            this.TargetSpeed.Add(Axis.Horizontal, 0);
            this.TargetSpeed.Add(Axis.Vertical, 0);
        }

        #endregion

        #region Properties

        public int AxisPosition
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

        public int AxisPositionX { get => this.axisPosition[Axis.Horizontal]; set { var item = this.axisPosition[Axis.Horizontal]; this.SetProperty(ref item, value); } }

        public int AxisPositionY { get => this.axisPosition[Axis.Vertical]; set { var item = this.axisPosition[Axis.Vertical]; this.SetProperty(ref item, value); } }

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

        public ICommand InverterInFaultCommand => this.inverterInFaultCommand ?? (this.inverterInFaultCommand = new DelegateCommand(() => this.ExecuteInverterInFaultCommand()));

        public InverterRole InverterRole => (InverterRole)this.Id;

        public InverterType InverterType
        {
            get => this.inverterType;
            set => this.inverterType = value;
        }

        public bool IsFault
        {
            get
            {
                return (this.statusWord & 0x0008) > 0;
            }
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

        public bool IsHorizontalAxis => (this.ControlWord & 0x8000) > 0;

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

        public bool IsShutterClosed => this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value && this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value;

        public bool IsShutterOpened => !this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorA].Value && !this.DigitalIO[(int)InverterSensors.AGL_ShutterSensorB].Value;

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

        public InverterOperationMode OperationMode
        {
            get => this.operationMode;
            set => this.SetProperty(ref this.operationMode, value);
        }

        public int SpeedRate { get; set; }

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

        public Dictionary<Axis, int> TargetAcceleration { get; set; }

        public Dictionary<Axis, int> TargetDeceleration { get; set; }

        public Dictionary<Axis, int> TargetPosition { get; set; }

        public int TargetShutterPosition { get; set; }

        public Dictionary<Axis, int> TargetSpeed { get; set; }

        private int homingTickCount { get; set; }

        private bool homingTimerActive { get; set; }

        private int shutterTickCount { get; set; }

        private bool shutterTimerActive { get; set; }

        private int targetTickCount { get; set; }

        private bool targetTimerActive { get; set; }

        #endregion

        #region Methods

        public void BuildHomingStatusWord()
        {
            //StartHoming
            if ((this.ControlWord & 0x0010) > 0)
            {
                if (!this.homingTimerActive)
                {
                    this.homingTimer.Change(0, 500);
                    this.homingTimerActive = true;
                    //this.AxisPosition = 0;
                }
            }
            else
            {
                this.StatusWord &= 0xEFFF;
            }

            //Fault Reset
            if ((this.ControlWord & 0x0080) > 0)
            {
                this.StatusWord &= 0xFFBF;
            }

            //Halt
            if ((this.ControlWord & 0x0100) > 0)
            {
            }
        }

        public void BuildPositionStatusWord()
        {
            //New SetPoint
            if ((this.ControlWord & 0x0010) > 0)
            {
                if (!this.targetTimerActive && !this.positionReached)
                {
                    this.StatusWord &= 0xFBFF;

                    this.targetTimer.Change(0, 500);
                    this.targetTimerActive = true;

                    //this.AxisPosition = 0;
                }
            }
            else
            {
                if (this.targetTimerActive)
                {
                    this.targetTimer.Change(-1, Timeout.Infinite);
                    // Reset contatore
                    this.targetTickCount = 0;

                    this.targetTimerActive = false;
                }
                this.StatusWord &= 0xEFFF;
            }
        }

        public void BuildVelocityStatusWord()
        {
            //EnableOperation
            if ((this.ControlWord & 0x0008) > 0)
            {
                this.StatusWord |= 0x0004;
                if (!this.shutterTimerActive)
                {
                    this.shutterTimer.Change(0, 500);
                    this.shutterTimerActive = true;
                }
            }
            else
            {
                if (this.shutterTimerActive)
                {
                    this.shutterTimer.Change(-1, Timeout.Infinite);
                    // Reset contatore
                    this.shutterTickCount = 0;

                    this.shutterTimerActive = false;
                }
                this.StatusWord &= ~0x0400;
            }
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

        public void HomingTick(object state)
        {
            this.homingTickCount++;
            this.AxisPosition++;

            if (this.homingTickCount > 10)
            {
                this.StatusWord |= 0x1000;
                this.homingTimerActive = false;
                this.homingTickCount = 0;
                this.homingTimer.Change(-1, Timeout.Infinite);
            }
            else if (this.homingTickCount == 1)
            {
                this.StatusWord &= 0xEFFF;
            }
        }

        public BitModel[] RefreshControlWordArray()
        {
            var cw = (from x in Enumerable.Range(0, 16)
                      let binary = Convert.ToString(this.ControlWord, 2).PadLeft(16, '0')
                      select new { Value = binary[x] == '1' ? true : false, Description = (15 - x).ToString(), Index = (15 - x) })
                     .Select(x => new BitModel(x.Index.ToString("00"), x.Value, GetControlWordSignalDescription(this.OperationMode, x.Index))).Reverse().ToArray();

            if (this.controlWordArray != null)
            {
                for (int i = 0; i < cw.Length; i++)
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
                    return "Switch On";

                case 1:
                    return "Enable Voltage";

                case 2:
                    return "Quick Stop (Low Active)";

                case 3:
                    return "Enable Operation";

                case 4:
                    return operationMode == InverterOperationMode.Velocity ? "Rfg enable" : operationMode == InverterOperationMode.Position ? "New set-point" : operationMode == InverterOperationMode.Homing ? "Homing operation started" : "Operation mode specific";

                case 5:
                    return operationMode == InverterOperationMode.Velocity ? "Rfg unlock" : operationMode == InverterOperationMode.Position ? "Change set immediately" : "Operation mode specific";

                case 6:
                    return operationMode == InverterOperationMode.Velocity ? "Rfg use ref" : operationMode == InverterOperationMode.Position ? "Abs/rel" : "Operation mode specific";

                case 7:
                    return "Reset Fault";

                case 8:
                    return "Halt";

                case 9:
                    return operationMode == InverterOperationMode.Position ? "Change on set-point" : "Operation mode specific";

                case 10:
                    return "Free";

                case 11:
                case 12:
                case 13:
                    return "Manufacturer specific";

                case 14:
                    return "HeartBeat";

                case 15:
                    return "Horizontal Axis";
            }

            return "Free";
        }

        internal static string GetInverterSignalDescription(InverterType inverterType, int signalIndex)
        {
            switch (signalIndex)
            {
                case 0:
                    return "Potenza ON";

                case 1:
                    return "Funzionamento normale";

                case 2:
                    return inverterType == InverterType.Ang ? "Posizione di zero elevatore" : inverterType == InverterType.Agl ? "Sensore serranda (A)" : "Posizione di zero";

                case 3:
                    return inverterType == InverterType.Ang ? "Encoder canale B --- culla" : inverterType == InverterType.Agl ? "Sensore serranda (B)" : "Encoder canale B";

                case 4:
                    return inverterType == InverterType.Ang ? "Encoder canale A --- culla" : inverterType == InverterType.Agl ? "Libero" : "Encoder canale A";

                case 5:
                    return inverterType == InverterType.Ang ? "Encoder canale Z --- culla" : inverterType == InverterType.Agl ? "Libero" : "Encoder canale Z";

                case 6:
                    return inverterType == InverterType.Ang ? "Extracorsa elevatore" : inverterType == InverterType.Agl ? "Libero" : "Libero";

                case 7:
                    return inverterType == InverterType.Ang ? "Sensore zero culla" : inverterType == InverterType.Agl ? "Libero" : "Libero";

                default:
                    return string.Empty;
            }
        }

        internal static string GetStatusWordSignalDescription(InverterOperationMode operationMode, int signalIndex)
        {
            switch (signalIndex)
            {
                case 0:
                    return "Ready to switch on";

                case 1:
                    return "Switched on";

                case 2:
                    return "Operation Enabled";

                case 3:
                    return "Fault";

                case 4:
                    return "Voltage Enabled";

                case 5:
                    return "Quick Stop (Low active)";

                case 6:
                    return "Switch on disabled";

                case 7:
                    return "Warning";

                case 8:
                    return "Manufacturer specific";

                case 9:
                    return "Remote";

                case 10:
                    return "Target reached";

                case 11:
                    return "Internal limit active";

                case 12:
                    return operationMode == InverterOperationMode.ProfileVelocity ? "Velocity" : operationMode == InverterOperationMode.Position ? "Set-point acknowledge" : operationMode == InverterOperationMode.Homing ? "Homing attained" : "Operation mode specific";

                case 13:
                    return operationMode == InverterOperationMode.ProfileVelocity ? "Max slippage" : operationMode == InverterOperationMode.Position ? "Following error" : operationMode == InverterOperationMode.Homing ? "Homing error" : "Operation mode specific";

                case 14:
                    return "Manufacturer specific";

                case 15:
                    return "Manufacturer specific Warning 2";
            }

            return "Free";
        }

        private void ExecuteInverterInFaultCommand()
        {
            this.IsFault = !this.IsFault;
        }

        private void ShutterTick(object state)
        {
            this.shutterTickCount++;
            if (this.TargetShutterPosition == (int)ShutterPosition.Opened)
            {
                this.AxisPosition++;
            }
            else
            {
                this.AxisPosition--;
            }

            if (this.shutterTickCount > 100
                || (this.TargetShutterPosition == (int)ShutterPosition.Closed && this.IsShutterClosed)
                || (this.TargetShutterPosition == (int)ShutterPosition.Opened && this.IsShutterOpened)
                )
            {
                this.ControlWord &= 0xFFEF;
                this.StatusWord |= 0x0400;

                this.shutterTimer.Change(-1, Timeout.Infinite);
                // Reset contatore
                this.shutterTickCount = 0;

                this.shutterTimerActive = false;
                this.positionReached = true;
            }
        }

        private void TargetTick(object state)
        {
            this.targetTickCount++;
            if (this.TargetPosition[this.currentAxis] > this.AxisPosition)
            {
                this.AxisPosition++;
            }
            else
            {
                this.AxisPosition--;
            }

            if (Math.Abs(this.TargetPosition[this.currentAxis] - this.AxisPosition) == 0 || this.targetTickCount > 100)
            {
                this.ControlWord &= 0xFFEF;
                this.StatusWord |= 0x0400;

                this.targetTimer.Change(-1, Timeout.Infinite);
                // Reset contatore
                this.targetTickCount = 0;

                this.targetTimerActive = false;
                this.positionReached = true;
            }
        }

        #endregion
    }
}
