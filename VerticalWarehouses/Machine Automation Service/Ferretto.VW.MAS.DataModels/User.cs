namespace Ferretto.VW.MAS.DataModels
{
    public class User
    {
        #region Properties

        public int AccessLevel { get; set; }

        public string Name { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        #endregion
    }
}
