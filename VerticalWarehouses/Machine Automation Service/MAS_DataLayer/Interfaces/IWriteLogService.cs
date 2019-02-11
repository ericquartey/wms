using Ferretto.VW.Common_Utils.EventParameters;

namespace Ferretto.VW.MAS_DataLayer
{
    public interface IWriteLogService
    {
        #region Methods

        void LogWriting(Command_EventParameter command_EventParameter);

        #endregion Methods
    }
}
