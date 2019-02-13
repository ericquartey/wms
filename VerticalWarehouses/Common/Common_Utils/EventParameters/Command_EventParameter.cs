namespace Ferretto.VW.Common_Utils.EventParameters
{
    public enum CommandType
    {
        ExecuteHoming,

        ExecuteStopHoming
    }

    public class Command_EventParameter
    {
        #region Constructors

        public Command_EventParameter(CommandType commandType)
        {
            this.CommandType = commandType;
        }

        #endregion

        #region Properties

        public CommandType CommandType { get; set; }

        #endregion
    }
}
