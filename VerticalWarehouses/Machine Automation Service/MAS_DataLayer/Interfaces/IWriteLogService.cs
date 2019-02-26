using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_DataLayer
{
    public interface IWriteLogService
    {
        #region Methods

        void LogWriting(CommandMessage command_EventParameter);

        #endregion Methods
    }
}
