namespace Ferretto.VW.Common_Utils.Messages.Enumerations
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
