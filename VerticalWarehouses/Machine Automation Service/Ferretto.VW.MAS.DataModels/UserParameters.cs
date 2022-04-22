using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class UserParameters : DataModel
    {
        #region Properties

        public int AccessLevel { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsDisabledWithWMS { get; set; }

        public bool IsService => string.CompareOrdinal(this.Name, "service") == 0;

        public string Language { get; set; }

        public string Name { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

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
                PasswordHash = "8k2DQfWKVZ2FAX+miwsVYN+RSiQZ/dxRO1IcRAC+TvA=",
                PasswordSalt = "7T4XrBJfRzWtyFNGrXFlsw==",
                Language = "it-IT",
                Validity = DateTime.Now,
            };

            public static readonly UserParameters Installer = new UserParameters
            {
                Id = -2,
                Name = "installer",
                AccessLevel = 3,
                PasswordHash = "RFzfGJR1H1hAi+t4eamhB1O0saoZkbKg3tWLkhlWiqs=",
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
