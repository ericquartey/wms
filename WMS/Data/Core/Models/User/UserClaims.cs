using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.Data.Core.Models
{
    public class UserClaims
    {
        public string Name { get; set; }

        public Enums.UserAccessLevel AccessLevel { get; set; }
    }
}
