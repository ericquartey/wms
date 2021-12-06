using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IUsersProvider
    {
        #region Methods

        int? Authenticate(string userName, string password, string supportToken);

        void ChangePassword(string userName, string newPassword);

        IEnumerable<UserParameters> GetAllUserWithCulture();

        string GetServiceToken();

        void SetUserCulture(string culture, string name);

        #endregion
    }
}
