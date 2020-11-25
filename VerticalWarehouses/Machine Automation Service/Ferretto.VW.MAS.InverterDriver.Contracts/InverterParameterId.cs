namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public enum InverterParameterId : short
    {
        ControlWord = 410,

        HomingCalibration = 1130,

        HomingFastSpeed = 1132,

        HomingCreepSpeed = 1133,

        HomingAcceleration = 1134,

        HomingOffset = 1185,

        HomingSensor = 1139,

        PositionAcceleration = 1457,

        PositionDeceleration = 1458,

        PositionTargetPosition = 1455,

        PositionTargetSpeed = 1456,

        SetOperatingMode = 1454,

        StatusWord = 411,

        ActualPositionShaft = 1108,

        StatusDigitalSignals = 250,

        DigitalInputsOutputs = 1411,

        ShutterTargetPosition = 414,

        ShutterTargetVelocity = 480,

        ShutterLowVelocity = 481,

        ShutterHighVelocityDuration = 583,

        TableTravelTableIndex = 1200,

        TableTravelTargetPosition = 1202,

        TableTravelTargetSpeeds = 1203,

        TableTravelTargetAccelerations = 1204,

        TableTravelTargetDecelerations = 1206,

        TableTravelDirection = 1261,

        TorqueCurrent = 211,

        ProfileInput = 251,

        BrakeReleaseTime = 625,

        BrakeActivatePercent = 637,

        CurrentError = 260,

        BlockDefinition = 17,

        BlockWrite = 18,

        BlockRead = 19,

        HeartBeatTimer1 = 1390,

        SoftwareVersion = 12,

        AxisChanged = 520,
    }
}
