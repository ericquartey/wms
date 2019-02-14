using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IUserProvider : IBusinessProvider<User, User, int>
    {
        #region Methods

        string IsValid(User user);

        #endregion
    }
}
