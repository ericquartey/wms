namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum ConditionToCheckType
    {
        MachineIsInEmergencyState = 0,

        DrawerIsCompletelyOnCradle,

        DrawerIsPartiallyOnCradle,

        SensorInZeroOnCradle,

        SensorInZeroOnElevator,
    }
}
