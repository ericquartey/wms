namespace Ferretto.VW.Installer
{
    internal enum StepStatus
    {
        ToDo,

        InProgress,

        Done,

        Failed,

        RollingBack,

        RolledBack,

        RollbackFailed
    }
}
