namespace Ferretto.VW.MAS.InverterDriver.Contracts
{
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

        StatusWordParam = 411, //19B INFO:Readonly

        ActualPositionShaft = 1108,

        StatusDigitalSignals = 250,

        DigitalInputsOutputs = 1411,

        ShutterTargetPosition = 414, // 19E

        ShutterAbsoluteEnable = 458,

        ShutterAbsoluteRevs = 460,

        ShutterTargetVelocityParam = 480,

        TableTravelTargetPosition = 1202,

        TableTravelTargetSpeeds = 1203,

        TableTravelTargetAccelerations = 1204,

        TableTravelTargetDecelerations = 1206,

        TableTravelSwitchPositions = 1209,

        TableTravelDirection = 1261,

        TorqueCurrent = 211,
    }
}
