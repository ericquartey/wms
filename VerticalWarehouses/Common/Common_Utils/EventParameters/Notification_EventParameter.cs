namespace Ferretto.VW.Common_Utils.EventParameters
{
    public enum OperationStatus
    {
        Error,

        End,

        Stopped
    }

    public enum OperationType
    {
        Homing,

        SwitchVerticalToHorizontal,

        SwitchHorizontalToVertical,

        Positioning
    }

    public enum Verbosity
    {
        Debug,

        Info
    }

    public class Notification_EventParameter
    {
        #region Constructors

        public Notification_EventParameter()
        {
        }

        public Notification_EventParameter(OperationType operationType, OperationStatus operationStatus, string description, Verbosity verbosity)
        {
            this.OperationType = operationType;
            this.OperationStatus = operationStatus;
            this.Description = description;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public string Description { get; set; }

        public OperationStatus OperationStatus { get; set; }

        public OperationType OperationType { get; set; }

        public Verbosity Verbosity { get; set; }

        #endregion
    }
}
