namespace Ferretto.VW.MAS.DataModels
{
    public sealed class User : DataModel
    {
        #region Properties

        public int AccessLevel { get; set; }

        public string Name { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        #endregion
    }
}
