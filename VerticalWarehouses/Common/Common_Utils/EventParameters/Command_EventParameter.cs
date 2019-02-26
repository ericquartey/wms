namespace Ferretto.VW.Common_Utils.EventParameters
{
    public enum CommandType
    {
        ExecuteHoming,

        ExecuteStopHoming,

        StopAction
    }

    public class Command_EventParameterOld
    {
        #region Constructors

        public Command_EventParameterOld(CommandType commandType)
        {
            this.CommandType = commandType;
        }

        #endregion

        #region Properties

        public CommandType CommandType { get; set; }

        #endregion
    }
}
