using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_DataLayer
{
    public interface IWriteLogService
    {
        #region Methods

        bool LogWriting(string logMessage);

        void LogWriting(WebAPI_Action webApiAction);

        #endregion Methods
    }
}
