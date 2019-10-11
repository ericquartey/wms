namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
    public enum InverterParameterId : short
    {
        ControlWordParam = 410,

        HomingCalibration = 1130,

        HomingFastSpeedParam = 1132,

        HomingCreepSpeedParam = 1133,

        HomingAcceleration = 1134,

        HomingOffset = 1185,

        HomingSensor = 1139,

        PositionAccelerationParam = 1457,

        PositionDecelerationParam = 1458,

        PositionTargetPositionParam = 1455,

        PositionTargetSpeedParam = 1456,

        SetOperatingModeParam = 1454,

        StatusWordParam = 411,

        ActualPositionShaft = 1108,

        StatusDigitalSignals = 250,

        DigitalInputsOutputs = 1411,

        ShutterTargetPosition = 414,

        ShutterTargetVelocityParam = 480,

        ShutterLowVelocity = 481,

        ShutterHighVelocityDuration = 583,

        TableTravelTableIndex = 1200,

        TableTravelTargetPosition = 1202,

        TableTravelTargetSpeeds = 1203,

        TableTravelTargetAccelerations = 1204,

        TableTravelTargetDecelerations = 1206,

        TableTravelDirection = 1261,

        TorqueCurrent = 211,

        BrakeReleaseTime = 625,

        BrakeActivatePercent = 637,
    }
}
