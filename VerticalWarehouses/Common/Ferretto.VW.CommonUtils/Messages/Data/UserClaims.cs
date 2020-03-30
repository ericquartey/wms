using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class UserClaims
    {
        #region Properties

        public UserAccessLevel AccessLevel { get; set; }

        public string Name { get; set; }

        #endregion
    }
}
