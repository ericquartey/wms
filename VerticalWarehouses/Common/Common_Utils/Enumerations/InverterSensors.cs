﻿namespace Ferretto.VW.Common_Utils.Enumerations
{
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

        #region INFO ACU Inverter Inputs

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
}
