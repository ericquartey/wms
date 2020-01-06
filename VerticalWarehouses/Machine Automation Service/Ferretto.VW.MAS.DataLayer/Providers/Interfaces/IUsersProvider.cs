namespace Ferretto.VW.MAS.DataLayer
{
    public interface IUsersProvider
    {
        #region Methods

        int? Authenticate(string userName, string password, string supportToken);

        string GenerateSupportToken();

        #endregion
    }
}
