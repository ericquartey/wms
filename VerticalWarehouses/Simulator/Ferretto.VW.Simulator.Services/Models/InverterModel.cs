using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.Simulator.Services.Models
{
    public enum InverterType
    {
        Undefined,
        Ang,
        Agl,
        Acu
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

    public enum InverterOperationMode : ushort
    {
        Position = 0x0001,

        Homing = 0x0006,

        Velocity = 0x0002,

        ProfileVelocity = 0x0003
    }

    public enum InverterSensors
    {
        #region INFO ANG Inverter Inputs

        /// <summary>
        /// S1IND-STO (hardware)
        /// </summary>
        ANG_HardwareSensorSTOA = 0,

        /// <summary>
        /// S2IND-SS1 (hardware)
        /// </summary>
        ANG_HardwareSensorSS1 = 1,

        /// <summary>
        /// S3IND-Sensore zero elevatore
        /// </summary>
        ANG_ZeroElevatorSensor = 2,

        /// <summary>
        /// S4IND-Extracorsa elevatore
        /// </summary>
        ANG_OverrunElevatorSensor = 3,

        /// <summary>
        /// S5IND-Taratura barriera
        /// </summary>
        ANG_BarrierCalibration = 4,

        /// <summary>
        /// S6IND-Sensore zero culla
        /// </summary>
        ANG_ZeroCradleSensor = 5,

        /// <summary>
        /// S7IND-STO (hardware)
        /// </summary>
        ANG_HardwareSensorSTOB = 6,

        /// <summary>
        /// MFI1-Barriera ottica di misura
        /// </summary>
        ANG_OpticalMeasuringBarrier = 7,

        /// <summary>
        /// MF2-Presenza cassetto su culla lato macchina
        /// </summary>
        ANG_PresenceDrawerOnCradleMachineSide = 8,

        /// <summary>
        /// MF3-Presenza cassetto su culla lato operatore
        /// </summary>
        ANG_PresenceDraweronCradleOperatoreSide = 9,

        /// <summary>
        /// MF4-Temperatura motore elevatore
        /// </summary>
        ANG_ElevatorMotorTemprature = 10,

        #endregion

        #region INFO AGL Inverter Inputs

        /// <summary>
        /// STO (hardware)
        /// </summary>
        AGL_HardwareSensorSTOA = 0, //11,

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

        #region INFO ACU Inverter Inputs // TODO: Temporary values

        /// <summary>
        /// S1IND-STO (hardware)
        /// </summary>
        ACU_HardwareSensorSTOA = 20,

        /// <summary>
        /// S2IND-SS1 (hardware)
        /// </summary>
        ACU_HardwareSensorSS1 = 21,

        /// <summary>
        /// S3IND-Sensore zero
        /// </summary>
        ACU_ZeroSensor = 22,

        /// <summary>
        /// S4IND-Encoder canale B
        /// </summary>
        ACU_EncoderCanalB = 23,

        /// <summary>
        /// S5IND-Encoder canale A
        /// </summary>
        ACU_EncoderCanalA = 24,

        /// <summary>
        /// S6IND-Libero
        /// </summary>
        ACU_FreeSensor1 = 25,

        /// <summary>
        /// S7IND-STO (hardware)
        /// </summary>
        ACU_HardwareSensorSTOB = 26,

        /// <summary>
        /// MF1-Libero
        /// </summary>
        ACU_FreeSensor2 = 27,

        #endregion
    }

    public class InverterModel
    {
        private InverterType inverterType;
        private readonly Timer homingTimer;
        private readonly Timer targetTimer;

        public InverterModel()
        {
            this.homingTimer = new Timer(this.HomingTick, null, -1, Timeout.Infinite);

            this.homingTimerActive = false;
            this.targetTimer = new Timer(this.TargetTick, null, -1, Timeout.Infinite);

            this.targetTimerActive = false;

            this.OperationMode = InverterOperationMode.Velocity;

            this.DigitalIO = new bool[8];
            this.DigitalIO[0] = true;

        }

        public int Id { get; set; }

        public InverterRole InverterRole => (InverterRole)this.Id;

        public InverterType InverterType
        {
            get { return this.inverterType; }
            set { this.inverterType = value; }
        }

        public InverterOperationMode OperationMode { get; set; }

        public int ControlWord { get; set; }

        public int StatusWord { get; set; }

        public bool[] DigitalIO { get; set; }

        public int GetDigitalIO()
        {
            int result = 0;
            for (int i = 0; i < this.DigitalIO.Length; i++)
            {
                if (this.DigitalIO[i])
                {
                    result += (int)Math.Pow(2, i);
                }
            }
            return result;
        }

        public int AxisPosition { get; set; }

        private int homingTickCount { get; set; }

        private bool homingTimerActive { get; set; }

        private int targetTickCount { get; set; }

        private bool targetTimerActive { get; set; }


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
                    this.homingTimer.Change(0, 1000);
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
                    this.targetTimer.Change(0, 1000);
                    this.targetTimerActive = true;
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

        public void HomingTick(object state)
        {
            this.homingTickCount++;

            if (this.homingTickCount > 10)
            {
                this.StatusWord |= 0x1000;
                this.homingTimerActive = false;
                this.homingTimer.Change(-1, Timeout.Infinite);
            }
        }

        private void TargetTick(object state)
        {
            this.targetTickCount++;

            if (this.targetTickCount > 10)
            {
                this.StatusWord |= 0x0400;
                this.targetTimerActive = false;
                this.targetTimer.Change(-1, Timeout.Infinite);
                // Reset contatore
                this.targetTickCount = 0;
            }
        }

    }
}
