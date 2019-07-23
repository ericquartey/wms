namespace Ferretto.VW.MAS.DataModels
{
    public enum HorizontalAxis : long
    {
        Undefined = 0L,

        MaxEmptySpeed = 1L,

        MaxEmptyAcceleration = 2L,

        MaxEmptyDeceleration = 3L,

        MaxFullSpeed = 9L,

        MaxFullAcceleration = 10L,

        MaxFullDeceleration = 11L,

        HomingExecuted = 4L,

        Resolution = 8L,

        Offset = 5L,

        ClockWiseRun = 6L,

        AntiClockWiseRun = 7L,
    }
}
