using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface IUserProvider : IBusinessProvider<User, User>
    {
        #region Methods

        string IsValid(User user);

        #endregion Methods
    }
}
