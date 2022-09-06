using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IUsersProvider
    {
        #region Methods

        int? Authenticate(string userName, string password, string supportToken);

        UserParameters Authenticate(string cardToken);

        void ChangePassword(string userName, string newPassword);

        IEnumerable<UserParameters> GetAllUserWithCulture();

        bool GetIsDisabled(string userName);

        bool GetIsLimited(string userName);

        string GetServiceToken();

        bool IsOperatorEnabledWithWMS();

        void SetIsDisabled(string userName, bool isDisabled);

        void SetIsLimited(string userName, bool isLimited);

        void SetOperatorEnabledWithWMS(bool isEnabled);

        void SetUserCulture(string culture, string name);

        #endregion
    }
}
