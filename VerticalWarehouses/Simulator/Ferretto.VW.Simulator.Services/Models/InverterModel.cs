using System;
using System.Collections.ObjectModel;
using System.Threading;
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

        private readonly Timer homingTimer;

        private readonly Timer targetTimer;

        private int axisPosition;

        private int controlWord;

        private ObservableCollection<BitModel> digitalIO = new ObservableCollection<BitModel>();

        private bool enabled;

        private InverterType inverterType;

        private bool positionReached;

        private int statusWord;

        #endregion

        #region Constructors

        public InverterModel()
        {
            this.homingTimer = new Timer(this.HomingTick, null, -1, Timeout.Infinite);

            this.homingTimerActive = false;
            this.targetTimer = new Timer(this.TargetTick, null, -1, Timeout.Infinite);

            this.targetTimerActive = false;

            this.OperationMode = InverterOperationMode.Velocity;

            this.digitalIO.Add(new BitModel("Bit 00", false));
            this.digitalIO.Add(new BitModel("Bit 01", false));
            this.digitalIO.Add(new BitModel("Bit 02", false));
            this.digitalIO.Add(new BitModel("Bit 03", false));
            this.digitalIO.Add(new BitModel("Bit 04", false));
            this.digitalIO.Add(new BitModel("Bit 05", false));
            this.digitalIO.Add(new BitModel("Bit 06", false));
            this.digitalIO.Add(new BitModel("Bit 07", false));
        }

        #endregion

        #region Properties

        public int AxisPosition { get => this.axisPosition; set => this.SetProperty(ref this.axisPosition, value, () => this.RaisePropertyChanged(nameof(this.AxisPosition))); }

        public int ControlWord
        {
            get => this.controlWord;
            set => this.SetProperty(ref this.controlWord, value, () =>
            {
                this.RaisePropertyChanged(nameof(this.ControlWord));
                this.RaisePropertyChanged(nameof(this.ControlWordBinary));
            });
        }

        public string ControlWordBinary => Convert.ToString(this.ControlWord, 2).PadLeft(16, '0');

        public ObservableCollection<BitModel> DigitalIO
        {
            get => this.digitalIO;
            set => this.SetProperty(ref this.digitalIO, value);
        }

        public bool Enabled { get => this.enabled; set => this.SetProperty(ref this.enabled, value, () => this.RaisePropertyChanged(nameof(this.Enabled))); }

        //public bool[] DigitalIO { get; set; }
        public int Id { get; set; }

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
            }
        }

        public bool IsOperationEnabled => (this.statusWord & 0x0004) > 0;

        public bool IsQuickStopTrue => (this.statusWord & 0x0020) > 0;

        public bool IsReadyToSwitchOn => (this.statusWord & 0x0001) > 0;

        public bool IsRemote => (this.statusWord & 0x0200) > 0;

        public bool IsSwitchedOn
        {
            get => (this.statusWord & 0x0002) > 0;
            set => this.statusWord |= 0x0002;
        }

        public bool IsSwitchOnDisabled => (this.statusWord & 0x0040) > 0;

        public bool IsVoltageEnabled => (this.statusWord & 0x0010) > 0;

        public bool IsWarning => (this.statusWord & 0x0080) > 0;

        public bool IsWarning2 => (this.statusWord & 0x8000) > 0;

        public InverterOperationMode OperationMode { get; set; }

        public int StatusWord
        {
            get => this.statusWord;
            set => this.SetProperty(ref this.statusWord, value, () =>
            {
                this.RaisePropertyChanged(nameof(this.StatusWord));
                this.RaisePropertyChanged(nameof(this.StatusWordBinary));
            });
        }

        public string StatusWordBinary => Convert.ToString(this.StatusWord, 2).PadLeft(16, '0');

        private int homingTickCount { get; set; }

        private bool homingTimerActive { get; set; }

        private int targetTickCount { get; set; }

        private bool targetTimerActive { get; set; }

        #endregion

        #region Methods

        public void BuildHomingStatusWord()
        {
            //SwitchON
            if ((this.ControlWord & 0x0001) > 0)
            {
                this.StatusWord |= 0x0002;
            }
            else
            {
                this.StatusWord &= 0xFFFD;
            }

            //EnableVoltage
            if ((this.ControlWord & 0x0002) > 0)
            {
                this.StatusWord |= 0x0001;
                this.StatusWord |= 0x0010;
            }
            else
            {
                this.StatusWord &= 0xFFFE;
                this.StatusWord &= 0xFFEF;
            }

            //QuickStop
            if ((this.ControlWord & 0x0004) > 0)
            {
                this.StatusWord |= 0x0020;
            }
            else
            {
                this.StatusWord &= 0xFFDF;
            }

            //EnableOperation
            if ((this.ControlWord & 0x0008) > 0)
            {
                this.StatusWord |= 0x0004;
            }
            else
            {
                this.StatusWord &= 0xFFFB;
            }

            //StartHoming
            if ((this.ControlWord & 0x0010) > 0)
            {
                if (!this.homingTimerActive)
                {
                    this.homingTimer.Change(0, 500);
                    this.homingTimerActive = true;
                    this.AxisPosition = 0;
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
            //SwitchON
            if ((this.ControlWord & 0x0001) > 0)
            {
                this.StatusWord |= 0x0002;
            }
            else
            {
                this.StatusWord &= 0xFFFD;
            }

            //EnableVoltage
            if ((this.ControlWord & 0x0002) > 0)
            {
                this.StatusWord |= 0x0001;
                this.StatusWord |= 0x0010;
            }
            else
            {
                this.StatusWord &= 0xFFFE;
                this.StatusWord &= 0xFFEF;
            }

            //QuickStop
            if ((this.ControlWord & 0x0004) > 0)
            {
                this.StatusWord |= 0x0020;
            }
            else
            {
                this.StatusWord &= 0xFFDF;
            }

            //EnableOperation
            if ((this.ControlWord & 0x0008) > 0)
            {
                this.StatusWord |= 0x0004;
            }
            else
            {
                this.StatusWord &= 0xFFFB;
                this.positionReached = false;
            }

            //New SetPoint
            if ((this.ControlWord & 0x0010) > 0)
            {
                if (!this.targetTimerActive && !this.positionReached)
                {
                    this.StatusWord &= 0xFBFF;

                    this.targetTimer.Change(0, 500);
                    this.targetTimerActive = true;
                    this.AxisPosition = 0;
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

        public void BuildVelocityStatusWord()
        {
            //SwitchON
            if ((this.ControlWord & 0x0001) > 0)
            {
                this.StatusWord |= 0x0002;
            }
            else
            {
                this.StatusWord &= 0xFFFD;
            }

            //EnableVoltage
            if ((this.ControlWord & 0x0002) > 0)
            {
                this.StatusWord |= 0x0001;
                this.StatusWord |= 0x0010;
            }
            else
            {
                this.StatusWord &= 0xFFFE;
                this.StatusWord &= 0xFFEF;
            }

            //QuickStop
            if ((this.ControlWord & 0x0004) > 0)
            {
                this.StatusWord |= 0x0020;
            }
            else
            {
                this.StatusWord &= 0xFFDF;
            }

            //EnableOperation
            if ((this.ControlWord & 0x0008) > 0)
            {
                this.StatusWord |= 0x0004;
                if (!this.targetTimerActive)
                {
                    this.targetTimer.Change(0, 500);
                    this.targetTimerActive = true;
                    this.AxisPosition = 0;
                }
            }
            else
            {
                this.StatusWord &= 0xFFFB;
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

            if (this.homingTickCount > 5)
            {
                this.StatusWord |= 0x1000;
                this.homingTimerActive = false;
                this.homingTickCount = 0;
                this.homingTimer.Change(-1, Timeout.Infinite);
            }
        }

        private void TargetTick(object state)
        {
            this.targetTickCount++;
            this.AxisPosition++;

            if (this.targetTickCount > 10)
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
