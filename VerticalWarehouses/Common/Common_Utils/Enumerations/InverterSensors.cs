namespace Ferretto.VW.Common_Utils.Enumerations
{
    public enum InverterSensors
    {
        //INFO ANG Inverter Inputs

        ANG_HardwareSensorSTOA = 0,                                    //S1IND-STO (hardware)

        ANG_HardwareSensorSS1 = 1,                                     //S2IND-SS1 (hardware)

        ANG_ZeroElevatorSensor = 2,                                    //S3IND-Sensore zero elevatore

        ANG_OverrunElevatorSensor = 3,                                 //S4IND-Extracorsa elevatore

        ANG_BarrierCalibration = 4,                                    //S5IND-Taratura barriera

        ANG_ZeroCradleSensor = 5,                                      //S6IND-Sensore zero culla

        ANG_HardwareSensorSTOB = 6,                                    //S7IND-STO (hardware)

        ANG_OpticalMeasuringBarrier = 7,                               //MFI1-Barriera ottica di misura

        ANG_PresenceDrawerOnCradleMachineSide = 8,                     //MF2-Presenza cassetto su culla lato macchina

        ANG_PresenceDraweronCradleOperatoreSide = 9,                   //MF3-Presenza cassetto su culla lato operatore

        ANG_ElevatorMotorTemprature = 10,                              //MF4-Temperatura motore elevatore

        //INFO AGL Inverter Inputs

        AGL_HardwareSensorSTOA = 0, //11,                                   //STO (hardware)

        AGL_HardwareSensorSS1 = 1, //12,                                    //IN1D-SS1 (hardware)

        AGL_ShutterSensorA = 2, //13,                                       //IN2D-Sensore serranda (A)

        AGL_ShutterSensorB = 3, //14,                                       //IN3D-Sensore serranda (B)

        AGL_FreeSensor1 = 4, //15,                                          //IN4D-Libero

        AGL_FreeSensor2 = 5, //16,                                          //IN5D-Libero

        AGL_FreeSensor3 = 6, //17,                                          //MFI1-Libero

        AGL_FreeSensor4 = 7, //18,                                          //MFI2-Libero

        AGL_HardwareSensorSTOB = 8, //19,                                   //STO (hardware)

        //INFO ACU Inverter Inputs

        ACU_HardwareSensorSTOA = 20,                                   //S1IND-STO (hardware)

        ACU_HardwareSensorSS1 = 21,                                    //S2IND-SS1 (hardware)

        ACU_ZeroSensor = 22,                                           //S3IND-Sensore zero

        ACU_EncoderCanalB = 23,                                        //S4IND-Encoder canale B

        ACU_EncoderCanalA = 24,                                        //S5IND-Encoder canale A

        ACU_FreeSensor1 = 25,                                          //S6IND-Libero

        ACU_HardwareSensorSTOB = 26,                                   //S7IND-STO (hardware)

        ACU_FreeSensor2 = 27                                           //MF1-Libero
    }
}
