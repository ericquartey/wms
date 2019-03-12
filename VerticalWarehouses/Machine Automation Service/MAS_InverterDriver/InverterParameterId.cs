namespace Ferretto.VW.MAS_InverterDriver
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

        StatusWordParam = 411, //INFO:Readonly

        ActualPositionShaft = 1108,

        StatusDigitalSignals = 250,
    }
}
