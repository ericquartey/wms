namespace Ferretto.VW.Installer.Core
{
    public enum StepStatus
    {
        /// <summary>
        /// The step still needs to be executed.
        /// </summary>
        ToDo,

        /// <summary>
        /// The step is the next in line to be executed.
        /// </summary>
        Start,

        /// <summary>
        /// The step is being executed.
        /// </summary>
        InProgress,

        /// <summary>
        /// The step completed successfully.
        /// </summary>
        Done,

        /// <summary>
        /// The step completed with errors.
        /// </summary>
        Failed,

        /// <summary>
        /// The step is being rolled back.
        /// </summary>
        RollingBack,

        /// <summary>
        /// The step was rolled back.
        /// </summary>
        RolledBack,

        /// <summary>
        /// The rollback of the step failed.
        /// </summary>
        RollbackFailed
    }
}
