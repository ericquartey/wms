using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ILogoutSettingsProvider
    {
        #region Method

        void AddLogoutSettings(LogoutSettings logoutSettings);

        void AddOrModifyLogoutSettings(LogoutSettings logoutSettings);

        IEnumerable<LogoutSettings> GetAllLogoutSettings();

        void ModifyLogoutSettings(LogoutSettings newLogoutSettings);

        void RemoveLogoutSettingsById(int id);

        void ResetRemainingTime(int id);

        void UpdateStatus(double minutes);

        #endregion
    }
}
