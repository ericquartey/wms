namespace Ferretto.VW.MAS.DataModels.Enumerations
{
    public enum ShutterManualMovements : long
    {
        Undefined = 0L,

        FeedRate = 1L,

        MaxSpeed = 2L,

        MinSpeed = 4L,

        Acceleration = 5L,

        Deceleration = 6L,

        HigherDistance = 7L,

        LowerDistance = 8L,
    }
}
