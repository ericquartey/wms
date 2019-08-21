namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IUsersProvider
    {
        #region Methods

        int? Authenticate(string userName, string password);

        #endregion
    }
}
