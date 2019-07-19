namespace Ferretto.VW.MAS.DataModels
{
    public enum VerticalAxis : long
    {
        Undefined = 0L,

        MaxEmptySpeed = 1L,

        MaxEmptyAcceleration = 2L,

        MaxEmptyDeceleration = 3L,

        MinFullSpeed = 18L,

        MaxFullAcceleration = 19L,

        MaxFullDeceleration = 20L,

        HomingExecuted = 4L,

        HomingSearchDirection = 5L,

        HomingSearchSpeed = 6L,

        HomingSearchAcceleration = 7L,

        HomingSearchDeceleration = 8L,

        HomingExitSpeed = 9L,

        HomingExitAcceleration = 10L,

        HomingExitDeceleration = 11L,

        Resolution = 12L,

        Offset = 13L,

        UpperBound = 14L,

        LowerBound = 15L,

        TakingOffset = 16L,

        DepositOffset = 17L,
    }
}
