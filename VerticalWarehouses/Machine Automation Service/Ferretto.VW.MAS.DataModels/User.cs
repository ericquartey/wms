using System;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class User : DataModel
    {
        #region Properties

        public int AccessLevel { get; set; }

        public bool IsService => string.CompareOrdinal(this.Name, "service") == 0;

        public string Name { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public DateTime Validity { get; set; }

        #endregion

        #region Classes

        public static class Values
        {
            #region Fields

            public static readonly User Admin = new User
            {
                Id = -99,
                Name = "admin",
                AccessLevel = 99,
                PasswordHash = "e1IrRSpcUNLIQAmdtSzQqrKT4DLcMaYMh662pgMh2xY=",
                PasswordSalt = "iB+IdMnlzvXvitHWJff38A==",
            };

            public static readonly User Installer = new User
            {
                Id = -2,
                Name = "installer",
                AccessLevel = 2,
                PasswordHash = "DsWpG30CTZweMD4Q+LlgzrsGOWM/jx6enmP8O7RIrvU=",
                PasswordSalt = "2xw+hMIYBtLCoUqQGXSL0A==",
            };

            public static readonly User Operator = new User
            {
                Id = -1,
                Name = "operator",
                AccessLevel = 1,
                PasswordHash = "e1IrRSpcUNLIQAmdtSzQqrKT4DLcMaYMh662pgMh2xY=",
                PasswordSalt = "iB+IdMnlzvXvitHWJff38A==",
            };

            public static readonly User Service = new User
            {
                Id = -3,
                Name = "service",
                AccessLevel = 2,
                PasswordHash = "",
                PasswordSalt = "",
            };

            #endregion
        }

        #endregion
    }
}
