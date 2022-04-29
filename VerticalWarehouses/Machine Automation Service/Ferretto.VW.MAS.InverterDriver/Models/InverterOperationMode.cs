namespace Ferretto.VW.MAS.InverterDriver.Enumerations
{
    public enum InverterOperationMode : ushort
    {
        Position = 1,

        Homing = 6,

        Velocity = 2,

        ProfileVelocity = 3,

        Nord = 10,

        SlaveGear = 253,

        LeaveLimitSwitch = 254,

        TableTravel = 255,
    }
}
