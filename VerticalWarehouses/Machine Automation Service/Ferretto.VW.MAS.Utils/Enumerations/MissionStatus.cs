namespace Ferretto.VW.MAS.Utils.Enumerations
{
    /// <summary>
    /// Mission Status see"https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskstatus?view=netframework-4.8 for values definition.
    /// </summary>
    public enum MissionStatus
    {
        Created = 0,

        WaitingForActivation,

        WaitingToRun,

        Running,

        WaitingForChildrenToComplete,

        RanToCompletion,

        Canceled,

        Faulted,
    }
}
