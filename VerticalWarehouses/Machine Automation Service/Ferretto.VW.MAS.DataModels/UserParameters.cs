using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class UserParameters : DataModel
    {
        #region Properties

        public int AccessLevel { get; set; }

        /// <summary>
        /// Movement user is normally disabled, only Admin can enable
        /// </summary>
        public bool IsDisabled { get; set; }

        public bool IsDisabledWithWMS { get; set; }

        /// <summary>
        /// local user cannot call Load Units
        /// </summary>
        public bool IsLimited { get; set; }

        public bool IsService => string.CompareOrdinal(this.Name, "service") == 0;

        public string Language { get; set; }

        public string Name { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        /// <summary>
        /// Local users do not appear in the login list if they have the token
        /// </summary>
        public string Token { get; set; }

        public DateTimeOffset Validity { get; set; }

        #endregion

        #region Classes

        public static class Values
        {
            #region Fields

            public static readonly UserParameters Admin = new UserParameters
            {
                Id = -99,
                Name = "admin",
                AccessLevel = 99,
                PasswordHash = "cbRN86CITnhRJ0jen38G9s9KZf7YNHirJfI0FP6qDbo=",
                PasswordSalt = "7T4XrBJfRzWtyFNGrXFlsw==",
                Language = "it-IT",
                Validity = DateTime.Now,
            };

            public static readonly UserParameters Guest = new UserParameters
            {
                Id = -5,
                Name = "guest",
                AccessLevel = 1,
                PasswordHash = "e1IrRSpcUNLIQAmdtSzQqrKT4DLcMaYMh662pgMh2xY=",
                PasswordSalt = "iB+IdMnlzvXvitHWJff38A==",
                Language = "it-IT",
                Validity = DateTime.Now,
                IsDisabled = true,
                IsLimited = true,
            };

            public static readonly UserParameters Installer = new UserParameters
            {
                Id = -2,
                Name = "installer",
                AccessLevel = 3,
                PasswordHash = "L3I3og8ZVak4fQVi8LRyXld3kKH+TK5TJ5/usnNXij4=",
                PasswordSalt = "obChaz6W7brGMtT7Dn7TAw==",
                Language = "it-IT",
                Validity = DateTime.Now,
            };

            public static readonly UserParameters Movement = new UserParameters
            {
                Id = -4,
                Name = "movement",
                AccessLevel = 2,
                PasswordHash = "+YrT7Qjs8rILy+SmYY8Ddd8nZ1JUJhtDoYdMefd5Ed4=",
                PasswordSalt = "iB+IdMnlzvXvitHWJff38A==",
                Language = "it-IT",
                Validity = DateTime.Now,
                IsDisabled = true,
            };

            public static readonly UserParameters Operator = new UserParameters
            {
                Id = -1,
                Name = "operator",
                AccessLevel = 1,
                PasswordHash = "e1IrRSpcUNLIQAmdtSzQqrKT4DLcMaYMh662pgMh2xY=",
                PasswordSalt = "iB+IdMnlzvXvitHWJff38A==",
                Language = "it-IT",
                Validity = DateTime.Now,
            };

            public static readonly UserParameters Service = new UserParameters
            {
                Id = -3,
                Name = "service",
                AccessLevel = 99,
                PasswordHash = "",
                PasswordSalt = "",
                Language = "it-IT",
                Validity = DateTime.Now,
            };

            #endregion
        }

        #endregion
    }
}
