namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    public enum MessageStatus
    {
        NotSpecified,

        OperationEnd,

        OperationError,

        OperationStop,

        OperationFaultStop,

        OperationRunningStop,

        OperationStart,

        /// <summary>
        /// Using this state when change data on db
        /// </summary>
        OperationUpdateData,

        OperationExecuting,

        OperationStepStart,

        OperationStepEnd,

        OperationStepStop,

        OperationWaitResume,

        OperationInverterFault,
    }
}
